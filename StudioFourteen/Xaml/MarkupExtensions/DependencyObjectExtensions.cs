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

namespace System.Windows;

using System.Collections.Generic;
using System.Windows.Media;

public static class DependencyObjectExtensions
{
	public static T? FindParent<T>(this DependencyObject child)
		where T : DependencyObject
	{
		DependencyObject parentObject;

		if (child == null)
			return null;

		if (child is Visual)
		{
			parentObject = VisualTreeHelper.GetParent(child);
		}
		else
		{
			parentObject = LogicalTreeHelper.GetParent(child);
		}

		if (parentObject == null)
			return null;

		T? parent = parentObject as T;
		if (parent != null)
		{
			return parent;
		}
		else
		{
			return parentObject.FindParent<T>();
		}
	}

	public static T? FindLogicalParent<T>(this DependencyObject child)
		where T : DependencyObject
	{
		DependencyObject parentObject;
		parentObject = LogicalTreeHelper.GetParent(child);

		if (parentObject == null)
			return null;

		T? parent = parentObject as T;
		if (parent != null)
		{
			return parent;
		}
		else
		{
			return parentObject.FindLogicalParent<T>();
		}
	}

	public static T? FindChild<T>(this DependencyObject self)
		where T : notnull
	{
		List<T> results = new List<T>();
		self.FindChildren<T>(ref results);

		if (results.Count == 0)
			return default;

		return results[0];
	}

	public static List<T> FindChildren<T>(this DependencyObject self)
	{
		List<T> results = new List<T>();
		self.FindChildren<T>(ref results);
		return results;
	}

	public static void FindChildren<T>(this DependencyObject self, ref List<T> results)
	{
		int children = VisualTreeHelper.GetChildrenCount(self);
		for (int i = 0; i < children; i++)
		{
			DependencyObject? child = VisualTreeHelper.GetChild(self, i);

			if (child is T tChild)
				results.Add(tChild);

			child.FindChildren(ref results);
		}
	}

	public static List<T> FindLogicalChildren<T>(this DependencyObject self)
	{
		List<T> results = new List<T>();
		self.FindLogicalChildren<T>(ref results);
		return results;
	}

	public static void FindLogicalChildren<T>(this DependencyObject self, ref List<T> results)
	{
		foreach (var child in LogicalTreeHelper.GetChildren(self))
		{
			if (child is T tChild)
				results.Add(tChild);

			if (child is DependencyObject dependencyObjectChild)
			{
				dependencyObjectChild.FindLogicalChildren(ref results);
			}
		}
	}
}
