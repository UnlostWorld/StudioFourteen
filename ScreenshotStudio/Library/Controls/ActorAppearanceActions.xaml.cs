namespace ScreenshotStudio.Library.Controls;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System.Windows;

public partial class ActorAppearanceActions : View
{
	public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
		nameof(ActorAppearanceActions.Appearance),
		typeof(IActorAppearance),
		typeof(ActorAppearanceActions),
		new(null));

	public ActorAppearanceActions()
	{
		this.ContentArea.DataContext = this;
	}

	public unsafe Actor* Actor { get; private set; }
	[AutoNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";
	[AutoNotify] public bool CanApply => this.Appearance != null && this.HasValidTarget;

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

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;
		}
	}

	private unsafe void OnApplyClicked(object sender, RoutedEventArgs e)
	{
		if (this.Appearance == null)
			return;

		this.Appearance.Apply(this.Actor);
	}
}