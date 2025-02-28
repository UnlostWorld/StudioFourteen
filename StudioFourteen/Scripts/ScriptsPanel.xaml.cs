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

namespace StudioFourteen.Scripts;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Utilities;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

public partial class ScriptsPanel : Panel
{
	public FastObservableCollection<ScriptEntry> Scripts { get; init; } = new();

	protected override void OnOpened()
	{
		this.Populate().Run();
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();
	}

	private async Task Populate()
	{
		await Threads.NonUiThread();

		List<ScriptEntry> scripts = new();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				foreach (Type t in assembly.GetExportedTypes())
				{
					if (t.IsAssignableTo(typeof(ScriptBase)) && !t.IsAbstract)
					{
						ScriptEntry entry = new();
						entry.Name = t.Name;
						entry.Type = t;
						scripts.Add(entry);
					}
				}
			}
			catch(Exception)
			{
			}
		}

		await this.MainThread();
		this.Scripts.Replace(scripts);
	}

	private void OnScriptClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.DataContext is ScriptEntry entry)
		{
			entry.Run().Run();
		}
	}

	public partial class ScriptEntry
	{
		[Notify] private string? name;
		[Notify] private Type? type;
		[Notify] private bool isRunning;

		public async Task Run()
		{
			if (this.Type == null)
				return;

			this.IsRunning = true;

			ScriptBase? script = Activator.CreateInstance(this.Type) as ScriptBase;
			if (script != null)
				await script.RunScript();

			this.IsRunning = false;
		}
	}
}