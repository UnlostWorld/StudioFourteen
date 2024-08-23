namespace ScreenshotStudio.Controls;

using DependencyPropertyGenerator;
using System;
using FontAwesome.Sharp;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ScreenshotStudio.Files;

[DependencyProperty<FileSystemInfo>("File")]
public partial class PathDisplay : ItemsControl
{
	private readonly List<PathSegment> pathSegments = new();

	public PathDisplay()
	{
		this.InitializeComponent();
		this.OnFileChanged(this.File);
	}

	partial void OnFileChanged(FileSystemInfo? newValue)
	{
		this.pathSegments.Clear();
		this.ItemsSource = null;

		if (newValue == null)
			return;

		if (newValue is DirectoryInfo dirInfo && dirInfo.Parent == null)
		{
			this.pathSegments.Add(new(newValue.Name.Trim('\\'), IconChar.HardDrive));
		}
		else
		{
			this.pathSegments.Add(new(newValue.Name));

			DirectoryInfo? dir = null;
			if (newValue is DirectoryInfo directoryInfo)
			{
				dir = directoryInfo.Parent;
			}
			else if (newValue is FileInfo fileInfo)
			{
				dir = fileInfo.Directory;
			}

			if (dir != null)
			{
				DirectoryInfo root = dir.Root;

				while (dir != null && !dir.IsSpecialFolder() && !dir.IsDirectory(root))
				{
					this.pathSegments.Insert(0, new(dir.Name));
					dir = dir.Parent;
				}

				if (dir != null)
				{
					if (dir.IsSpecialFolder())
					{
						this.pathSegments.Insert(0, new(dir.Name, dir.GetSpecialFolder()?.ToIcon()));
					}
					else
					{
						this.pathSegments.Insert(0, new(dir.Name.Trim('\\'), IconChar.HardDrive));
					}
				}
			}
		}

		this.ItemsSource = this.pathSegments;
	}
}

public class PathSegment(string? name, IconChar? icon = null)
{
	public string? Name { get; set; } = name;
	public IconChar? Icon { get; set; } = icon;
}