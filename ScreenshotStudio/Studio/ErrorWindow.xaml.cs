// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Windows;
using System.Threading.Tasks;
using PropertyChanged.SourceGenerator;
using System.Windows;
using ScreenshotStudio.Plugin;
using System.ComponentModel;

public partial class ErrorWindow : PanelWindow
{
	private static int windowCount = 0;

	[Notify] private string? errorMessage = "An Unknown error has occurred";

	public static void Show(string message)
	{
		Task.Run(async () => await ShowAsync(message));
	}

	public static async Task ShowAsync(string message)
	{
		if (windowCount > 5)
			return;

		windowCount++;

		ErrorWindow? wnd = await Panel.ShowAsync<ErrorWindow>();
		if (wnd != null)
		{
			wnd.ErrorMessage = message;
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		windowCount--;
		base.OnClosing(e);
	}

	private void OnConsoleClicked(object sender, RoutedEventArgs e)
	{
		DalamudServices.CommandManager.ProcessCommand("/xllog");
	}

	private void OnGitHubClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://github.com/XIV-Tools/ScreenshotStudio");
	}

	private void OnDiscordClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://discord.gg/KvGJCCnG8t");
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}