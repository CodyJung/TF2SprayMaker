using System.Runtime.InteropServices;

namespace ExplodingJelly.SprayGenerator
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VTFHeader
    {
        public char V;
        public char T;
        public char F;
        public char Null;
        public uint VersionMajor;
        public uint VersionMinor;
        public uint HeaderSize;
        public ushort Width;
        public ushort Height;
        public uint Flags;
        public ushort Frames;
        public ushort FirstFrame;
        public byte Padding00;
        public byte Padding01;
        public byte Padding02;
        public byte Padding03;
        public float Reflectivity0;
        public float Reflectivity1;
        public float Reflectivity2;
        public byte Padding10;
        public byte Padding11;
        public byte Padding12;
        public byte Padding13;
        public float BumpmapScale;
        public uint HighResImageFormat;
        public byte MipmapCount;
        public uint LowResImageFormat;
        public byte LowResImageWidth;
        public byte LowResImageHeight;
        public ushort Depth;
    }
}
