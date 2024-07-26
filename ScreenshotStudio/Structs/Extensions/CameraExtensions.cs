namespace FFXIVClientStructs.FFXIV.Client.Game;

using FFXIVClientStructs.FFXIV.Client.Game;
using global::System.Numerics;

public static class CameraExtensions
{
	public static unsafe Matrix4x4 GetViewMatrix(this Camera camera)
	{
		var viewMatrix = camera.CameraBase.SceneCamera.ViewMatrix;
		viewMatrix.M44 = 1; // hcsf
		return viewMatrix;
	}
}
