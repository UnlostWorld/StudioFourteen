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

using PropertyChanged.SourceGenerator;
using StudioFourteen.History;
using StudioFourteen.Mvm;
using StudioFourteen.Posing;
using StudioFourteen.Utilities;
using System;

public abstract partial class SceneObjectBase : ViewModel, IHistoryTarget
{
	[Notify(Setter.Protected)] private string name = string.Empty;
	[Notify(Setter.Protected)] private string? subtitle;
	[Notify(Setter.Protected)] private string? description;
	[Notify(Setter.Protected)] private bool isReady = false;
	[Notify(Setter.Private)] private bool isHovered;
	[Notify(Setter.Private)] private bool isSelected;

	public bool IsActive { get; private set; }

	public abstract object? Icon { get; }
	public abstract string TypeName { get; }

	public virtual bool CanMirror => false;
	[History] public virtual MirrorModes MirrorMode { get; set; }

	public abstract bool CanReset { get; }
	public abstract ISceneObjectId Id { get; }

	public virtual void Reset()
	{
	}

	public virtual void Activate()
	{
		this.IsActive = true;
	}

	public virtual void Deactivate()
	{
		this.IsActive = false;
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

	public virtual bool Equals(SceneObjectBase? other)
	{
		return this == other;
	}

	public Operation CreateHistoryOperation()
	{
		return new SelectionObjectOperation();
	}

	public virtual void FinalizeHistoryOperation(ref Operation operation)
	{
	}

	public virtual bool IsHit(HitInfo hitInfo) => false;

	public class SelectionObjectOperation : Operation
	{
		public override IHistoryTarget GetTarget()
		{
			if (ServiceManager.Instance.Selection.Current == null)
				throw new Exception("No selection");

			return ServiceManager.Instance.Selection.Current;
		}

		public override bool IsTarget(IHistoryTarget target)
		{
			return ServiceManager.Instance.Selection.Current == target;
		}
	}
}
