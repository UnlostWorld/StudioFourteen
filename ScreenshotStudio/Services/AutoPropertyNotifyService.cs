// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public class AutoPropertyNotifyService : ServiceBase
{
	private static readonly List<TrackedObject> TrackedObjects = new();
	private static readonly List<TrackedObject> DeadObjects = new();

	public static void Register(IAutoNotify obj)
	{
		try
		{
			TrackedObjects.Add(new(obj));
		}
		catch(Exception ex)
		{
			Logging.Shared.Error(ex, "Error in AutoPropertyNotify Register");
		}
	}

	public static void Remove(IAutoNotify obj)
	{
		foreach(TrackedObject trackedObject in TrackedObjects)
		{
			if (trackedObject.Object.TryGetTarget(out IAutoNotify? target) && target == obj)
			{
				Remove(trackedObject);
			}
		}
	}

	public override async Task Start()
	{
		await base.Start();
		this.TickTask().Run();
	}

	public override Task Stop()
	{
		TrackedObjects.Clear();
		DeadObjects.Clear();
		return base.Stop();
	}

	private static void Remove(TrackedObject obj)
	{
		DeadObjects.Add(obj);
		Logging.Shared.Information($"remove: {obj}");
	}

	private async Task TickTask()
	{
		while (this.IsAlive)
		{
			await Task.Delay(10);

			try
			{
				for (int i = TrackedObjects.Count - 1; i >= 0; i--)
				{
					bool alive = TrackedObjects[i].Tick();
					if (!alive)
					{
						Remove(TrackedObjects[i]);
					}
				}

				foreach(TrackedObject tracked in DeadObjects)
				{
					TrackedObjects.Remove(tracked);
				}

				DeadObjects.Clear();
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "error ticking auto properties");
			}
		}
	}

	private class TrackedObject
	{
		public readonly WeakReference<IAutoNotify> Object;
		public readonly List<PropertyInfo> Properties = new();

		private object? lastValue = null;

		public TrackedObject(IAutoNotify obj)
		{
			this.Object = new WeakReference<IAutoNotify>(obj);

			// TODO: cache property lookups by target type
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				AutoNotifyAttribute? attribute = property.GetCustomAttribute<AutoNotifyAttribute>();
				if (attribute == null)
					continue;

				this.Properties.Add(property);
			}
		}

		public bool Tick()
		{
			if (!this.Object.TryGetTarget(out IAutoNotify? notify))
				return false;

			foreach (PropertyInfo property in this.Properties)
			{
				object? currentVal = property.GetValue(notify);
				if (currentVal == null)
					continue;

				if (!currentVal.Equals(this.lastValue))
				{
					this.lastValue = currentVal;
					notify.NotifyPropertyChanged(property.Name);
				}
			}

			return true;
		}
	}
}

[AttributeUsage(AttributeTargets.Property)]
public class AutoNotifyAttribute : Attribute
{
}

public interface IAutoNotify : INotifyPropertyChanged
{
	void NotifyPropertyChanged(string propertyName);
}

public class AutoNotify : IAutoNotify
{
	public AutoNotify()
	{
		AutoPropertyNotifyService.Register(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}
