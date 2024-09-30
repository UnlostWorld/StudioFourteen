namespace ScreenshotStudio.Settings;

using Dalamud.Game.ClientState.Keys;
using DependencyPropertyGenerator;
using ScreenshotStudio.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

[DependencyProperty<KeyBind>("KeyBind")]
public partial class KeyBindEditor : TextBox
{
	public KeyBindEditor()
	{
		this.IsReadOnly = true;
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		base.OnPreviewKeyDown(e);
		if (e.Key == Key.Escape)
			return;

		e.Handled = true;

		if (this.KeyBind == null)
			this.KeyBind = new();

		if (e.Key >= Key.LeftShift || e.Key <= Key.Help)
			return;

		this.KeyBind.Key = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.Key);
		this.KeyBind.Shift = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift);
		this.KeyBind.Control = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);
		this.KeyBind.Alt = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt);
		this.OnKeyBindChanged(this.KeyBind);
	}

	protected override void OnPreviewKeyUp(KeyEventArgs e)
	{
		base.OnPreviewKeyUp(e);
		if (e.Key == Key.Escape)
			return;

		e.Handled = true;
	}

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		base.OnPreviewMouseDown(e);

		if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
		{
			this.KeyBind = null;
			this.OnKeyBindChanged(this.KeyBind);
		}
	}

	partial void OnKeyBindChanged(KeyBind? newValue)
	{
		this.Text = newValue?.ToString();
	}
}
