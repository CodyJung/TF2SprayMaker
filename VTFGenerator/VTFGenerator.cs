using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ManagedSquish;

namespace ExplodingJelly.SprayGenerator
{
    public class VTFGenerator : IDisposable
    {
        private delegate void ProcessAsync(bool alpha, int maxSize);
        private ProcessAsync processDelegate;

        public delegate void ProcessingCompleteHandler(object sender, ProcessCompleteEventArgs e);
        public delegate void ProcessProgressHandler(object sender, ProcessProgressEventArgs e);
        public event ProcessingCompleteHandler ProcessingComplete;
        public event ProcessProgressHandler ProcessProgress;

        VTFOutput _output;
        private int _baseImageSize;
        private int _imageFrames;
        private Bitmap[] _frames;
        private Stream _largeInputFile;
        private Stream _smallInputFile;

        private byte _numMipmaps
        {
            get
            {
                return Convert.ToByte(Math.Log(_baseImageSize, 2) + 1);
            }
        }

        private int GetMipmapSize(int mipmapLevel)
        {
            return (int)(_baseImageSize / Math.Pow(2, mipmapLevel));
        }

        // http://stackoverflow.com/questions/364985/algorithm-for-finding-the-smallest-power-of-two-thats-greater-or-equal-to-a-giv
        private int pow2roundup(int x)
        {
            if (x < 0)
                return 0;
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x+1;
        }

        public VTFGenerator(Stream inputStream, Stream outputStream)
        {
            processDelegate = new ProcessAsync(this.Process);
            _output = new VTFOutput(outputStream);
            _smallInputFile = new MemoryStream();
            inputStream.CopyTo(_smallInputFile);
        }

        public VTFGenerator(Stream smallInputStream, Stream largeInputStream, Stream outputStream)
        {
            processDelegate = new ProcessAsync(this.Process);
            _output = new VTFOutput(outputStream);

            _smallInputFile = new MemoryStream();
            smallInputStream.CopyTo(_smallInputFile);

            _largeInputFile = new MemoryStream();
            largeInputStream.CopyTo(_largeInputFile);
        }


        public VTFGenerator(string inputFile, string outputFile)
        {
            processDelegate = new ProcessAsync(this.Process);
            // Create the output directories
            (new FileInfo(outputFile)).Directory.Create();
            _output = new VTFOutput(outputFile);

            _smallInputFile = new FileStream(inputFile, FileMode.Open);
        }

        public VTFGenerator(string smallInputFile, string largeInputFile, string outputFile)
        {
            processDelegate = new ProcessAsync(this.Process);
            // Create the output directories
            (new FileInfo(outputFile)).Directory.Create();
            _output = new VTFOutput(outputFile);

            _smallInputFile = new FileStream(smallInputFile, FileMode.Open);
            _largeInputFile = new FileStream(largeInputFile, FileMode.Open);
        }

        public void Process_Async(bool alpha)
        {
            processDelegate.BeginInvoke(alpha, 512, null, null);
        }

        public void Process_Async(bool alpha, int maxSize)
        {
            processDelegate.BeginInvoke(alpha, maxSize, null, null);
        }

        public void Process(bool alpha)
        {
            Process(alpha, 512);
        }

        public void Process(bool alpha, int maxSize)
        {
            // Load the big image
            LoadImage(_smallInputFile, maxSize);
            WriteHeader(alpha);
            WriteLowResData();

            double totalPercent = _numMipmaps;

            for (int i = _numMipmaps - 1; i > 0; i--)
            {
                for (int frame = 0; frame < _imageFrames; frame++)
                {
                    WriteHighResData(alpha, i, FrameDimension.Time, frame);
                }
                ProcessProgress(null, new ProcessProgressEventArgs { PercentComplete = ((_numMipmaps * 1.0f) - i) / _numMipmaps });
            }

            if (_largeInputFile == null)
            {
                // Write the last frame
                for (int frame = 0; frame < _imageFrames; frame++)
                {
                    WriteHighResData(alpha, 0, FrameDimension.Time, frame);
                }
            }
            else
            {
                // Dispose of the old frames
                foreach (var frame in _frames)
                {
                    frame.Dispose();
                }

                // Load the new image
                LoadImage(_largeInputFile, maxSize, false);
                for (int frame = 0; frame < _imageFrames; frame++)
                {
                    WriteHighResData(alpha, 0, FrameDimension.Time, frame);
                }
            }

            // Dispose of the images
            foreach (var frame in _frames)
            {
                frame.Dispose();
            }

            if (_output._outputStream.Length > 524288)
            {
                _output._outputStream.SetLength(0); // Reset output stream

                // Reset input streams
                if (_smallInputFile != null)
                    _smallInputFile.Seek(0, SeekOrigin.Begin);

                if (_largeInputFile != null)
                    _largeInputFile.Seek(0, SeekOrigin.Begin);

                // Process with half the size
                Process(alpha, maxSize / 2);
            }
            
            // And close the stream
            if (_smallInputFile != null)
                _smallInputFile.Close();

            if (_largeInputFile != null)
                _largeInputFile.Close();

            ProcessingComplete(null, new ProcessCompleteEventArgs { outputStream = _output._outputStream, ImageSize = _baseImageSize, Animated = _imageFrames > 1, Fading = _largeInputFile != null });
        }

