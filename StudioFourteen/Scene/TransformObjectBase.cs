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

using CommunityToolkit.Mvvm.ComponentModel;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Scene;

[Inspect("Icons/SceneObject.svg")]
public abstract partial class TransformObjectBase : SceneObjectBase
{
	private Transform? initialWorldTransform = null;

	[ObservableProperty]
	[Inspect("UI/Inspectors/Transform.ui")]
	public partial Transform WorldTransform { get; set; }

	[ObservableProperty]
	[Inspect("UI/Inspectors/Transform.ui")]
	public partial Transform LocalTransform { get; set; }

	[ObservableProperty]
	[Inspect]
	public partial bool LockTransform { get; set; }

	public virtual double TranslationChange => 0.1;
	public virtual int DecimalPlacesToDisplay => 2;
	////public virtual TransformHandleTypes DefaultGizmo => TransformHandleTypes.Translation;
	public virtual double GizmoSensitivity => 1.0;

	public override void Reset()
	{
		if (this.initialWorldTransform != null)
		{
			this.WorldTransform = (Transform)this.initialWorldTransform;
		}

		base.Reset();
	}

	protected virtual void WorldTransformChanged(Transform oldValue, Transform newValue)
	{
		if (this.initialWorldTransform == null && oldValue != Transform.Identity && oldValue != default)
		{
			this.initialWorldTransform = oldValue;
		}
	}

	protected virtual void LocalTransformChanged(Transform oldValue, Transform newValue)
	{
	}

	protected virtual void LockTransformChanged(bool oldValue, bool newValue)
	{
	}

	partial void OnWorldTransformChanged(Transform oldValue, Transform newValue)
	{
		this.WorldTransformChanged(oldValue, newValue);
	}

	partial void OnLocalTransformChanged(Transform oldValue, Transform newValue)
	{
		this.LocalTransformChanged(oldValue, newValue);
	}

	partial void OnLockTransformChanged(bool oldValue, bool newValue)
	{
		this.LockTransformChanged(oldValue, newValue);
	}
}