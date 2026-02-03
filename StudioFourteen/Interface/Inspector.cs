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

namespace StudioFourteen.Interface;

using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioFourteen.Services.Avalonia;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Tick;

public partial class Inspector : WindowReference
{
	private SceneObjectBase? lastTarget;

	public Inspector()
		: base("UI/Inspector.ui")
	{
		Studio.Scene.ObjectSelected += this.OnObjectSelected;
		Studio.Scene.ObjectDeselected += this.OnObjectDeselected;
	}

	[ObservableProperty]
	public partial SceneObjectBase? Target { get; set; }

	[ObservableProperty]
	public partial int TabIndex { get; set; } = 0;

	[ObservableProperty]
	public partial string? InspectorPath { get; private set; }

	[ObservableProperty]
	public partial string? Page { get; set; } = "UI/Inspectors/EmptyPage.ui";

	[ObservableProperty]
	public partial object? PageDataContext { get; set; }

	[ObservableProperty]
	public partial bool IsPageOpen { get; set; }

	public override void Dispose()
	{
		base.Dispose();

		Studio.Scene.ObjectSelected -= this.OnObjectSelected;
		Studio.Scene.ObjectDeselected -= this.OnObjectDeselected;
	}

	public void OpenPage(string path, object? dataContext)
	{
		if (this.Target == null)
			return;

		this.Page = path;
		this.PageDataContext = dataContext;
		this.IsPageOpen = true;

		this.OnPropertyChanged(nameof(Inspector.Page));
	}

	[RelayCommand]
	public void ClosePage()
	{
		if (this.IsPageOpen)
		{
			this.IsPageOpen = false;
			this.Page = null;
		}
		else
		{
			Studio.Scene.ClearSelection();
		}
	}

	private void OnObjectSelected(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Target = obj;
			InspectAttribute? inspect = this.Target.GetType().GetCustomAttribute<InspectAttribute>();
			this.InspectorPath = inspect?.Path;
			this.Show();
		});
	}

	private void OnObjectDeselected(SceneObjectBase obj)
	{
		this.lastTarget = this.Target;
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Hide();
		});
	}

	partial void OnTabIndexChanged(int oldValue, int newValue)
	{
		this.Target?.InspectorTab = newValue;
	}
}