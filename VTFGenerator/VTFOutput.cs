using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace ExplodingJelly.SprayGenerator
{
    class VTFOutput
    {
        public Stream _outputStream { get; set; }

        public VTFOutput(Stream outputStream)
        {
            _outputStream = outputStream;
        }

        public VTFOutput(string outputFile)
        {
            _outputStream = new FileStream(outputFile, FileMode.Append);
        }

        public void WriteData(byte[] imageData)
        {
            _outputStream.Write(imageData, 0, imageData.Length);
        }

        public void Close()
        {
            _outputStream.Close();
        }

    }
}
