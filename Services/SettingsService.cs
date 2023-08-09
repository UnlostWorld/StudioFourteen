// © Anamnesis.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dalamud.Configuration;
using PropertyChanged.SourceGenerator;

[Serializable]
public class Settings : IPluginConfiguration
{
	public int Version { get; set; } = 0;
	public double Opacity { get; set; } = 1.0;

	public Dictionary<string, PanelService.PanelSettings> Panels { get; set; } = new();
}

public partial class SettingsService : ServiceBase
{
	public static event PropertyChangedEventHandler? SettingsChanged;

	[Notify] private Settings current;

	public SettingsService()
	{
		current = new();

		////current = Plugin.PluginInterface.GetPluginConfig() as Settings ?? new Settings();
	}

	public void Save()
	{
		Plugin.PluginInterface.SavePluginConfig(this.Current);
	}
}