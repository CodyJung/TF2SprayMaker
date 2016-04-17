// ManagedSquish - Copyright (c) 2011-12 Rodrigo 'r2d2rigo' Díaz
// libsquish - Copyright (c) 2006 Simon Brown
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

#pragma once

using namespace System;

namespace NativeSquish_x64
{
	public ref class Squish
	{
	public:
		static int GetStorageRequirements( int width, int height, int flags );
		static void Compress(IntPtr const rgba, IntPtr block, int flags);
		static void CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags);
		static void Decompress(IntPtr rgba, IntPtr block, int flags );
		static void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags );
		static void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags );
	};
}
