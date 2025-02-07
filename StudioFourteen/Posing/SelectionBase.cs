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

namespace StudioFourteen.Posing;

using Dalamud.Plugin.Services;
using FontAwesome.Sharp;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.History;
using StudioFourteen.Mvm;
using System;

public abstract class SelectionBase : AutoViewModel, IEquatable<SelectionBase>, IHistoryTarget
{
	[AutoNotify] public abstract string Name { get; }
	[AutoNotify] public abstract string? Subtitle { get; }
	public abstract IconChar Icon { get; }

	public virtual bool CanMirror => false;
	[History] public virtual MirrorModes MirrorMode { get; set; }

	public abstract bool CanReset { get; }

	public virtual void Reset()
	{
	}

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}

	public virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	public virtual bool Equals(SelectionBase? other)
	{
		return this == other;
	}

	public Operation CreateHistoryOperation()
	{
		return new SelectionObjectOperation();
	}

	public class SelectionObjectOperation : Operation
	{
		public override IHistoryTarget GetTarget()
		{
			if (ServiceManager.Instance.Pose.Selection == null)
				throw new Exception("No selection");

			return ServiceManager.Instance.Pose.Selection;
		}

		public override bool IsTarget(IHistoryTarget target)
		{
			return ServiceManager.Instance.Pose.Selection == target;
		}
	}
}

public abstract class TransformSelectionBase : SelectionBase
{
	[History] public abstract Transform WorldTransform { get; set; }
	[History] public abstract Transform LocalTransform { get; set; }

	[History][AutoNotify] public abstract bool LockTransform { get; set; }
	[AutoNotify] public virtual bool CanLockTransform => true;

	public virtual double TranslationLargeChange => 0.1;
	public virtual double TranslationSmallChange => 0.01;
	public virtual double TranslationRange => 1;
	public virtual int DecimalPlacesToDisplay => 2;
	public virtual TransformHandleTypes DefaultGizmo => TransformHandleTypes.Translation;
	public virtual double GizmoSensitivity => 1.0;

	[AutoNotify] public virtual bool IsReady => true;
}