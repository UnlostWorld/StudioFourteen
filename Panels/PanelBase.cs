namespace ScreenshotStudio.Panels;

using Dalamud.Interface;
using FontAwesome.Sharp.Pro;
using ScreenshotStudio.Services;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Localization;
using static ScreenshotStudio.Services.PanelService;

public abstract class PanelBase : UserControl
{
	public static readonly IBind<string?> TitleDp = Binder.Register<string?, PanelBase>(nameof(Title), BindMode.OneWay);
	private PanelHostWindow? window;

	public PanelBase()
	{
		TextBlockHook.Attach();

		ResourceDictionary res = new();
		res.Source = new("Resources.xaml", UriKind.Relative);
		this.Resources = res;

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		if (this.Services.Settings.Current == null || !this.Services.Settings.Current.Panels.TryGetValue(this.Id, out PanelSettings? settings) || settings == null)
		{
			this.Settings = new();
			this.Services.Settings.Current?.Panels.Add(this.Id, this.Settings);
		}
		else
		{
			this.Settings = settings;
		}
	}

	public enum OpenModes
	{
		AlwaysCenter,
		TopLeftOrSaved,
		CenterOrSaved,
	}

	public ILogger Log => Serilog.Log.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;
	public PanelService.PanelSettings Settings { get; private set; }
	public bool IsOpen { get; private set; } = true;
	public virtual string Id => this.GetType().ToString();
	public ProIcons Icon { get; set; }
	public bool ShowBackground { get; set; } = true;
	public bool CanResize { get; set; }
	public bool CanScroll { get; set; } = false;

	public OpenModes OpenMode { get; set; } = OpenModes.CenterOrSaved;
	public double? DefaultWidth { get; set; } = null;
	public double? DefaultHeight { get; set; } = null;

	public bool IsActive
	{
		get => this.Window.IsActive;
		set => this.Window.Activate();
	}

	public string? Title
	{
		get => TitleDp.Get(this);
		set => TitleDp.Set(this, value);
	}

	public PanelHostWindow Window
	{
		get
		{
			if (this.window == null)
				throw new Exception("Attempt to access panel host window before it has been initialized");

			return this.window;
		}
	}

	public void DragMove() => this.Window.DragMove();

	public void Open(PanelHostWindow window)
	{
		this.window = window;
		this.IsOpen = true;
		this.OnOpening();
	}

	public void Close()
	{
		this.OnClosing();
		this.IsOpen = false;
		this.Services.Panels.OnPanelClosed(this);
		this.Settings.Save();
		this.window?.Close();
	}

	public async Task WhileOpen()
	{
		await Dispatch.NonUiThread();

		while (this.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	public virtual void OnOpening() { }
	public virtual void OnClosing() { }
	public virtual void OnActivated() { }
	public virtual void OnDeactivated() { }
}