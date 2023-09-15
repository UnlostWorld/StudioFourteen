// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Services;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

public class PersistentPanel : Panel
{
	private readonly string panelId;

	public PersistentPanel()
	{
		this.panelId = this.GetType().Name;
	}

	public T? GetPersistence<T>([CallerMemberName]string id = "")
	{
		try
		{
			string persistenceId = this.panelId + "_" + id;

			if (!Settings.Current.PanelPersistence.TryGetValue(persistenceId, out string? json) || json == null)
				return default;

			return JsonConvert.DeserializeObject<T>(json);
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
			string persistenceId = this.panelId + "_" + id;

			if (!Settings.Current.PanelPersistence.ContainsKey(persistenceId))
				Settings.Current.PanelPersistence.Add(persistenceId, string.Empty);

			Settings.Current.PanelPersistence[persistenceId] = JsonConvert.SerializeObject(value);
			Settings.Current.Save();

			this.Log.Information($" Save {persistenceId} - {Settings.Current.PanelPersistence[persistenceId]}");
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
		}
	}
}