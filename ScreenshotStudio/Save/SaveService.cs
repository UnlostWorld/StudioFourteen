namespace ScreenshotStudio.Save;

using Microsoft.Win32;
using ScreenshotStudio.Input;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio;
using ScreenshotStudio.Utilities;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using WpfUtils.Extensions;

public class SaveService : ServiceBase
{
	private string? defaultDirectoryPath;

	public SaveConfiguration Current => this.Services.Settings.Current.SaveConfig;

	public override Task Start()
	{
		this.EnsureDefaultDirectory();

		this.Services.Input.AddListener(KeyBindEvents.Save, this.Save);
		this.Services.Input.AddListener(KeyBindEvents.SaveAs, this.SaveAs);

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.Input.RemoveListener(KeyBindEvents.Save, this.Save);
		this.Services.Input.RemoveListener(KeyBindEvents.SaveAs, this.SaveAs);

		return base.Stop();
	}

	public void Save() => this.Save(null);
	public void SaveAs() => this.SaveAs(null);

	public void Save(SaveConfiguration? configuration)
	{
		this.SaveAsAsync(configuration).Run();
	}

	public void SaveAs(SaveConfiguration? configuration)
	{
		this.SaveAsAsync(configuration).Run();
	}

	public async Task SaveAsAsync(SaveConfiguration? configuration = null)
	{
		await Task.Yield();

		if (configuration == null)
			configuration = this.Current;

		BackgroundWindow? bgWindow = this.Services.Panels.Get<BackgroundWindow>();
		if (bgWindow == null)
		{
			this.Log.Error("No background window found");
			return;
		}

		await Threads.UiThread(bgWindow);

		SaveFileDialog dialog = new();
		dialog.FileName = "Hello World";
		dialog.DefaultExt = ".studio";
		dialog.Filter = "Studio Scene (.studio)|*.studio";
		dialog.DefaultDirectory = this.defaultDirectoryPath?.TrimEnd('/', '\\');

		if (this.Services.Settings.Current.LastSaveDirectory != null)
			dialog.DefaultDirectory = this.Services.Settings.Current.LastSaveDirectory;

		foreach (SourceBase src in this.Services.Library.Sources)
		{
			if (src is FileSource fileSource)
			{
				if (fileSource.Directory?.Exists == true)
				{
					FileDialogCustomPlace place = new(fileSource.Directory.FullName);
					dialog.CustomPlaces.Add(place);
				}
			}
		}

		bool? result = dialog.ShowDialog(bgWindow);

		if (result != true)
			return;

		string fileName = dialog.FileName;
		await Threads.NonUiThread();

		// do save!
		{
		}

		this.Services.Settings.Current.SaveConfig = configuration;
		this.Services.Settings.Current.LastSaveDirectory = Path.GetDirectoryName(fileName);
	}

	private void EnsureDefaultDirectory()
	{
		this.defaultDirectoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ScreenshotStudio\\";
		if (!Directory.Exists(this.defaultDirectoryPath))
		{
			Directory.CreateDirectory(this.defaultDirectoryPath);
		}
	}

	public class SaveConfiguration
	{
		public bool IncludeLocation { get; set; } = true;
		public bool IncludeWeather { get; set; } = true;
		public bool IncludeTimeOfDay { get; set; } = true;
		public bool IncludePoses { get; set; } = true;
		public bool IncludeAppearances { get; set; } = false;

		public SaveConfiguration Copy()
		{
			SaveConfiguration other = new();
			other.IncludeLocation = this.IncludeLocation;
			other.IncludeWeather = this.IncludeWeather;
			other.IncludeTimeOfDay = this.IncludeTimeOfDay;
			other.IncludePoses = this.IncludePoses;
			other.IncludeAppearances = this.IncludeAppearances;
			return other;
		}
	}
}
