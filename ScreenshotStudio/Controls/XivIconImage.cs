namespace ScreenshotStudio.Controls;

using ScreenshotStudio.GameData;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public class XivIconImage : Image
{
	public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
		nameof(XivIconImage.Id),
		typeof(uint),
		typeof(XivIconImage),
		new(0U, OnIdChanged));

	private readonly ImageReference imageReference = new(0);

	public uint Id
	{
		get => (uint)this.GetValue(IdProperty);
		set => this.SetValue(IdProperty, value);
	}

	private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is XivIconImage image)
		{
			image.UpdateSource();
		}
	}

	private void UpdateSource()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.imageReference.ImageId = this.Id;
		this.Source = this.imageReference.Source;
	}
}
