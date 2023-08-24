// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;

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

			this.lastName = this.lastName.Replace(" Fullko", string.Empty);

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