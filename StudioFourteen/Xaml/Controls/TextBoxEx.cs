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

namespace StudioFourteen.Xaml;

using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Controls;

[DependencyProperty<object>("Suffix")]
[DependencyProperty<object>("SuffixTemplate")]
[DependencyProperty<Thickness>("SuffixPosition")]
public partial class TextBoxEx : TextBox
{
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		this.UpdateSuffix();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		this.UpdateSuffix();
	}

	protected override void OnTextChanged(TextChangedEventArgs e)
	{
		base.OnTextChanged(e);
		this.UpdateSuffix();
	}

	partial void OnSuffixChanged()
	{
		this.UpdateSuffix();
	}

	private void UpdateSuffix()
	{
		Rect r = this.GetRectFromCharacterIndex(this.Text.Length, true);
		if (r != Rect.Empty)
		{
			Thickness m = this.SuffixPosition;
			m.Left = r.Left;
			m.Top = r.Top;
			this.SuffixPosition = m;
		}
	}
}
