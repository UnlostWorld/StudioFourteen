namespace ScreenshotStudio.Studio.Background;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

public partial class Targets : View
{
	public Targets()
	{
		for (int i = GroupPoseService.GPoseFirstCharacter; i < GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount; ++i)
		{
			this.Characters.Add(new(i));
		}
	}

	public List<CharacterViewModel> Characters { get; init; } = new();

	[AutoNotify] public bool IsInGPose => this.Services.Studio.IsOpenAndInGPose;

	[AutoNotify]
	public CharacterViewModel? Target
	{
		get
		{
			foreach (CharacterViewModel vm in this.Characters)
			{
				if (vm.IsCurrent)
				{
					return vm;
				}
			}

			return null;
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		foreach (CharacterViewModel character in this.Characters)
		{
			character.OnFrameworkUpdate();
		}
	}

	private void OnAddCharacterClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		// uuuh
		throw new NotImplementedException();
		/*LibraryModal.Show<ICharacterEntry>(
			sender,
			"Create Character",
			defaultTags,
			null,
			(appearance, isFinal) =>
			{
				if (!isFinal)
					return;

				this.CreateCharacter(appearance);
			});*/
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

		int index = this.Characters.IndexOf(target);
		await this.Services.CharacterLifecycle.DestroyAsync(target.ObjectTableIndex);
		this.SelectNearest(index);
	}

	private void SelectNearest(int index)
	{
		for (int i = index; i < this.Characters.Count; i++)
		{
			if (!this.Characters[i].IsValid)
				continue;

			this.Characters[i].IsCurrent = true;
			break;
		}

		if (this.Target == null)
		{
			for (int i = index; i >= 0; i--)
			{
				if (!this.Characters[i].IsValid)
					continue;

				this.Characters[i].IsCurrent = true;
				break;
			}
		}
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

	private void OnResetNicknameClicked(object sender, RoutedEventArgs e)
	{
		CharacterViewModel? target = this.Target;
		if (target == null)
			return;

		target.Nickname = null;
	}
}

public unsafe class CharacterViewModel : ViewModel
{
	public readonly int ObjectTableIndex;

	private bool isCurrent = false;

	public CharacterViewModel(int index)
	{
		this.ObjectTableIndex = index;
	}

	[AutoNotify] public bool IsValid { get; private set; }
	[AutoNotify] public string? Name { get; private set; }

	[AutoNotify]
	public string? Nickname
	{
		get => this.Services.Nickname.GetNickname(this.ObjectTableIndex);
		set => this.Services.Nickname.SetNickname(this.ObjectTableIndex, value);
	}

	[AutoNotify]
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
			this.IsValid &= pCharacter->ObjectKind != ObjectKind.Ornament;
			this.IsValid &= pCharacter->ObjectKind != ObjectKind.Mount;
		}

		if (this.IsValid)
		{
			this.Name = pCharacter->GetNameAsString();

			if (pCharacter->ObjectKind == ObjectKind.Companion)
			{
				Character* pOwner = pCharacter->GetParentCharacter();
				this.Name = $"{pOwner->GetNameAsString()}'s {this.Name}";
			}

			this.isCurrent = TargetSystem.Instance()->GPoseTarget == pCharacter;
		}
		else
		{
			this.isCurrent = false;
		}
	}
}