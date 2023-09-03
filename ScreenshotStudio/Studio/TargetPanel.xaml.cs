// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public partial class TargetPanel : DockPanel
{
	public const int GPoseActorCount = 39;
	public const int GPoseFirstActor = 201;

	public TargetPanel()
	{
		for (int i = GPoseFirstActor; i < GPoseFirstActor + GPoseActorCount; ++i)
		{
			this.Actors.Add(new(i));
		}
	}

	public List<ActorViewModel> Actors { get; init; } = new();

	[AutoNotify]
	public bool IsInGPose
	{
		get
		{
			if (!this.Services.Studio.IsOpen)
				return false;

			return DalamudServices.PluginInterface.UiBuilder.GposeActive;
		}
	}

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		QuickSearch.Show<IActorAppearance>(
			sender,
			"Create Actor",
			defaultTags,
			null,
			(appearance, isFinal) =>
			{
				if (!isFinal)
					return;

				this.Services.ActorLifecycle.Create(appearance);
			});
	}

	private void OnRemoveActorClicked(object sender, RoutedEventArgs e)
	{
		this.Services.ActorLifecycle.DestroyAllCreated();
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

	[AutoNotify] public IntPtr Address => DalamudServices.ObjectTable.GetObjectAddress(this.ObjectTableIndex);
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
			if (!this.IsValid)
				return;

			TargetSystem.Instance()->GPoseTarget = (NativeObject*)this.Actor;
		}
	}
}