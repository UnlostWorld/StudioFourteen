namespace ScreenshotStudio.Studio.Background;

using Dalamud.Plugin.Services;
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
		for (int i = GroupPoseService.GPoseFirstActor; i < GroupPoseService.GPoseFirstActor + GroupPoseService.GPoseActorCount; ++i)
		{
			this.Actors.Add(new(i));
		}
	}

	public List<ActorViewModel> Actors { get; init; } = new();

	[AutoNotify] public bool IsInGPose => this.Services.Studio.IsOpenAndInGPose;

	[AutoNotify]
	public ActorViewModel? Target
	{
		get
		{
			foreach (ActorViewModel vm in this.Actors)
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

		foreach (ActorViewModel actor in this.Actors)
		{
			actor.OnFrameworkUpdate();
		}
	}

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		LibraryModal.Show<IActorAppearance>(
			sender,
			"Create Actor",
			defaultTags,
			null,
			(appearance, isFinal) =>
			{
				if (!isFinal)
					return;

				this.CreateActor(appearance);
			});
	}

	private unsafe void CreateActor(IActorAppearance appearance)
	{
		Threads.RunOnFrameworkThread(() =>
		{
			Actor* actor = this.Services.ActorLifecycle.Create(appearance);
			int index = actor->GameObject.ObjectIndex;
			this.SelectObject(index);
		});
	}

	private unsafe void OnRemoveActorClicked(object sender, RoutedEventArgs e)
	{
		ActorViewModel? target = this.Target;
		if (target == null)
			return;

		int index = this.Actors.IndexOf(target);
		this.Services.ActorLifecycle.Destroy(target.Actor);
		this.SelectNearest(index);
	}

	private void SelectNearest(int index)
	{
		Task.Run(async () =>
		{
			await Task.Delay(100);

			for (int i = index; i < this.Actors.Count; i++)
			{
				if (!this.Actors[i].IsValid)
					continue;

				this.Actors[i].IsCurrent = true;
				break;
			}

			if (this.Target == null)
			{
				for (int i = index; i >= 0; i--)
				{
					if (!this.Actors[i].IsValid)
						continue;

					this.Actors[i].IsCurrent = true;
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

			foreach (ActorViewModel vm in this.Actors)
			{
				if (vm.ObjectTableIndex == index)
				{
					vm.IsCurrent = true;
				}
			}
		});
	}
}

public unsafe class ActorViewModel : ViewModel
{
	public readonly int ObjectTableIndex = 0;
	private string? lastName;

	public ActorViewModel(int index)
	{
		this.ObjectTableIndex = index;
	}

	[AutoNotify] public IntPtr Address { get; set; } = IntPtr.Zero;
	[AutoNotify] public bool IsValid => this.Address != IntPtr.Zero;
	[AutoNotify] public unsafe Actor* Actor => (Actor*)this.Address;

	[AutoNotify]
	public string? Name
	{
		get
		{
			if (!this.IsValid)
				return this.lastName ?? "Invalid";

			this.lastName = this.Actor->Name ?? "???";

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

			return (Actor*)TargetSystem.Instance()->GPoseTarget == this.Actor;
		}

		set
		{
			Threads.RunOnFrameworkThread(() =>
			{
				this.Address = DalamudServices.ObjectTable?.GetObjectAddress(this.ObjectTableIndex) ?? IntPtr.Zero;

				if (!this.IsValid)
					return;

				TargetSystem.Instance()->GPoseTarget = (NativeObject*)this.Actor;
			});
		}
	}

	public void OnFrameworkUpdate()
	{
		this.Address = DalamudServices.ObjectTable?.GetObjectAddress(this.ObjectTableIndex) ?? IntPtr.Zero;
	}
}