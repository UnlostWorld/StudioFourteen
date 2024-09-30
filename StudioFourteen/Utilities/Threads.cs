namespace ScreenshotStudio.Utilities;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfUtils;

public static class Threads
{
	public static bool IsFrameworkThread => DalamudServices.Framework?.IsInFrameworkUpdateThread == true;

	public static SwitchToFrameworkThreadAwaitable FrameworkThread() => new();
	public static Dispatch.SwitchFromUiAwaitable NonUiThread() => Dispatch.NonUiThread();
	public static Dispatch.SwitchToMainThreadAwaitable UiThread(DispatcherObject obj) => Dispatch.MainThread(obj);

	public static Task RunOnFrameworkThread(Action action)
	{
		if (!IsFrameworkThread)
		{
			return DalamudServices.Framework?.RunOnFrameworkThread(action) ?? Task.CompletedTask;
		}

		action.Invoke();
		return Task.CompletedTask;
	}

	public static unsafe Task RunOnFrameworkThread(this Character character, Action<nint> action)
	{
		return RunOnFrameworkThread(character.GameObject, action);
	}

	public static unsafe Task RunOnFrameworkThread(Character* character, Action<nint> action)
	{
		return RunOnFrameworkThread(character->GameObject, action);
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

	public static async Task NextFrame()
	{
		await Task.Delay(5);
		await FrameworkThread();
	}

	public struct SwitchToFrameworkThreadAwaitable : INotifyCompletion
	{
		public SwitchToFrameworkThreadAwaitable()
		{
		}

		public bool IsCompleted => Threads.IsFrameworkThread;

		public SwitchToFrameworkThreadAwaitable GetAwaiter() => this;
		public void GetResult()
		{
		}

		public void OnCompleted(Action continuation)
		{
			Threads.RunOnFrameworkThread(continuation);
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