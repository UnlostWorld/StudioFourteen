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

namespace StudioFourteen.Services.Avalonia;

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Platform;
using global::Avalonia.Rendering;
using global::Avalonia.Rendering.Composition;
using global::Avalonia.Threading;
using StudioFourteen.Services.Dalamud;
using StudioFourteen.Services.Avalonia.Platform;

public partial class AvaloniaService : IService, IPlatformLifetimeEventsImpl
{
	public long DispatcherFramerate = 60;
	public long RenderFramerate = 60;

	public bool UseWin32 = false;

	// This will break plugin reloading.
	public bool UseWin32Hybrid = false;

	private readonly StudioWindow testWindow = new("UI/TestWindow.ui");
	private readonly UiPass renderingPass = new();
	private readonly CancellationTokenSource cts = new();
	private readonly Thread? uiThread;

	private DispatcherImpl? dispatcher;
	private RenderTimer? renderTimer;
	private WindowingPlatform? windowing;
	private StudioScreens? screen;
	private Compositor? compositor;

	public AvaloniaService()
	{
		Studio.Rendering.OverlayRenderer.AddAfterEffectsPass(this.renderingPass);

		ThreadStart ts = new(this.StartImpl);
		this.uiThread = new Thread(ts);
		this.uiThread.Start();
	}

	public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

	public DispatcherImpl Dispatcher => this.dispatcher ?? throw new Exception("Avalonia not initalized");
	public UiPass RenderPass => this.renderingPass;

	public void Dispose()
	{
		Studio.Rendering.OverlayRenderer.RemoveAfterEffectsPass(this.renderingPass);
		this.renderingPass.Dispose();

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

			AppBuilder app = AppBuilder.Configure<StudioApplication>();
			app.WithInterFont();
			app.LogToTrace();

			if (this.UseWin32)
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
				app.UseWindowingSubsystem(this.InitializeWindowing, "StudioFourteen");
			}

			app.UseSkia();

			Studio.Log.Information($"Starting Avalonia");

			string[] args = [];
			app.Start(
				(main, args) =>
				{
					StudioApplication? application = Application.Current as StudioApplication;
					if (application == null)
						throw new Exception("Failed to create Application");

					if (this.UseWin32 && this.UseWin32Hybrid)
					{
						this.InitializeWindowing();
					}

					// Ready to run!
					this.LoadTypes();

					application.LoadTheme();

					try
					{
						this.testWindow.Show();
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
		this.screen = new StudioScreens(Studio.Rendering.OverlayRenderer);

		this.renderTimer = new(TimeSpan.FromSeconds(1.0 / this.RenderFramerate));
		this.dispatcher = new(TimeSpan.FromSeconds(1.0 / this.DispatcherFramerate));
		this.windowing = new(this.screen);

		AvaloniaLocator.CurrentMutable.Bind<IScreenImpl>().ToConstant(this.screen);
		AvaloniaLocator.CurrentMutable.Bind<IDispatcherImpl>().ToConstant(this.dispatcher);
		AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(this.renderTimer);
		AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToConstant(this.windowing);
		AvaloniaLocator.CurrentMutable.Bind<IPlatformLifetimeEventsImpl>().ToConstant(this);
		AvaloniaLocator.CurrentMutable.Bind<ICursorFactory>().ToConstant(new CursorFactory());

		IPlatformGraphics? platformGraphics = GlManager.Initialize();
		this.compositor = new Compositor(platformGraphics);
		AvaloniaLocator.CurrentMutable.Bind<Compositor>().ToConstant(this.compositor);
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

		IList assembliesList = sreTypeSystem.Field<IList>("_assemblies");
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

		method.Invoke(sreTypeSystem, [typeof(global::Avalonia.Svg.Svg).Assembly]);

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