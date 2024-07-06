namespace ScreenshotStudio.Services;

using ScreenshotStudio.Plugin;

internal class GroupPoseService : ServiceBase
{
	public static bool IsGroupPosing => DalamudServices.ClientState?.IsGPosing ?? false;
}
