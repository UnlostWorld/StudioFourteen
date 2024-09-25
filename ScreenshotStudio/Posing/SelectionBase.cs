namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Mvm;
using System.Numerics;

public abstract class SelectionBase : ViewModel
{
	[AutoNotify] public abstract string Name { get; }
	[AutoNotify] public abstract string? Subtitle { get; }

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}

	public virtual void OnFrameworkUpdate(IFramework framework)
	{
	}
}

public abstract class TransformSelectionBase : SelectionBase
{
	public abstract Vector3 WorldTranslation { get; set; }
	public abstract Quaternion WorldRotation { get; set; }
	public abstract Vector3 WorldScale { get; set; }

	public abstract Vector3 LocalTranslation { get; set; }
	public abstract Quaternion LocalRotation { get; set; }
	public abstract Vector3 LocalScale { get; set; }

	[AutoNotify] public abstract bool LockTransform { get; set; }
	[AutoNotify] public virtual bool CanLockTransform => true;

	public virtual double TranslationLargeChange => 0.1;
	public virtual double TranslationSmallChange => 0.01;
	public virtual double TranslationRange => 1;
	public virtual int DecimalPlacesToDisplay => 2;
	public virtual PoseEditModes DefaultEditMode => PoseEditModes.Translation;
}