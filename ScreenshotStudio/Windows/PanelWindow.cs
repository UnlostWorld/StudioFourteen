namespace ScreenshotStudio.Windows;

using FontAwesome.Sharp.Pro;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Commands;
using XivToolsWpf.Extensions;

public abstract partial class PanelWindow : Window
{
	public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(
		"TitleIcon",
		typeof(ProIcons),
		typeof(PanelWindow));

	public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
		"Actions",
		typeof(FastObservableCollection<PanelAction>),
		typeof(PanelWindow),
		new(new FastObservableCollection<PanelAction>()));

	public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
		"CanClose",
		typeof(bool),
		typeof(PanelWindow),
		new(true));

	public PanelWindow()
	{
		this.Resources = ScreenshotStudio.Resources.Instance;
		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
	}

	public ProIcons TitleIcon
	{
		get => (ProIcons)GetValue(TitleIconProperty);
		set => SetValue(TitleIconProperty, value);
	}

	public FastObservableCollection<PanelAction> Actions
	{
		get => (FastObservableCollection<PanelAction>)GetValue(ActionsProperty);
		set => SetValue(ActionsProperty, value);
	}

	public bool CanClose
	{
		get => (bool)GetValue(CanCloseProperty);
		set => SetValue(CanCloseProperty, value);
	}

	public new void Show() => this.Dispatcher.BeginInvoke(() => base.Show());
	public new void Close() => this.Dispatcher.BeginInvoke(() => base.Close());

	public static T? Show<T>()
		where T : PanelWindow
	{
		T? wnd = CreateInstance<T>().Result;
		wnd?.Show();
		return wnd;
	}

	public static async Task<T?> ShowAsync<T>()
		where T : PanelWindow
	{
		T? wnd = await CreateInstance<T>();
		wnd?.Show();
		return wnd;
	}

	public static async Task<T?> CreateInstance<T>()
		where T : PanelWindow
	{
		PanelWindow? pwb = await PanelWindow.CreateInstance(typeof(T));
		return pwb as T;
	}

	public static async Task<PanelWindow?> CreateInstance(Type panelWindowType)
	{
		return await new PanelWindowThread().Start(panelWindowType);
	}

	private class PanelWindowThread
	{
		private static readonly object createInstanceLock = new();

		private PanelWindow? panel;
		private Type? panelType;

		protected ILogger Log => Serilog.Log.ForContext<PanelWindowThread>();

		public async Task<PanelWindow?> Start(Type panelType)
		{
			this.panelType = panelType;

			Thread panelMainThread = new Thread(this.PanelMainThread);
			panelMainThread.SetApartmentState(ApartmentState.STA);
			panelMainThread.Start(this);

			// Wait for the panel to load for up to 5 seconds.
			int timeOut = 5000;
			while (this.panel == null && timeOut > 0)
			{
				await Task.Delay(10);
				timeOut -= 10;
			}

			if (this.panel == null)
				Log.Error($"Failed to create panel window {this.panelType}");

			return this.panel;
		}

		private void PanelMainThread(object? param)
		{
			if (this.panelType == null)
				throw new Exception("No panel type in panel thread");

			try
			{
				// Even though we're doing this on another thread, we can still only do one panel
				// at a time since WPF's LoadComponent system isn't thread safe.
				lock (PanelWindowThread.createInstanceLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as PanelWindow;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Exception during panel construction: {this.panelType}");
				return;
			}

			this.Log.Information($"Panel: {this.panelType} has started");
			System.Windows.Threading.Dispatcher.Run();
			this.Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}

public class PanelAction
{
	public string? ToolTip { get; set; }
	public ProIcons Icon { get; set; } = ProIcons.None;
	public ICommand? Command { get; set; }

	public PanelAction()
	{
	}

	public PanelAction(ProIcons icon, string? tooltip, Action callback)
	{
		this.Icon = icon;
		this.ToolTip = tooltip;
		this.Command = new SimpleCommand(callback);
	}
}