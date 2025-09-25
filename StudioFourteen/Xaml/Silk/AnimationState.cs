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

namespace StudioFourteen.Xaml.Silk;

using System.Windows;
using System.Windows.Controls;
using DependencyPropertyGenerator;

[DependencyProperty<object>("Binding")]
[DependencyProperty<object>("Value")]
[DependencyProperty<AnimationCollection>("Enter")]
[DependencyProperty<AnimationCollection>("Exit")]
public partial class AnimationState : FrameworkElement
{
	public AnimationState()
	{
		this.Enter = new(this, this);
		this.Exit = new(this, this);
	}

	protected void SetState(bool state)
	{
		if (state)
		{
			this.Exit?.Stop();
			this.Enter?.Play(this.VisualParent);
		}
		else
		{
			this.Enter?.Stop();
			this.Exit?.Play(this.VisualParent);
		}
	}

	partial void OnBindingChanged(object? newValue)
	{
		if (this.Value == null)
		{
			if (newValue is bool bValue)
			{
				this.SetState(bValue);
			}
		}
		else
		{
			this.SetState(newValue == this.Value);
		}
	}
}

public class AnimationCollection : UIElementCollection
{
	public AnimationCollection(UIElement visualParent, FrameworkElement logicalParent)
		: base(visualParent, logicalParent)
	{
	}

	public void Play(DependencyObject target)
	{
		foreach (UIElement el in this)
		{
			if (el is Animation anim)
			{
				anim.Play(target);
			}
		}
	}

	public void Stop()
	{
		foreach (UIElement el in this)
		{
			if (el is Animation anim)
			{
				anim.Stop();
			}
		}
	}
}