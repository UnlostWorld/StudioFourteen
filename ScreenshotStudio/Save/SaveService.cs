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
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;

public class SaveService : ServiceBase
{
	private readonly Dictionary<int, bool> includeCharacters = new();
	private DirectoryInfo? defaultDirectory;

	public delegate void SaveEventDelegate();

	public event SaveEventDelegate? Saving;
	public event SaveEventDelegate? Saved;

	[AutoNotify] public FileInfo? SaveFileInfo { get; set; }

	[AutoNotify] public SaveConfiguration Current => this.Services.Settings.Current.SaveConfig;

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

		if (this.Services.Settings.Current.LastSaveDirectory != null)
			this.SetSaveFileInfo(new(this.Services.Settings.Current.LastSaveDirectory), null);

		this.Services.Input.AddListener(KeyBindEvents.Save, this.Save);

		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.Input.RemoveListener(KeyBindEvents.Save, this.Save);

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

	public void Save(SaveConfiguration? configuration)
	{
		this.SaveAsync(configuration).Run();
	}

	public async Task SaveAsync(SaveConfiguration? configuration = null)
	{
		if (configuration == null)
			configuration = this.Current;

		this.Saving?.Invoke();

		await Threads.NonUiThread();

		// do save!
		{
		}

		this.Services.Settings.Current.SaveConfig = configuration;
		this.Services.Settings.Current.LastSaveDirectory = this.SaveFileInfo?.FullName;

		this.Saved?.Invoke();
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
