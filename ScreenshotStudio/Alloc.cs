//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Interop/Alloc.cs

namespace ScreenshotStudio;

using FFXIVClientStructs.Havok;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

internal static class Alloc
{
	internal static unsafe Matrix4x4* Matrix; // Align to 16-byte boundary
	private static IntPtr matrixAlloc;

	internal static unsafe Matrix4x4 GetMatrix(hkQsTransformf* transform)
	{
		transform->get4x4ColumnMajor((float*)Matrix);
		return *Matrix;
	}

	internal static unsafe void SetMatrix(hkQsTransformf* transform, Matrix4x4 matrix)
	{
		*Matrix = matrix;
		transform->set((hkMatrix4f*)Matrix);
	}

	// Init & disspose
	internal static unsafe void Init()
	{
		// Allocate space for our matrix to be aligned on a 16-byte boundary.
		// This is required due to ffxiv's use of the MOVAPS instruction.
		// Thanks to Fayti1703 for helping with debugging and coming up with this fix.
		matrixAlloc = Marshal.AllocHGlobal((sizeof(float) * 16) + 16);
		Matrix = (Matrix4x4*)(16 * ((long)(matrixAlloc + 15) / 16));
	}

	internal static void Dispose()
	{
		Marshal.FreeHGlobal(matrixAlloc);
	}
}
