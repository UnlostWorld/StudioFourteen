namespace ScreenshotStudio.Controls;

using DependencyPropertyGenerator;
using System;
using FontAwesome.Sharp;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ScreenshotStudio.Files;

[DependencyProperty<FileInfo>("File")]
public partial class PathControl : Control
{
	private readonly List<PathSegment> pathSegments = new();

	public PathControl()
	{
		this.InitializeComponent();
	}

	protected TextBox? TextInputBox { get; private set; }
	protected ItemsControl? PathDisplay { get; private set; }

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.TextInputBox = this.GetTemplateChild("PART_TextInput") as TextBox;
		this.PathDisplay = this.GetTemplateChild("PART_PathDisplay") as ItemsControl;

		this.OnFileChanged(this.File);
	}

	private void OnTextInputGotFocus(object sender, RoutedEventArgs e)
	{
		if (this.TextInputBox == null)
			return;

		this.TextInputBox.Text = this.File?.FullName ?? string.Empty;
		this.TextInputBox.CaretIndex = int.MaxValue;
	}

	private void OnTextInputLostFocus(object sender, RoutedEventArgs e)
	{
		if (this.TextInputBox == null)
			return;

		this.File = new FileInfo(this.TextInputBox.Text);
		this.TextInputBox.Text = null;
	}

	partial void OnFileChanged(FileInfo? newValue)
	{
		if (this.PathDisplay == null)
			return;

		this.pathSegments.Clear();
		this.PathDisplay.ItemsSource = null;

		if (newValue == null)
			return;

		this.pathSegments.Add(new(newValue.Name));

		DirectoryInfo? dir = newValue.Directory;
		if (dir == null)
			return;

		DirectoryInfo root = dir.Root;

		while (dir != null && !dir.IsSpecialFolder() && dir != root)
		{
			this.pathSegments.Insert(0, new(dir.Name));
			dir = dir.Parent;
		}

		if (dir != null && dir.IsSpecialFolder())
		{
			this.pathSegments.Insert(0, new(dir.Name, dir.GetSpecialFolder()?.ToIcon()));
		}

		this.PathDisplay.ItemsSource = this.pathSegments;
	}

	private async void OnBrowseClicked(object sender, RoutedEventArgs e)
	{
		FileInfo? newFile = await ServiceManager.Instance.Files.ShowSaveDialog<SceneFile>();
		if (newFile == null)
			return;

		this.File = newFile;
	}
}

public class PathSegment(string? name, IconChar? icon = null)
{
	public string? Name { get; set; } = name;
	public IconChar? Icon { get; set; } = icon;
}