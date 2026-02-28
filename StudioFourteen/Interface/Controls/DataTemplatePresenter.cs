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

namespace StudioFourteen.Interface.Controls;

using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using StudioFourteen.Services.Avalonia;

public class DataTemplatePresenter : ContentControl
{
	public static readonly StyledProperty<string?> TemplateDirectoryProperty;
	public static readonly StyledProperty<object?> TargetProperty;

	private readonly List<AvaloniaContentReference<DataTemplate>> templateReferences = new();

	static DataTemplatePresenter()
	{
		TemplateDirectoryProperty = AvaloniaProperty.Register<DataTemplatePresenter, string?>(
			nameof(DataTemplatePresenter.TemplateDirectory));

		TargetProperty = AvaloniaProperty.Register<DataTemplatePresenter, object?>(
			nameof(DataTemplatePresenter.Target));
	}

	public string? TemplateDirectory
	{
		get => this.GetValue(TemplateDirectoryProperty);
		set => this.SetValue(TemplateDirectoryProperty, value);
	}

	public object? Target
	{
		get => this.GetValue(TargetProperty);
		set => this.SetValue(TargetProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == TemplateDirectoryProperty && this.TemplateDirectory != null)
		{
			List<string> templatePaths = Studio.Content.GetContents(this.TemplateDirectory);
			this.templateReferences.Clear();
			foreach (string templatePath in templatePaths)
			{
				AvaloniaContentReference<DataTemplate> reference = new(templatePath);
				reference.Reloaded += this.ReloadTemplates;
				this.templateReferences.Add(reference);
			}

			this.ReloadTemplates();
		}

		if (change.Property == TargetProperty)
		{
			// Cant set target to null!
			if (change.OldValue != null && change.NewValue == null)
			{
				this.Target = change.OldValue;
			}
		}

		if (change.Property == TargetProperty)
		{
			this.Content = this.Target;
		}
	}

	private void ReloadTemplates()
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			this.DataTemplates.Clear();
			foreach (AvaloniaContentReference<DataTemplate> reference in this.templateReferences)
			{
				this.DataTemplates.Add(reference.Get());
			}
		});
	}
}