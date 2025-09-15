// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Scripting;

using Serilog.Events;
using StudioFourteen.Panels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Extensions;

public partial class ScriptPanel : Panel
{
	private bool trust = false;
	private bool run = false;

	public ScriptPanel()
	{
		this.Status = string.Empty;
		this.Progress = 0;
		this.IsIndeterminate = true;
		this.IsInfo = true;
		this.IsTrustPrompt = false;
		this.IsConfigurePrompt = false;
		this.AlwaysTrust = false;
		this.IsRunning = false;
	}

	[Bind] public partial ScriptFile? Script { get; set; }
	[Bind] public partial string Status { get; set; }
	[Bind] public partial double Progress { get; set; }
	[Bind] public partial bool IsIndeterminate { get; set; }
	[Bind] public partial bool IsInfo { get; set; }
	[Bind] public partial bool IsTrustPrompt { get; set; }
	[Bind] public partial bool IsConfigurePrompt { get; set; }
	[Bind] public partial bool AlwaysTrust { get; set; }
	[Bind] public partial bool IsRunning { get; set; }

	public FastObservableCollection<LogEntry> ScriptLog { get; init; } = new();
	public FastObservableCollection<OptionBase> Options { get; init; } = new();

	public static async Task<ScriptPanel?> Show(ScriptFile script)
	{
		ScriptPanel? panel = await ServiceManager.Instance.Panels.GamePanels.CreatePanelAsync<ScriptPanel>();
		if (panel != null)
		{
			panel.Script = script;
		}

		return panel;
	}

	public void SetTitle(string title)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Subtitle = title;
		});
	}

	public void SetStatus(string status)
	{
		this.Status = status;

		this.AppendLog(LogEventLevel.Information, status);
	}

	public void SetProgress(double? progress)
	{
		if (progress == null)
		{
			this.IsIndeterminate = true;
		}
		else
		{
			this.IsIndeterminate = false;
			this.Progress = (double)progress * 100;
		}
	}

	public void AppendLog(LogEventLevel level, string message, string? location = null)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.ScriptLog.Add(new(level, message, location));
			this.Scroller.ScrollToBottom();
		});
	}

	public async Task<bool> CheckTrust()
	{
		this.IsInfo = false;
		this.IsTrustPrompt = true;
		while (this.IsTrustPrompt)
			await Task.Delay(100);

		this.IsInfo = true;

		return this.trust;
	}

	public async Task<Dictionary<string, object>?> Configure()
	{
		if (this.Script == null)
			throw new InvalidOperationException();

		await this.Dispatcher.InvokeAsync(() =>
		{
			foreach (ScriptOption option in this.Script.Options)
			{
				if (option.Name == null)
					continue;

				switch (option.Type)
				{
					case ScriptOption.Types.CheckBox:
						{
							this.Options.Add(new CheckBoxOption(option.Name, option.ToolTip));
							break;
						}

					case ScriptOption.Types.Toggle:
						{
							this.Options.Add(new ToggleOption(option.Name, option.ToolTip));
							break;
						}

					case ScriptOption.Types.Input:
						{
							this.Options.Add(new InputOption(option.Name, option.ToolTip));
							break;
						}
				}
			}
		});

		this.IsInfo = false;
		this.IsConfigurePrompt = true;
		while (this.IsConfigurePrompt)
			await Task.Delay(100);

		this.IsInfo = true;

		if (!this.run)
			return null;

		Dictionary<string, object> results = new();
		foreach (OptionBase option in this.Options)
		{
			results.Add(option.Name, option.Value);
		}

		return results;
	}

	protected override void OnClosed()
	{
		this.IsConfigurePrompt = false;
		this.IsTrustPrompt = false;
		this.IsInfo = true;
		this.trust = false;
		this.run = false;
		base.OnClosed();
	}

	private void OnTrustClicked(object sender, RoutedEventArgs e)
	{
		this.IsTrustPrompt = false;
		this.IsInfo = true;
		this.trust = true;
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void OnRunClicked(object sender, RoutedEventArgs e)
	{
		this.IsConfigurePrompt = false;
		this.IsInfo = true;
		this.run = true;
	}

	private void OnCancelClicked(object sender, RoutedEventArgs e)
	{
		this.Script?.Cancel();
	}
}

public class LogEntry(LogEventLevel level, string message, string? location)
{
	public LogEventLevel Level { get; init; } = level;
	public string Message { get; init; } = message;
	public string? Location { get; init; } = location;
}

public abstract class OptionBase(string name, string? toolTip)
{
	public string Name { get; init; } = name;
	public string? ToolTip { get; init; } = toolTip;
	public abstract object Value { get; }
}

public class CheckBoxOption(string name, string? toolTip)
	: OptionBase(name, toolTip)
{
	public bool IsChecked { get; set; }
	public override object Value => this.IsChecked;
}

public class ToggleOption(string name, string? toolTip)
	: OptionBase(name, toolTip)
{
	public bool IsChecked { get; set; }
	public override object Value => this.IsChecked;
}

public class InputOption(string name, string? toolTip)
	: OptionBase(name, toolTip)
{
	public string Text { get; set; } = string.Empty;
	public override object Value => this.Text;
}