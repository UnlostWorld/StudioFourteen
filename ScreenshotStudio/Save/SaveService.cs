namespace ScreenshotStudio.Save;

using ScreenshotStudio.Files;
using ScreenshotStudio.Input;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WpfUtils.Extensions;

public class SaveService : ServiceBase
{
	private readonly Dictionary<int, bool> includeCharacters = new();
	private DirectoryInfo? defaultDirectory;

	[AutoNotify] public FileInfo? SaveFileInfo { get; set; }

	public SaveConfiguration Current => this.Services.Settings.Current.SaveConfig;

	public void SetIncludeCharacter(int objectTableIndex, bool include)
	{
		if (!this.includeCharacters.ContainsKey(objectTableIndex))
			this.includeCharacters.Add(objectTableIndex, include);

		this.includeCharacters[objectTableIndex] = include;
	}

	public bool GetIncludeCharacter(int objectTableIndex)
	{
		bool include = false;
		if (!this.includeCharacters.TryGetValue(objectTableIndex, out include))
			return false;

		return include;
	}

	public bool CanIncludeCharacter(int objectTableIndex)
	{
		return this.includeCharacters.ContainsKey(objectTableIndex);
	}

	public override Task Start()
	{
		this.EnsureDefaultDirectory();

		this.Services.Input.AddListener(KeyBindEvents.Save, this.Save);
		this.Services.Input.AddListener(KeyBindEvents.SaveAs, this.SaveAs);

		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.Input.RemoveListener(KeyBindEvents.Save, this.Save);
		this.Services.Input.RemoveListener(KeyBindEvents.SaveAs, this.SaveAs);

		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;

		return base.Stop();
	}

	public void SetSaveFileInfo(DirectoryInfo? directory = null, string? name = null)
	{
		directory = directory ?? this.SaveFileInfo?.Directory ?? this.defaultDirectory;
		name = name ?? Path.GetFileNameWithoutExtension(this.SaveFileInfo?.Name) ?? "New Scene";

		this.SaveFileInfo = new FileInfo($"{directory?.FullName}\\{name}.studio");
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

		////if (this.Services.Settings.Current.LastSaveDirectory != null)
		////	dialog.DefaultDirectory = this.Services.Settings.Current.LastSaveDirectory;

		FileInfo? destination = await this.Services.Files.ShowSaveDialog<SceneFile>(this.SaveFileInfo);
		if (destination == null)
			return;

		this.SaveFileInfo = destination;

		await Threads.NonUiThread();

		// do save!
		{
		}

		this.Services.Settings.Current.SaveConfig = configuration;
		this.Services.Settings.Current.LastSaveDirectory = destination.Directory?.FullName;
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.includeCharacters.Clear();

		if (DalamudServices.ObjectTable == null)
			return;

		int fromIndex = GroupPoseService.GPoseFirstCharacter;
		int toIndex = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;

		if (!this.Services.GroupPose.IsGroupPosing)
		{
			fromIndex = 0;
			toIndex = Math.Min(DalamudServices.ObjectTable.Length, GroupPoseService.GPoseFirstCharacter);
		}

		for (int i = fromIndex; i < toIndex; ++i)
		{
			if (!this.includeCharacters.ContainsKey(i))
			{
				this.includeCharacters.Add(i, i == fromIndex);
			}
		}
	}

	private void EnsureDefaultDirectory()
	{
		this.defaultDirectory = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ScreenshotStudio\\");
		if (!this.defaultDirectory.Exists)
		{
			this.defaultDirectory.Create();
		}

		this.SaveFileInfo = new FileInfo($"{this.defaultDirectory.FullName}\\New Scene.studio");
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
