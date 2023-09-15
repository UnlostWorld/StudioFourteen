// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using Dalamud.Configuration;
using ScreenshotStudio.Plugin;
using System.Collections.Generic;

public class Settings : IPluginConfiguration
{
	private static Settings? current;

	public static Settings Current
	{
		get
		{
			if (current == null)
			{
				current = DalamudServices.PluginInterface.GetPluginConfig() as Settings;

				if (current == null)
				{
					current = new();
					current.Save();
				}
			}

			return current;
		}
	}

	// Settings
	public int Version { get; set; } = 0;

	public Dictionary<string, string> PanelPersistence { get; set; } = new();

	public void Save()
	{
		DalamudServices.PluginInterface.SavePluginConfig(this);
	}
}