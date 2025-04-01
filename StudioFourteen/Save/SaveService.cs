// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Save;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Files;
using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WpfUtils.Extensions;

public class SaveService : ServiceBase
{
	private readonly InputActionListener saveListener;
	private readonly Dictionary<int, bool> includeCharacters = new();
	private DirectoryInfo? defaultDirectory;

	public SaveService()
	{
		this.saveListener = new(InputAction.Save);
		this.saveListener.Activate = this.Save;
	}

	public delegate void SaveEventDelegate();

	public event SaveEventDelegate? Saving;
	public event SaveEventDelegate? Saved;

	[AutoNotify] public FileInfo? SaveFileInfo { get; set; }
	[AutoNotify] public SaveMetaData MetaData { get; init; } = new();

	[AutoNotify] public SaveConfiguration Current => this.Services.Settings.Current.SaveConfig;
	[AutoNotify] public bool IsSaving { get; set; } = false;

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

	public override Task Start()
	{
		this.EnsureDefaultDirectory();

		if (this.Services.Settings.Current.LastSaveDirectory != null)
		{
			this.SetSaveFileInfo(new(this.Services.Settings.Current.LastSaveDirectory), null);
		}

		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);

		this.MetaData.LoadDefaults();
		this.saveListener.Enable();

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		this.saveListener.Disable();

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

		if (this.SaveFileInfo == null)
		{
			this.Log.Error("No save file info in SaveService");
			return;
		}

		this.IsSaving = true;
		this.Saving?.Invoke();

		await TickService.GameTick();

		// do save!
		{
			SceneFile file = new();
			file.Author = this.MetaData.Author;
			file.Description = this.MetaData.Description;
			file.Version = this.MetaData.Version;
			file.Tags = this.MetaData.Tags;

			// Actors
			foreach ((int objectTableIndex, bool include) in this.includeCharacters)
			{
				if (!include)
					continue;

				SceneFile.Actor actor = new();
				actor.Role = this.Services.Roles.GetRoleOrDefault(objectTableIndex);

				if (configuration.IncludePoses)
				{
					actor.Pose = new PoseFile();
					await actor.Pose.Save(objectTableIndex, false);
				}

				file.Actors.Add(actor);
			}

			// Cameras
			file.Cameras.Clear();
			file.Cameras.AddRange(this.Services.Camera.Cameras);

			await this.Services.Files.Save(file, this.SaveFileInfo);
		}

		await Threads.NonUiThread();

		this.Services.Settings.Current.SaveConfig = configuration;
		this.Services.Settings.Current.LastSaveDirectory = this.SaveFileInfo.Directory?.FullName;

		this.IsSaving = false;
		this.Saved?.Invoke();
	}

	public void Open(SceneFile file)
	{
		try
		{
			OpenWindow.OpenScene(file);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error opening scene");
		}
	}

	public Task OpenAsync(SceneFile file)
	{
		try
		{
			return OpenWindow.OpenSceneAsync(file);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error opening scene");
		}

		return Task.CompletedTask;
	}

	public async Task Revert(SceneFile file)
	{
		// TODO: record which indexes were used by this scene file
		this.Services.Pose.FlushBoneReferences();

		for (int i = 0; i < 300; i++)
		{
			if (this.Services.CharacterAppearance.CanRestore(i))
			{
				await this.Services.CharacterAppearance.Restore(i);
			}
		}
	}

	public unsafe bool CanInclude(Character* pCharacter)
	{
		TickService.VerifyGameTickThread();

		if (pCharacter == null)
			return false;

		if (pCharacter->GetKind() != ObjectKind.Player
			&& pCharacter->GetKind() != ObjectKind.BattleNpc
			&& pCharacter->GetKind() != ObjectKind.Companion)
			return false;

		if (pCharacter->GetKind() == ObjectKind.BattleNpc
			&& pCharacter->NameString != "Carbuncle")
			return false;

		return this.includeCharacters.ContainsKey(pCharacter->ObjectIndex);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.includeCharacters.Clear();

		if (DalamudServices.ObjectTable == null)
			return;

		int fromIndex = GroupPoseService.GPoseFirstCharacter;
		int toIndex = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;

		if (!newState)
		{
			fromIndex = 0;
			toIndex = Math.Min(DalamudServices.ObjectTable.Length, GroupPoseService.GPoseFirstCharacter);
		}

		for (int i = fromIndex; i < toIndex; ++i)
		{
			this.includeCharacters.Add(i, i == fromIndex);
		}
	}

	private void EnsureDefaultDirectory()
	{
		this.defaultDirectory = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\StudioFourteen\\");
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

	public class SaveMetaData : AutoViewModel
	{
		[AutoNotify] public string? Author { get; set; }
		[AutoNotify] public string? Version { get; set; }
		[AutoNotify] public string? Description { get; set; }
		[AutoNotify] public TagCollection Tags { get; init; } = new();

		public void LoadDefaults()
		{
			this.Author = this.Services.Settings.Current.DefaultAuthor;
			this.Version = this.Services.Settings.Current.DefaultVersion;
		}
	}
}
