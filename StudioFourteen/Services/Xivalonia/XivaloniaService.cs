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
using StudioFourteen.Services.Content;
using System.Reflection;
using Avalonia.Markup.Xaml;
using System.Collections;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using StudioFourteen.Services.Dalamud;

public partial class XivaloniaService : IService, IPlatformLifetimeEventsImpl
{
	private readonly XamlContentReference<Window> testWindowReference = new("UI/TestWindow.axaml");
	private readonly CancellationTokenSource cts = new();
	private readonly Thread? uiThread;

	private DispatcherImpl? dispatcher;
	private RenderTimer? renderTimer;
	private WindowingPlatform? windowing;

	public XivaloniaService()
	{
		ThreadStart ts = new(this.StartImpl);
		this.uiThread = new Thread(ts);
		this.uiThread.Start();
	}

	public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

	public DispatcherImpl Dispatcher => this.dispatcher ?? throw new Exception("Xivalonia not initalized");

	public void Dispose()
	{
		this.ShutdownRequested?.Invoke(this, new ShutdownRequestedEventArgs());

		this.windowing?.Dispose();
		this.cts.Cancel();
		this.renderTimer?.Dispose();

		AvaloniaLocator.Current = null!;
		AvaloniaLocator.CurrentMutable = null!;
	}

	private void StartImpl()
	{
		try
		{
			while (Studio.Rendering.OverlayRenderer.BackBuffer == null)
			{
				Thread.Sleep(10);
			}

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
				app.UseWindowingSubsystem(this.InitializeWindowing, "Xivalonia");
			}

			app.UseSkia();

			Studio.Log.Information($"Starting Xivalonia");

			string[] args = [];
			app.Start(
				(main, args) =>
				{
					// Ready to run!
					this.LoadTypes();

					try
					{
						Window wnd = this.testWindowReference.Get();
						wnd.Show();
					}
					catch (Exception ex)
					{
						Studio.Log.Error(ex, "Error test");
					}

					main.Run(this.cts.Token);

					Studio.Log.Information($"Bye!");
				},
				args);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "error in main");
		}
	}

	private void InitializeWindowing()
	{
		this.renderTimer = new(TimeSpan.FromSeconds(1.0 / 60.0));
		this.dispatcher = new();
		this.windowing = new();

		AvaloniaLocator.CurrentMutable.Bind<IScreenImpl>().ToSingleton<ScreenImpl>();
		AvaloniaLocator.CurrentMutable.Bind<IDispatcherImpl>().ToFunc(() => this.dispatcher);
		AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToFunc(() => this.renderTimer);
		AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToFunc(() => this.windowing);
		AvaloniaLocator.CurrentMutable.Bind<IPlatformLifetimeEventsImpl>().ToFunc(() => this);
		AvaloniaLocator.CurrentMutable.Bind<ICursorFactory>().ToConstant(new CursorFactory());

		IPlatformGraphics? platformGraphics = GlManager.Initialize();
		AvaloniaLocator.CurrentMutable.Bind<Compositor>().ToConstant(new Compositor(platformGraphics));
	}

	// ⚠️ WARNING: REFLECTION BASED CRIMES ⚠️
	// Normally the SreTypeSystem simply iterates over every loaded assembly in the current domain, but doing that inside
	// Dalamud will return assemblies from other plugins and also earlier versions of Studio that have been unloaded.
	// We do this reflection horror here since we cant just implement IXamlTypeSystem ourselves unfortunately.
	// https://github.com/kekekeks/XamlX/blob/master/src/XamlX/IL/SreTypeSystem.cs
	// https://github.com/AvaloniaUI/Avalonia/blob/master/src/Markup/Avalonia.Markup.Xaml.Loader/AvaloniaXamlIlRuntimeCompiler.cs
	private void LoadTypes()
	{
		Type? type = typeof(AvaloniaRuntimeXamlLoader).Assembly.GetType("Avalonia.Markup.Xaml.XamlIl.AvaloniaXamlIlRuntimeCompiler");
		if (type == null)
			throw new Exception("Failed to locate AvaloniaXamlIlRuntimeCompiler");

		FieldInfo? typeSystemField = type.GetField("_sreTypeSystem", BindingFlags.Static | BindingFlags.NonPublic);
		if (typeSystemField == null)
			throw new Exception("Failed to get sre Type System");

		Type sreTypeSystemType = typeSystemField.FieldType;
		object? sreTypeSystem = Activator.CreateInstance(sreTypeSystemType);
		typeSystemField.SetValue(null, sreTypeSystem);

		FieldInfo? assembliesField = sreTypeSystemType.GetField("_assemblies", BindingFlags.NonPublic | BindingFlags.Instance);
		if (assembliesField == null)
			throw new Exception("Failed to find _assemblies field on SreTypeSystem");

		IList? assembliesList = assembliesField.GetValue(sreTypeSystem) as IList;
		if (assembliesList == null)
			throw new Exception("Failed to get assemblies list");

		assembliesList.Clear();

		MethodInfo? method = sreTypeSystemType.GetMethod("ResolveAssembly", BindingFlags.NonPublic | BindingFlags.Instance);
		if (method == null)
			throw new Exception("Failed to find ResolveAssembly method on SreTypeSystem");

		// Hard coded assemblies
		method.Invoke(sreTypeSystem, [typeof(System.IServiceProvider).Assembly]);
		method.Invoke(sreTypeSystem, [typeof(System.ComponentModel.ITypeDescriptorContext).Assembly]);
		method.Invoke(sreTypeSystem, [typeof(System.ComponentModel.ISupportInitialize).Assembly]);
		method.Invoke(sreTypeSystem, [typeof(System.ComponentModel.TypeConverterAttribute).Assembly]);
		method.Invoke(sreTypeSystem, [typeof(System.Collections.Generic.IList<object>).Assembly]);
		method.Invoke(sreTypeSystem, [typeof(System.Uri).Assembly]);

		PluginManager.LocalPlugin localPlugin = PluginManager.GetLocalPlugin();
		int count = 0;
		foreach (Assembly asm in localPlugin.LoadContext.Assemblies)
		{
			try
			{
				method.Invoke(sreTypeSystem, [asm]);
				count++;
			}
			catch
			{
			}
		}

		Studio.Log.Information($"Resolved {count} assemblies for the SreTypeSystem");
	}
}