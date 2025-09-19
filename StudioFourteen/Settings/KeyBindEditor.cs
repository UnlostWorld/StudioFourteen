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

namespace StudioFourteen.Settings;

using DependencyPropertyGenerator;
using StudioFourteen.Input;
using System.Windows.Controls;

[DependencyProperty<Bind>("Bind")]
public partial class KeyBindEditor : TextBox
{
	public KeyBindEditor()
	{
		this.IsReadOnly = true;
	}

	public delegate void KeyBindChangedDelegate(KeyBindEditor sender, Bind? bind);

	public event KeyBindChangedDelegate? KeyBindChanged;

	/*protected override void OnPreviewKeyDown(KeyEventArgs e)
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
		this.KeyBind.Modifiers = e.KeyboardDevice.Modifiers;

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

	partial void OnKeyBindChanged(KeyboardDevice.Bind? newValue)
	{
		this.Text = newValue?.ToString();
		this.KeyBindChanged?.Invoke(this, newValue);
	}*/
}
