namespace ScreenshotStudio.Library.Controls;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System;
using System.Windows;

public partial class ActorAppearanceActions : View
{
	public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
		nameof(ActorAppearanceActions.Appearance),
		typeof(IActorAppearance),
		typeof(ActorAppearanceActions),
		new(null, OnAppearanceChanged));

	public ActorAppearanceActions()
	{
		this.ContentArea.DataContext = this;
	}

	public unsafe Actor* Actor { get; private set; }
	[AutoNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";
	[AutoNotify] public bool CanApply => this.Appearance != null && this.HasValidTarget;
	[AutoNotify] public unsafe bool CanRevert => this.Services.ActorAppearanceBackup.CanRestore(this.Actor);
	[AutoNotify] public bool IsLive { get; set; }

	[AutoNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			if (this.Actor == null)
				return false;

			if (this.Actor->RenderMode != RenderMode.Draw)
				return false;

			return true;
		}
	}

	public IActorAppearance? Appearance
	{
		get => (IActorAppearance?)this.GetValue(AppearanceProperty);
		set => this.SetValue(AppearanceProperty, value);
	}

	protected unsafe void OnFrameworkUpdate(IFramework framework)
	{
		this.Actor = ActorWindow.GetTarget();
	}

	private static unsafe void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ActorAppearanceActions actionsView)
		{
			if (actionsView.IsLive && actionsView.Appearance != null)
			{
				actionsView.Appearance.Apply(actionsView.Actor);
			}
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		LibraryWindow? window = this.FindParent<LibraryWindow>();
		if (window != null)
		{
			window.ItemDoubleClicked += this.OnItemDoubleClicked;
		}

		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		LibraryWindow? window = this.FindParent<LibraryWindow>();
		if (window != null)
		{
			window.ItemDoubleClicked += this.OnItemDoubleClicked;
		}

		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;
		}
	}

	private unsafe void OnItemDoubleClicked(IEntryBase entry)
	{
		if (!this.CanApply)
			return;

		if (entry is IActorAppearance appearance)
		{
			appearance.Apply(this.Actor);
		}
	}

	private unsafe void OnApplyClicked(object sender, RoutedEventArgs e)
	{
		if (this.Appearance == null)
			return;

		this.Appearance.Apply(this.Actor);
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.ActorAppearanceBackup.Restore(this.Actor);
		this.IsLive = false;
	}
}