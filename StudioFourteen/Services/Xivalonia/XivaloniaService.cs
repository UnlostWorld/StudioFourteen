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

namespace StudioFourteen.Services.Xivalonia;

using System;
using System.Threading;
using Avalonia;
using StudioFourteen.Services.Xivalonia.Platform;
using Avalonia.Controls;
using StudioFourteen.Services.Content;
using System.Reflection;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;

public partial class XivaloniaService : IService
{
	private readonly CancellationTokenSource cts = new();
	private readonly Thread? uiThread;

	private readonly XamlContentReference<Window> testWindowReference = new("UI/TestWindow.axaml");

	public XivaloniaService()
	{
		ThreadStart ts = new(this.StartImpl);
		this.uiThread = new Thread(ts);
		this.uiThread.Start();
	}

	public void Dispose()
	{
		XivaloniaPlatform.Stop();
		this.cts.CancelAfter(250);
	}

	private void StartImpl()
	{
		try
		{
			while (Studio.Rendering.OverlayRenderer.BackBuffer == null)
			{
				Thread.Sleep(1000);
			}

			Studio.Log.Information($"setting up Xivalonia");

			////Avalonia.Logging.Logger.Sink

			AppBuilder app = AppBuilder.Configure<App>();
			app.WithInterFont();
			app.LogToTrace();

			bool useWin32 = false;

			if (useWin32)
			{
				app.With<Win32PlatformOptions>(() =>
				{
					return new()
					{
						CompositionMode = [Win32CompositionMode.LowLatencyDxgiSwapChain],
					};
				});

				app.UseWin32();
			}
			else
			{
				app.UseStandardRuntimePlatformSubsystem();
				app.UseWindowingSubsystem(() => XivaloniaPlatform.Initialize(), "Xivalonia");
			}

			app.UseSkia();

			Studio.Log.Information($"Starting Xivalonia");

			string[] args = [];
			app.Start(
				(app2, args) =>
				{
					// Ready to run!
					this.HookTypeResolver();

					try
					{
						Window wnd = this.testWindowReference.Get();
						wnd.Show();
					}
					catch (Exception ex)
					{
						Studio.Log.Error(ex, "Error test");
					}

					app2.Run(this.cts.Token);

					Studio.Log.Information($"Bye!");
				},
				args);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "error in main");
		}
	}

	private void HookTypeResolver()
	{
		try
		{
			RuntimeXamlLoaderDocument doc = new("<Window xmlns=\"https://github.com/avaloniaui\"></Window>");
			AvaloniaRuntimeXamlLoader.Load(doc);
		}
		catch (Exception)
		{
		}

		Type? type = typeof(AvaloniaRuntimeXamlLoader).Assembly.GetType("Avalonia.Markup.Xaml.XamlIl.AvaloniaXamlIlRuntimeCompiler");
		if (type == null)
			throw new Exception("Failed to locate AvaloniaXamlIlRuntimeCompiler");

		// Redirect any xaml namespace assemblies to the current version, since we may have reloaded
		// the plugin multiple times during testing, and we always want to be using the latest version.
#if DEBUG
		{
			FieldInfo? xmlnsInfo = type.GetField("_sreXmlns", BindingFlags.NonPublic | BindingFlags.Static);
			if (xmlnsInfo == null)
				throw new Exception("Failed to locate _sreXmlns in AvaloniaXamlIlRuntimeCompiler");

			object? xmlnsSystem = xmlnsInfo.GetValue(null);
			if (xmlnsSystem == null)
				throw new Exception("Failed to get XMLNS System from Avalonia Xaml Il Runtime Compiler");

			PropertyInfo? namespacesPropertyInfo = xmlnsSystem.GetType().GetProperty("Namespaces");
			if (namespacesPropertyInfo == null)
				throw new Exception("Failed to locate Namespaces property on XamlXmlnsMappings");

			IDictionary? namespaces = namespacesPropertyInfo.GetValue(xmlnsSystem) as IDictionary;
			if (namespaces == null)
				throw new Exception("Failed to get namespaces dictionary");

			foreach (string xmlns in namespaces.Keys)
			{
				IList? values = namespaces[xmlns] as IList;
				if (values == null)
					continue;

				foreach (ITuple? value in values)
				{
					if (value == null)
						continue;

					object? sreAssembly = value[0];
					if (sreAssembly == null)
						continue;

					FieldInfo? assemblyFieldInfo = sreAssembly.GetType().GetField("<Assembly>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
					if (assemblyFieldInfo == null)
						throw new Exception("Failed to get assembly backing field");

					Assembly? asm = assemblyFieldInfo.GetValue(sreAssembly) as Assembly;
					if (asm == null)
						continue;

					Assembly asm2 = typeof(Window).Assembly;
					if (asm.FullName == asm2.FullName && asm != asm2)
					{
						assemblyFieldInfo.SetValue(sreAssembly, asm2);
						Studio.Log.Info($"Redirecting xaml assembly {asm}");
					}
				}
			}
		}
#endif
	}
}