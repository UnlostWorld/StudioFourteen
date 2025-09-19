namespace StudioFourteen.Silk;

using System;
using System.Windows;

public static class AnimationUtils
{
	public static Duration GetDuration(int ms) => new Duration(GetTimeSpan(ms));
	public static TimeSpan GetTimeSpan(int ms) => TimeSpan.FromMilliseconds(ms);
}
