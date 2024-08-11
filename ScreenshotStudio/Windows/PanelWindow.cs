namespace ScreenshotStudio.Windows;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using ScreenshotStudio.Utilities;
using System.Windows;
using WpfUtils.Extensions;

[DependencyProperty<IconChar>("TitleIcon")]
[DependencyProperty<bool>("CanClose", DefaultValue = true)]
[DependencyProperty<bool>("CanChangeEmbed", DefaultValue = true)]
[DependencyProperty<string>("Subtitle")]
[DependencyProperty<double>("Scale", DefaultValue = 1.0)]
public partial class PanelWindow : PersistentPanel
{
	public virtual Point? SavedPosition
	{
		get => this.GetPersistence<Point?>();
		set => this.SetPersistence(value);
	}

	public Point Position
	{
		get => XivWindow.GetPosition(this);
		set => XivWindow.SetPosition(this, value);
	}

	public FastObservableCollection<double> ZoomOptions { get; init; } = new()
	{
		0.5,
		0.75,
		1.0,
		1.25,
		1.5,
		2.0,
		2.5,
	};

	protected override Style GetDefaultStyle() => (Style)this.FindResource("PanelWindowStyle");

	protected override void OnOpened()
	{
		if (this.SavedPosition != null)
			this.Position = (Point)this.SavedPosition;

		if (XivWindow.Process == null)
			this.CanChangeEmbed = false;

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.SavedPosition = this.Position;
	}
}