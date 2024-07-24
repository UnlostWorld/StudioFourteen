namespace ScreenshotStudio.Windows;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScreenshotStudio.Serialization;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio;

public class PersistentPanel : Panel
{
	private readonly string panelId;
	private readonly Dictionary<string, object?> persistenceCache = new();

	public PersistentPanel()
	{
		this.panelId = this.GetType().Name;
	}

	public T? GetPersistence<T>([CallerMemberName]string id = "")
	{
		try
		{
			lock (this.persistenceCache)
			{
				if (this.persistenceCache.ContainsKey(id))
				{
					return (T?)this.persistenceCache[id];
				}
			}

			string persistenceId = this.panelId + "_" + id;

			if (!this.Services.Settings.Current.PanelPersistence.TryGetValue(persistenceId, out string? json) || json == null)
				return default;

			if (!json.StartsWith('"') || !json.EndsWith('"'))
				json = '"' + json + '"';

			T? value = Serializer.Deserialize<T>(json);
			this.persistenceCache.Add(id, value);
			return value;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
			return default;
		}
	}

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.SetPersistence(id, value);
	}

	public void SetPersistence(string id, object? value)
	{
		try
		{
			lock (this.persistenceCache)
			{
				if (!this.persistenceCache.ContainsKey(id))
					this.persistenceCache.Add(id, value);

				this.persistenceCache[id] = value;
			}

			this.Dispatcher.Invoke(() =>
			{
				string persistenceId = this.panelId + "_" + id;

				if (value != null)
				{
					if (!this.Services.Settings.Current.PanelPersistence.ContainsKey(persistenceId))
						this.Services.Settings.Current.PanelPersistence.Add(persistenceId, string.Empty);

					this.Services.Settings.Current.PanelPersistence[persistenceId] = Serializer.Serialize(value);
				}
				else
				{
					if (this.Services.Settings.Current.PanelPersistence.ContainsKey(persistenceId))
					{
						this.Services.Settings.Current.PanelPersistence.Remove(persistenceId);
					}
				}
			});

			this.Services.Settings.Save();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
		}
	}
}