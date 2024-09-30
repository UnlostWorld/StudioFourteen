namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Panels;
using System.Windows;
using ScreenshotStudio.Plugin;
using WpfUtils.Extensions;

public partial class ErrorWindow : Panel
{
	private static ErrorWindow? instance;
	private static bool isOpening = false;
	private static string? message = "An Unknown error has occurred";

	public string? ErrorMessage
	{
		get => message;
		set
		{
			message = value;
			this.NotifyPropertyChanged();
		}
	}

	public static void Show(string message)
	{
		ErrorWindow.message = message;

		if (instance == null)
		{
			if (isOpening)
				return;

			isOpening = true;
			ServiceManager.Instance.Panels.Open<ErrorWindow>().Run();
		}
		else
		{
			instance.NotifyPropertyChanged(nameof(ErrorMessage));
		}
	}

	protected override void OnOpened()
	{
		instance = this;
		isOpening = false;
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		instance = null;
		base.OnClosed();
	}

	private void OnConsoleClicked(object sender, RoutedEventArgs e)
	{
		DalamudServices.CommandManager?.ProcessCommand("/xllog");
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