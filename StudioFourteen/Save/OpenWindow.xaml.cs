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

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance;
using StudioFourteen.Files;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils;
using WpfUtils.Extensions;
using Panel = StudioFourteen.Panels.Panel;

public partial class OpenWindow : Panel
{
	private readonly Dictionary<int, OpenCharacterViewModel> characterLookup = new();

	[AutoNotify] public FileTypeInfoBase? SceneType => this.Services.Files.GetTypeInfo(this.Scene);
	[AutoNotify] public SceneFile? Scene { get; set; }
	[AutoNotify] public FastObservableCollection<Assignment> Actors { get; init; } = new();
	[AutoNotify] public FastObservableCollection<OpenCharacterViewModelBase> Characters { get; init; } = new();

	public bool? Result { get; set; }

	public static void OpenScene(SceneFile scene)
	{
		OpenSceneAsync(scene).Run();
	}

	public static async Task OpenSceneAsync(SceneFile scene)
	{
		try
		{
			OpenWindow? panel = await ServiceManager.Instance.Panels.Open<OpenWindow>();

			if (panel == null)
				throw new Exception("No Open Window");

			panel.Result = null;

			await panel.Dispatcher.MainThread();
			await panel.SetScene(scene);

			while (panel.Result == null)
				await Task.Delay(100);

			foreach (Assignment assignment in panel.Actors)
			{
				await assignment.Apply();
			}

			// TODO option to not load cameras.
			ServiceManager.Instance.Camera.LoadCameras(scene.Cameras);
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error opening scene");
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.Characters.Add(new OpenIgnoreCharacterViewModel());

		if (this.Services.CharacterLifecycle.CanSpawn)
		{
			this.Characters.Add(new OpenCreateCharacterViewModel());
		}
	}

	protected override void OnClosed()
	{
		if (this.Result == null)
			this.Result = false;

		base.OnClosed();
	}

	protected async Task SetScene(SceneFile scene)
	{
		await this.Dispatcher.MainThread();
		this.Scene = scene;
		this.Actors.Clear();

		while (this.Characters.Count <= 2)
		{
			await Task.Delay(33);
		}

		await this.Dispatcher.MainThread();

		if (scene.Actors.Count == 1)
		{
			Assignment assignment = new(scene.Actors[0]);

			foreach(OpenCharacterViewModelBase character in this.Characters)
			{
				if (character is OpenIgnoreCharacterViewModel)
					continue;

				if (character is OpenCreateCharacterViewModel)
					continue;

				assignment.Character = character;
				break;
			}

			this.Actors.Add(assignment);
		}
		else
		{
			foreach (SceneFile.Actor actor in scene.Actors)
			{
				Assignment assignment = new(actor);
				assignment.Character = this.Characters[0];
				this.Actors.Add(assignment);
			}
		}
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			return;

		for (int i = 0; i < DalamudServices.ObjectTable.Length; ++i)
		{
			bool isValid = true;

			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(i);
			isValid = this.Services.Save.CanInclude(pCharacter);

			if (!isValid && this.characterLookup.ContainsKey(i))
			{
				OpenCharacterViewModel vm = this.characterLookup[i];
				this.Dispatcher.Invoke(() => this.Characters.Remove(vm));

				this.characterLookup.Remove(i);
				continue;
			}
			else if (isValid && !this.characterLookup.ContainsKey(i))
			{
				OpenCharacterViewModel vm = new(i);
				this.characterLookup.Add(i, vm);
				this.Dispatcher.Invoke(() => this.Characters.Add(vm));
			}
			else if (isValid && this.characterLookup.ContainsKey(i))
			{
				this.characterLookup[i].Name = pCharacter->GetDisplayName();
			}
		}
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs e)
	{
		this.Result = true;
		this.Close();
	}

	private void OnCancelClicked(object sender, RoutedEventArgs e)
	{
		this.Result = false;
		this.Close();
	}
}

public abstract class OpenCharacterViewModelBase
	: AutoViewModel
{
}

public class OpenIgnoreCharacterViewModel
	: OpenCharacterViewModelBase
{
}

public class OpenCreateCharacterViewModel
	: OpenCharacterViewModelBase
{
}

public class OpenCharacterViewModel(int objectTableIndex)
	: OpenCharacterViewModelBase
{
	[AutoNotify] public int ObjectTableIndex { get; set; } = objectTableIndex;
	[AutoNotify] public string? Name { get; set; }
}

public class Assignment(SceneFile.Actor actor)
	: AutoViewModel
{
	private bool includePose = true;
	private bool includeAppearance = true;

	[AutoNotify] public SceneFile.Actor Actor { get; init; } = actor;
	[AutoNotify] public ICharacterAppearance? Appearance { get; set; }
	[AutoNotify] public OpenCharacterViewModelBase? Character { get; set; }

	[AutoNotify]
	public bool IncludePose
	{
		get => this.includePose && this.CanIncludePose;
		set => this.includePose = value;
	}

	[AutoNotify]
	public bool IncludeAppearance
	{
		get => this.includeAppearance && this.CanIncludeAppearance;
		set => this.includeAppearance = value;
	}

	[AutoNotify]
	public bool CanSelectAppearance =>
		this.Character is OpenCreateCharacterViewModel
		&& !this.IncludeAppearance
		&& this.Services.CharacterLifecycle.CanSpawn;

	[AutoNotify]
	public bool CanIncludePose =>
		this.Actor.Pose != null
		&& this.Character is not OpenIgnoreCharacterViewModel;

	[AutoNotify]
	public bool CanIncludeAppearance =>
		this.Actor.Appearance != null
		&& this.Character is not OpenIgnoreCharacterViewModel;

	[AutoNotify]
	public string ApplyPoseTooltip
	{
		get
		{
			if (this.Character is OpenCharacterViewModel character)
			{
				return Resources.Format("LOC_OpenScene_ApplyPose", this.Actor.Role, character.Name);
			}
			else if (this.Character is OpenIgnoreCharacterViewModel)
			{
				return Resources.Format("LOC_OpenScene_ApplyPoseIgnore", this.Actor.Role);
			}
			else if (this.Character is OpenCreateCharacterViewModel)
			{
				return Resources.Format("LOC_OpenScene_ApplyPoseNew", this.Actor.Role);
			}

			return string.Empty;
		}
	}

	[AutoNotify]
	public string ApplyAppearanceTooltip
	{
		get
		{
			if (this.Character is OpenCharacterViewModel character)
			{
				return Resources.Format("LOC_OpenScene_ApplyAppearance", this.Actor.Role, character.Name);
			}
			else if (this.Character is OpenIgnoreCharacterViewModel)
			{
				return Resources.Format("LOC_OpenScene_ApplyAppearanceIgnore", this.Actor.Role);
			}
			else if (this.Character is OpenCreateCharacterViewModel)
			{
				return Resources.Format("LOC_OpenScene_ApplyAppearanceNew", this.Actor.Role);
			}

			return string.Empty;
		}
	}

	public async Task Apply()
	{
		if (this.Actor == null)
			return;

		if (this.Character is OpenIgnoreCharacterViewModel)
			return;

		ICharacterAppearance? appearance = null;
		if (this.Character is OpenCreateCharacterViewModel && this.Appearance != null)
		{
			appearance = this.Appearance;
		}
		else if (this.IncludeAppearance && this.CanIncludeAppearance && this.Actor.Appearance != null)
		{
			appearance = this.Actor.Appearance;
		}

		int objectTableIndex = -1;
		if (this.Character is OpenCharacterViewModel character)
		{
			objectTableIndex = character.ObjectTableIndex;

			if (appearance != null)
			{
				await appearance.Apply(objectTableIndex);
			}
		}
		else if (this.Character is OpenCreateCharacterViewModel && this.Services.CharacterLifecycle.CanSpawn)
		{
			objectTableIndex = await this.Services.CharacterLifecycle.CreateAsync(appearance);
		}

		if (this.IncludePose && this.CanIncludePose && this.Actor.Pose != null)
		{
			await this.Actor.Pose.Apply(objectTableIndex);
		}
	}
}