namespace ScreenshotStudio.Services;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;

public abstract class SelectionBase
{
	public abstract string Name { get; }

	public abstract ref hkQsTransformf Transform { get; }
	public abstract bool LockTransform { get; set; }

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}
}
