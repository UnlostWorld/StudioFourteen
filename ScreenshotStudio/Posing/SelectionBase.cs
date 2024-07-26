namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;

public abstract class SelectionBase
{
	public abstract string Name { get; }

	public abstract hkQsTransformf Transform { get; set; }
	public abstract bool LockTransform { get; set; }

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}
}
