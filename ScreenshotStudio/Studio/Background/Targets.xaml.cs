namespace ScreenshotStudio.Studio.Background;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

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

		LibraryModal.Show<ICharacterEntry>(
			sender,
			"Create Character",
			defaultTags,
			null,
			(appearance, isFinal) =>
			{
				if (!isFinal)
					return;

				this.CreateCharacter(appearance);
			});
	}

	private unsafe void CreateCharacter(ICharacterAppearance appearance)
	{
		Threads.RunOnFrameworkThread(() =>
		{
			Character* character = this.Services.CharacterLifecycle.Create(appearance);
			int index = character->GameObject.ObjectIndex;
			this.SelectObject(index);
		});
	}

	private unsafe void OnRemoveCharacterClicked(object sender, RoutedEventArgs e)
	{
		CharacterViewModel? target = this.Target;
		if (target == null)
			return;

		int index = this.Characters.IndexOf(target);
		this.Services.CharacterLifecycle.Destroy(target.Character);
		this.SelectNearest(index);
	}

	private void SelectNearest(int index)
	{
		Task.Run(async () =>
		{
			await Task.Delay(100);

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
		});
	}

	private void SelectObject(int index)
	{
		Task.Run(async () =>
		{
			await Task.Delay(300);

			foreach (CharacterViewModel vm in this.Characters)
			{
				if (vm.ObjectTableIndex == index)
				{
					vm.IsCurrent = true;
				}
			}
		});
	}
}

public unsafe class CharacterViewModel : ViewModel
{
	public readonly int ObjectTableIndex = 0;
	private string? lastName;

	public CharacterViewModel(int index)
	{
		this.ObjectTableIndex = index;
	}

	[AutoNotify] public IntPtr Address { get; set; } = IntPtr.Zero;
	[AutoNotify] public bool IsValid => this.Address != IntPtr.Zero;
	[AutoNotify] public unsafe Character* Character => (Character*)this.Address;

	[AutoNotify]
	public string? Name
	{
		get
		{
			if (!this.IsValid)
				return this.lastName ?? "Invalid";

			this.lastName = this.Character->GetNameAsString() ?? "???";

			return this.lastName;
		}
	}

	[AutoNotify]
	public bool IsCurrent
	{
		get
		{
			if (!this.IsValid)
				return false;

			return (Character*)TargetSystem.Instance()->GPoseTarget == this.Character;
		}

		set
		{
			Threads.RunOnFrameworkThread(() =>
			{
				this.Address = DalamudServices.ObjectTable?.GetObjectAddress(this.ObjectTableIndex) ?? IntPtr.Zero;

				if (!this.IsValid)
					return;

				TargetSystem.Instance()->GPoseTarget = (NativeObject*)this.Character;
			});
		}
	}

	public void OnFrameworkUpdate()
	{
		this.Address = DalamudServices.ObjectTable?.GetObjectAddress(this.ObjectTableIndex) ?? IntPtr.Zero;
	}
}