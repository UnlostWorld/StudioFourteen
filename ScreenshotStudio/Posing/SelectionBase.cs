namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using System.Numerics;

public abstract class SelectionBase
{
	public abstract string Name { get; }

	public abstract Vector3 Translation { get; set; }
	public abstract Quaternion Rotation { get; set; }
	public abstract Vector3 Scale { get; set; }

	public abstract bool LockTransform { get; set; }

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
