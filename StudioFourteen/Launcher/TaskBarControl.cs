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

namespace StudioFourteen.Launcher;

using FontAwesome.Sharp;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WpfUtils.Extensions;
using DependencyPropertyGenerator;
using System.Windows.Input;

public partial class TaskBarControl : Control
{
	public TaskBarControl()
	{
		this.Entries.Add(new TaskBarEntry("fa-Book", "Library"));
		this.Entries.Add(new TaskBarEntry("fa-Gears", "Setings"));
		this.Entries.Add(new TaskBarEntry("Camera", "Setings"));
	}

	public FastObservableCollection<TaskBarEntry> Entries { get; init; } = new();
}

public class TaskBarEntry
{
	public TaskBarEntry(string icon, string tooltip)
	{
		this.Icon = icon;
		this.ToolTip = tooltip;
	}

	public string Icon { get; set; }
	public string? ToolTip { get; set; }
}

[DependencyProperty<bool>("IsMinimized")]
[DependencyProperty<object>("Icon")]
public partial class TaskBarButtonControl : Control
{
	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Left)
		{
			this.IsMinimized = !this.IsMinimized;
		}
	}
}