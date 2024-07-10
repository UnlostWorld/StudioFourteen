namespace ScreenshotStudio.Utilities;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using System;
using System.Threading.Tasks;

public static class Threads
{
	public static bool IsFrameworkThread => DalamudServices.Framework?.IsInFrameworkUpdateThread == true;
	public static Task RunOnFrameworkThread(Action action)
	{
		if (!IsFrameworkThread)
		{
			return DalamudServices.Framework?.RunOnFrameworkThread(action) ?? Task.CompletedTask;
		}

		action.Invoke();
		return Task.CompletedTask;
	}

	public static unsafe Task RunOnFrameworkThread(this Structs.Actor actor, Action<nint> action)
	{
		return RunOnFrameworkThread(actor.GameObject, action);
	}

	public static unsafe Task RunOnFrameworkThread(Structs.Actor* actor, Action<nint> action)
	{
		return RunOnFrameworkThread(actor->GameObject, action);
	}

	public static Task RunOnFrameworkThread(GameObject obj, Action<nint> action)
	{
		if (DalamudServices.Framework == null ||
			DalamudServices.ObjectTable == null)
			return Task.CompletedTask;

		int objectId = obj.ObjectIndex;
		return DalamudServices.Framework.RunOnFrameworkThread(() =>
		{
			action.Invoke(DalamudServices.ObjectTable.GetObjectAddress(objectId));
		});
	}

	public static void VerifyFrameworkThread()
	{
		if (!IsFrameworkThread)
		{
			throw new InvalidThreadException();
		}
	}
}

public class InvalidThreadException : Exception
{
	public InvalidThreadException()
		: base("Invalid Thread")
	{
	}
}
