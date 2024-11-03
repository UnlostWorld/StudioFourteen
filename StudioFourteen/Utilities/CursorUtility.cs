namespace StudioFourteen.Utilities;
using System.Runtime.InteropServices;

public static class CursorUtility
{
	private static bool cursorVisible = true;

	public static void SetCursorVisible(bool visible)
	{
		if (cursorVisible == visible)
			return;

		cursorVisible = visible;

		Logging.Shared.Information($">> {visible}");
		ShowCursor(visible);
	}

	[DllImport("user32.dll")]
	private static extern int ShowCursor(bool bShow);
}
