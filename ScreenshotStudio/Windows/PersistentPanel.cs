// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Services;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
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

			if (!Settings.Current.PanelPersistence.TryGetValue(persistenceId, out string? json) || json == null)
				return default;

			T? value = JsonConvert.DeserializeObject<T>(json);
			this.persistenceCache.Add(id, value);
			return value;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
			return default;
		}
	}

	public void SetPersistence(object value, [CallerMemberName] string id = "")
	{
		this.SetPersistence(id, value);
	}

	public void SetPersistence(string id, object value)
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

				if (!Settings.Current.PanelPersistence.ContainsKey(persistenceId))
					Settings.Current.PanelPersistence.Add(persistenceId, string.Empty);

				Settings.Current.PanelPersistence[persistenceId] = JsonConvert.SerializeObject(value);
			});

			Settings.Current.Save();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
		}
	}
}