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

namespace StudioFourteen.Scene;

using System;
using System.Collections.Generic;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Utilities;
using WpfUtils.Commands;

public abstract partial class SceneObjectBase : ViewModel, IDisposable
{
	[Notify] private string name = string.Empty;
	[Notify(Setter.Protected)] private string? subtitle;
	[Notify(Setter.Protected)] private string? description;
	[Notify(Setter.Protected)] private bool isReady = false;
	[Notify(Setter.Private)] private bool isHovered;
	[Notify(Setter.Private)] private bool isSelected;

	public SceneObjectBase()
	{
		this.ResetCommand = new(this.Reset);
	}

	public abstract string Id { get; }
	public abstract object? Icon { get; }
	public abstract string TypeName { get; }

	public SimpleCommand ResetCommand { get; init; }
	public List<GizmoBase> Gizmos { get; init; } = new();

	public virtual void Dispose()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void OnSelected(bool value)
	{
		this.IsSelected = value;
	}

	public virtual void OnHovered(bool value)
	{
		this.IsHovered = value;
	}

	public virtual void OnGameTick()
	{
	}

	public bool Equals(SceneObjectBase? other)
	{
		return this.Id == other?.Id;
	}

	public virtual bool IsHit(HitInfo hitInfo) => false;
}
