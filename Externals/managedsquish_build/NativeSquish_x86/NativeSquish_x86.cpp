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

#include "stdafx.h"

#include "NativeSquish_x86.h"
#include "squish.h"

namespace NativeSquish_x86
{
	int Squish::GetStorageRequirements( int width, int height, int flags)
	{
		return squish::GetStorageRequirements(width, height, flags);
	}

	void Squish::Compress(IntPtr rgba, IntPtr block, int flags)
	{
		squish::Compress(reinterpret_cast<Byte*>(rgba.ToPointer()),reinterpret_cast<Byte*>(block.ToPointer()), flags);
	}

	void Squish::CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags)
	{
		squish::CompressMasked(reinterpret_cast<Byte*>(rgba.ToPointer()), mask, reinterpret_cast<Byte*>(block.ToPointer()), flags);
	}

	void Squish::Decompress(IntPtr rgba, IntPtr block, int flags )
	{
		squish::Decompress(reinterpret_cast<Byte*>(rgba.ToPointer()), reinterpret_cast<Byte*>(block.ToPointer()), flags);
	}

	void Squish::CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags )
	{
		squish::CompressImage(reinterpret_cast<Byte*>(rgba.ToPointer()), width, height, reinterpret_cast<Byte*>(blocks.ToPointer()), flags);
	}

	void Squish::DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags )
	{
		squish::DecompressImage(reinterpret_cast<Byte*>(rgba.ToPointer()), width, height, reinterpret_cast<Byte*>(blocks.ToPointer()), flags);
	}
}