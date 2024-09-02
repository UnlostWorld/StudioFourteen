namespace ScreenshotStudio.Posing;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Services;
using System.Numerics;

public abstract class SelectionBase : ViewModel
{
	[AutoNotify] public abstract string Name { get; }
	[AutoNotify] public abstract string? Subtitle { get; }

	public abstract Vector3 WorldTranslation { get; set; }
	public abstract Quaternion WorldRotation { get; set; }
	public abstract Vector3 WorldScale { get; set; }

	public abstract Vector3 LocalTranslation { get; set; }
	public abstract Quaternion LocalRotation { get; set; }
	public abstract Vector3 LocalScale { get; set; }

	[AutoNotify] public abstract bool LockTransform { get; set; }
	[AutoNotify] public virtual bool CanLockTransform => true;

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
