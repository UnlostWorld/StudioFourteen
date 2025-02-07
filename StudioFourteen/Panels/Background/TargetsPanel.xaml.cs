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

namespace StudioFourteen.Studio.Background;

using Dalamud.Plugin.Services;
using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Appearance;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Extensions;

[DependencyProperty<bool>("ShowBackground", DefaultValue = true)]
public partial class TargetsPanel : Panel
{
	[Notify] private CharacterViewModel? target;
	[Notify] private string removeCharacterTooltip = string.Empty;

	public TargetsPanel()
	{
		for (int i = GroupPoseService.GPoseFirstCharacter; i < GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount; ++i)
		{
			this.Characters.Add(new(i));
		}

		this.Services.Target.TargetChanged += this.OnCurrentTargetChanged;
	}

	public List<CharacterViewModel> Characters { get; init; } = new();
	public FastObservableCollection<CharacterViewModel> ValidCharacters { get; init; } = new();

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		List<CharacterViewModel> validCharacters = new();
		bool changed = false;
		foreach (CharacterViewModel character in this.Characters)
		{
			bool wasValid = character.IsValid;
			character.OnFrameworkUpdate();

			if (character.IsValid)
			{
				validCharacters.Add(character);
			}

			changed |= wasValid != character.IsValid;
		}

		if (changed)
		{
			this.Dispatcher.Invoke(() => this.ValidCharacters.Replace(validCharacters));
		}
	}

	private void OnCurrentTargetChanged(int objectTableIndex)
	{
		foreach(CharacterViewModel character in this.ValidCharacters)
		{
			if (character.ObjectTableIndex == this.Services.Target.TargetObjectIndex)
			{
				this.Target = character;
				break;
			}
		}

		this.RemoveCharacterTooltip = StudioFourteen.Resources.Format("LOC_Target_DeleteCharacter", this.Target?.Name);
	}

	private void OnAddCharacterClicked(object sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
		/*if (sender is UIElement target)
		{
			LibraryPopOut<ICharacterAppearance> popOut = new();
			popOut.DefaultTags = new("Named");
			popOut.PlacementTarget = target;
			popOut.Title = "Create Character";
			popOut.StaysOpen = false;
			popOut.SelectionChanged = (appearance, isFinal) =>
			{
				if (appearance == null || !isFinal)
					return;

				this.CreateCharacter(appearance).Run();
			};
		}*/
	}

	private async Task CreateCharacter(ICharacterAppearance appearance)
	{
		int index = await this.Services.CharacterLifecycle.CreateAsync(appearance);
		await this.SelectObject(index);
	}

	private async void OnRemoveCharacterClicked(object sender, RoutedEventArgs e)
	{
		CharacterViewModel? target = this.Target;
		if (target == null)
			return;

		await this.Services.CharacterLifecycle.DestroyAsync(target.ObjectTableIndex);

		this.NotifyPropertyChanged(nameof(TargetsPanel.Target));
	}

	private async Task SelectObject(int index)
	{
		await Task.Delay(300);

		foreach (CharacterViewModel vm in this.Characters)
		{
			if (vm.ObjectTableIndex == index)
			{
				vm.IsCurrent = true;
			}
		}
	}
}

public unsafe partial class CharacterViewModel : ViewModel
{
	public readonly int ObjectTableIndex;

	private bool isCurrent = false;
	[Notify] private bool isValid = false;
	[Notify] private string? name = null;

	public CharacterViewModel(int index)
	{
		this.ObjectTableIndex = index;
	}

	public bool IsCurrent
	{
		get => this.isCurrent;
		set => this.Services.Target.SetTarget(this.ObjectTableIndex);
	}

	public unsafe void OnFrameworkUpdate()
	{
		IntPtr address = DalamudServices.ObjectTable?.GetObjectAddress(this.ObjectTableIndex) ?? IntPtr.Zero;
		Character* pCharacter = (Character*)address;

		this.IsValid = pCharacter != null;

		if (this.IsValid)
		{
			// Hide ornaments and mounts
			this.IsValid &= pCharacter->ObjectKind != ObjectKind.Ornament;
			this.IsValid &= pCharacter->ObjectKind != ObjectKind.Mount;
		}

		if (this.IsValid)
		{
			this.Name = pCharacter->GetRoleOrDisplayName();
			this.isCurrent = TargetSystem.Instance()->GPoseTarget == pCharacter;
			this.RaisePropertyChanged(nameof(CharacterViewModel.IsCurrent));
		}
		else
		{
			this.isCurrent = false;
			this.RaisePropertyChanged(nameof(CharacterViewModel.IsCurrent));
		}
	}
}