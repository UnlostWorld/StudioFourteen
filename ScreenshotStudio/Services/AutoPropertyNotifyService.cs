namespace ScreenshotStudio.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

public interface IAutoNotify : INotifyPropertyChanged
{
	void NotifyPropertyChanged(string propertyName);
	bool ShouldTickAutoProperties();
}

public class AutoPropertyNotifyService : ServiceBase
{
	private static readonly List<TrackedObject> TrackedObjects = new();
	private static readonly List<TrackedObject> DeadObjects = new();

	public static void Register(IAutoNotify obj)
	{
		try
		{
			lock (TrackedObjects)
			{
				TrackedObjects.Add(new(obj));
			}
		}
		catch(Exception ex)
		{
			Logging.Shared.Error(ex, "Error in AutoPropertyNotify Register");
		}
	}

	public static void Remove(IAutoNotify obj)
	{
		lock (TrackedObjects)
		{
			foreach (TrackedObject trackedObject in TrackedObjects)
			{
				if (trackedObject.Object.TryGetTarget(out IAutoNotify? target) && target == obj)
				{
					Remove(trackedObject);
				}
			}
		}
	}

	public override Task Stop()
	{
		lock (TrackedObjects)
		{
			TrackedObjects.Clear();
		}

		DeadObjects.Clear();
		return base.Stop();
	}

	public override Task Tick()
	{
		try
		{
			if (TrackedObjects.Count > 0)
			{
				List<TrackedObject> objects;
				lock (TrackedObjects)
				{
					objects = new(TrackedObjects);
				}

				for (int i = objects.Count - 1; i >= 0; i--)
				{
					bool alive = objects[i].Tick();
					if (!alive)
					{
						Remove(objects[i]);
					}
				}
			}

			lock (TrackedObjects)
			{
				foreach (TrackedObject tracked in DeadObjects)
				{
					TrackedObjects.Remove(tracked);
				}
			}

			DeadObjects.Clear();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "error ticking auto properties");
		}

		return base.Tick();
	}

	private static void Remove(TrackedObject obj)
	{
		DeadObjects.Add(obj);
	}

	private class TrackedObject
	{
		public readonly WeakReference<IAutoNotify> Object;
		public readonly List<PropertyInfo> Properties = new();
		public readonly List<PropertyInfo> AlwaysProperties = new();

		private readonly Dictionary<PropertyInfo, object?> lastValues = new();

		public TrackedObject(IAutoNotify obj)
		{
			this.Object = new WeakReference<IAutoNotify>(obj);

			// TODO: cache property lookups by target type
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				AutoNotifyAttribute? attribute = property.GetCustomAttribute<AutoNotifyAttribute>();
				if (attribute != null)
				{
					this.Properties.Add(property);
					this.lastValues.TryAdd(property, null);
				}

				AlwaysNotifyAttribute? alwaysAttribute = property.GetCustomAttribute<AlwaysNotifyAttribute>();
				if (alwaysAttribute != null)
				{
					this.AlwaysProperties.Add(property);
					this.lastValues.TryAdd(property, null);
				}
			}
		}

		public bool Tick()
		{
			if (!this.Object.TryGetTarget(out IAutoNotify? notify))
				return false;

			// If the target object has a dispatcher, use that instead of our own thread.
			if (notify is DispatcherObject dispatcherObj)
			{
				if (dispatcherObj.Dispatcher.HasShutdownStarted)
					return false;

				try
				{
					dispatcherObj.Dispatcher.Invoke(() => this.TickProperties(notify));
				}
				catch (TaskCanceledException)
				{
				}
			}
			else
			{
				this.TickProperties(notify);
			}

			return true;
		}

		private void TickProperties(IAutoNotify notify)
		{
			foreach (PropertyInfo property in this.AlwaysProperties)
			{
				this.Tick(property, notify);
			}

			if (!notify.ShouldTickAutoProperties())
				return;

			foreach (PropertyInfo property in this.Properties)
			{
				this.Tick(property, notify);
			}

			return;
		}

		private void Tick(PropertyInfo property, IAutoNotify notify)
		{
			object? currentVal = property.GetValue(notify);

			this.lastValues.TryGetValue(property, out object? lastValue);

			if (currentVal is null && lastValue is not null)
			{
				this.lastValues[property] = currentVal;
				notify.NotifyPropertyChanged(property.Name);
			}

			if (currentVal is not null && !currentVal.Equals(lastValue))
			{
				////Logging.Shared.Information($"Changed {property.Name} from {lastValue} to {currentVal}");
				this.lastValues[property] = currentVal;
				notify.NotifyPropertyChanged(property.Name);
			}
		}
	}
}

[AttributeUsage(AttributeTargets.Property)]
public class AutoNotifyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public class AlwaysNotifyAttribute : Attribute
{
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

	public virtual bool ShouldTickAutoProperties()
	{
		return true;
	}
}
