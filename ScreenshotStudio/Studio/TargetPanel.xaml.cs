// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

public partial class TargetPanel : DockPanel
{
	public TargetPanel()
	{
		for (int i = TargetService.GPoseFirstActor; i < TargetService.GPoseFirstActor + TargetService.GPoseActorCount; ++i)
		{
			this.Actors.Add(new(i));
		}
	}

	public List<ActorViewModel> Actors { get; init; } = new();

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
    }

	private void OnRemoveActorClicked(object sender, RoutedEventArgs e)
	{
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

	[AutoNotify] public IntPtr Address => this.Services.Targets.GetObjectTable(this.ObjectTableIndex);
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

			return this.Services.Targets.GPoseTarget == this.Actor;
		}

		set
		{
			if (!this.IsValid)
				return;

			this.Services.Targets.GPoseTarget = this.Actor;
		}
	}
}