        private void LoadImage(Stream inputStream, int maxSize, bool resetBaseImageSize = true)
        {
            Bitmap bitmap = (Bitmap)Image.FromStream(inputStream);

            if (bitmap.FrameDimensionsList[0] != FrameDimension.Time.Guid)
            {
                _imageFrames = 1;
            }
            else
            {
                _imageFrames = bitmap.GetFrameCount(FrameDimension.Time);
            }
            _frames = new Bitmap[_imageFrames];

            // Set the image size
            _baseImageSize = resetBaseImageSize ? Math.Min(pow2roundup(bitmap.Width), maxSize) : _baseImageSize;

            // Swap red and blue channels and split frames into array
            // http://www.codeproject.com/Questions/167235/How-to-swap-Red-and-Blue-channels-on-bitmap
            var imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] {0.0F, 0.0F, 1.0F, 0.0F, 0.0F},
                new[] {0.0F, 1.0F, 0.0F, 0.0F, 0.0F},
                new[] {1.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                new[] {0.0F, 0.0F, 0.0F, 1.0F, 0.0F},
                new[] {0.0F, 0.0F, 0.0F, 0.0F, 1.0F}
            }
            ));

            for (int i = 0; i < _imageFrames; i++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Time, i);

                var temp = new Bitmap(_baseImageSize, _baseImageSize);
                //GraphicsUnit pixel = GraphicsUnit.Pixel;
                using (Graphics g = Graphics.FromImage(temp))
                {
                    if(bitmap.Width < _baseImageSize && bitmap.Height < _baseImageSize)
                    {
                        g.DrawImage(bitmap, new Rectangle((_baseImageSize - bitmap.Width) / 2, (_baseImageSize - bitmap.Height) / 2, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttr);
                    }
                    else if (_baseImageSize > bitmap.Height) // Wider than tall
                    {
                        int newWidth = _baseImageSize;
                        int newHeight = (bitmap.Height * _baseImageSize) / bitmap.Width;

                        //int scaledWidth = (_baseImageSize * bitmap.Width) / bitmap.Height;
                        g.DrawImage(bitmap, new Rectangle(0, (_baseImageSize - newHeight) / 2, newWidth, newHeight), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttr);
                    }
                    else
                    {
                        int newHeight = _baseImageSize;
                        int newWidth = (bitmap.Width * _baseImageSize) / bitmap.Height;
                        //int scaledHeight = (_baseImageSize * bitmap.Height) / bitmap.Width;
                        //g.DrawImage(bitmap, new Rectangle(0, (_baseImageSize - scaledHeight) / 2, _baseImageSize, scaledHeight), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttr);
                        g.DrawImage(bitmap, new Rectangle((_baseImageSize - newWidth) / 2, 0, newWidth, newHeight), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttr);
                    }
                }

                _frames[i] = temp;
            }
        }

        private void WriteHeader(bool alpha)
        {
            VTFHeader header = new VTFHeader();
            header.V = 'V';
            header.T = 'T';
            header.F = 'F';
            header.Null = '\0';

            header.VersionMajor = 7;
            header.VersionMinor = 2;

            header.HeaderSize = 0x50;

            header.Width = (ushort)_baseImageSize;
            header.Height = (ushort)_baseImageSize;

            header.Flags = 0x2200;

            header.Frames = (ushort)_imageFrames;
            header.FirstFrame = 0;

            header.Padding00 = 0;
            header.Padding01 = 0;
            header.Padding02 = 0;
            header.Padding03 = 0;

            header.Reflectivity0 = 1;
            header.Reflectivity1 = 1;
            header.Reflectivity2 = 1;

            header.Padding10 = 0;
            header.Padding11 = 0;
            header.Padding12 = 0;
            header.Padding13 = 0;

            header.BumpmapScale = 1;

            header.HighResImageFormat = (uint)(alpha ? 0xF : 0xD);

            header.MipmapCount = (byte)(_numMipmaps);

            header.LowResImageFormat = (uint)0xD;
            header.LowResImageHeight = (byte)(_baseImageSize <= 16 ? _baseImageSize : 16);
            header.LowResImageWidth = (byte)(_baseImageSize <= 16 ? _baseImageSize : 16);

            header.Depth = 1;

            // Create a byte array to hold our header and initialize the elements to 0.
            // The last 15 bytes should be padding (0s)
            int size = Marshal.SizeOf(header);

            byte[] arr = new byte[size + 15];
            for (int i = 0; i < size; i++)
            {
                arr[i] = 0;
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(header, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            _output.WriteData(arr);
        }

        private void WriteLowResData()
        {
            int lowResSize = _baseImageSize <= 16 ? _baseImageSize : 16;

            Byte[] resizedImage;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap b = new Bitmap(lowResSize, lowResSize, PixelFormat.Format32bppRgb))
                {
                    using (Graphics g = Graphics.FromImage((Image)b))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.DrawImage(_frames[0], 0, 0, lowResSize, lowResSize);
                        g.Save();
                    }

                    // Lock the bitmap's bits.
                    Rectangle rect = new Rectangle(0, 0, b.Width, b.Height);
                    BitmapData bmpData = b.LockBits(rect, ImageLockMode.ReadOnly, b.PixelFormat);
                    int bytes = Math.Abs(bmpData.Stride) * b.Height;
                    resizedImage = new byte[bytes];

                    // Get the address of the first line.
                    IntPtr ptr = bmpData.Scan0;

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy(ptr, resizedImage, 0, bytes);

                    // Unlock the bits.
                    b.UnlockBits(bmpData);
                }
            }

            byte[] lowResData = Squish.CompressImage(resizedImage, lowResSize, lowResSize, SquishFlags.Dxt1 | SquishFlags.ColourRangeFit);

            _output.WriteData(lowResData);
        }

        private void WriteHighResData(bool alpha, int mipmapLevel, FrameDimension dimension, int frameNum)
        {
            int newSize = GetMipmapSize(mipmapLevel);

            Byte[] resizedImage;

            using(MemoryStream ms = new MemoryStream())
            {
                using (Bitmap b = new Bitmap(newSize, newSize, PixelFormat.Format32bppRgb))
                {
                    using (Graphics g = Graphics.FromImage((Image)b))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.DrawImage(_frames[frameNum], 0, 0, newSize, newSize);
                        g.Save();
                    }

                    // Lock the bitmap's bits.
                    Rectangle rect = new Rectangle(0, 0, b.Width, b.Height);
                    BitmapData bmpData = b.LockBits(rect, ImageLockMode.ReadOnly, b.PixelFormat);
                    int bytes = Math.Abs(bmpData.Stride) * b.Height;
                    resizedImage = new byte[bytes];

                    // Get the address of the first line.
                    IntPtr ptr = bmpData.Scan0;

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy(ptr, resizedImage, 0, bytes);

                    // Unlock the bits.
                    b.UnlockBits(bmpData);
                }
            }

            byte[] highResData = Squish.CompressImage(resizedImage, newSize, newSize, (alpha ? SquishFlags.Dxt5 : SquishFlags.Dxt1) | SquishFlags.ColourRangeFit);

            _output.WriteData(highResData);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _largeInputFile.Dispose();
                _smallInputFile.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class ProcessProgressEventArgs : EventArgs
    {
        public float PercentComplete { get; set; }
    }

    public class ProcessCompleteEventArgs : EventArgs
    {
        public Stream outputStream { get; set; }
        public int ImageSize { get; set; }
        public bool Animated { get; set; }
        public bool Fading { get; set; }
    }
}
