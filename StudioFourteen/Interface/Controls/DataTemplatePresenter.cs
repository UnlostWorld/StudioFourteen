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
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using PropertyGenerator.Avalonia;
using StudioFourteen.Services.Avalonia;

public partial class DataTemplatePresenter : TemplatedControl
{
	private readonly List<AvaloniaContentReference<DataTemplate>> templateReferences = new();

	[GeneratedStyledProperty]
	public partial string? TemplateDirectory { get; set; }

	[GeneratedStyledProperty]
	public partial object? Content { get; set; }

	partial void OnTemplateDirectoryPropertyChanged(string? newValue)
	{
		if (newValue != null)
		{
			List<string> templatePaths = Studio.Content.GetContents(newValue);
			this.templateReferences.Clear();
			foreach (string templatePath in templatePaths)
			{
				AvaloniaContentReference<DataTemplate> reference = new(templatePath);
				reference.Reloaded += this.ReloadTemplates;
				this.templateReferences.Add(reference);
			}

			this.ReloadTemplates();
		}
	}

	private void ReloadTemplates()
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			List<DataTemplate> templates = new();
			foreach (AvaloniaContentReference<DataTemplate> reference in this.templateReferences)
			{
				templates.Add(reference.Get());
			}

			this.DataTemplates.Clear();
			this.DataTemplates.AddRange(templates);
		});
	}
}