namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using System.Numerics;

public abstract class SelectionBase
{
	public abstract string Name { get; }
	public abstract string? Subtitle { get; }

	public abstract Vector3 WorldTranslation { get; set; }
	public abstract Quaternion WorldRotation { get; set; }
	public abstract Vector3 WorldScale { get; set; }

	public abstract Vector3 LocalTranslation { get; set; }
	public abstract Quaternion LocalRotation { get; set; }
	public abstract Vector3 LocalScale { get; set; }

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
