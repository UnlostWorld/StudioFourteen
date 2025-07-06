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

namespace StudioFourteen.Selection;

using System;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects.Characters;
using WpfUtils.Extensions;

using static StudioFourteen.Selection.SelectionService;

public partial class SelectionPanel : Panel
{
	private SelectionType? currentType;

	public FastObservableCollection<SelectionType> Types { get; init; } = new();

	public SelectionType? CurrentType
	{
		get => this.currentType;
		set
		{
			this.currentType = value;
			this.NotifyPropertyChanged();
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.OnSelectionChanged(null, this.Services.Selection.Current, null);

		this.Types.Add(new SelectionType<Character>(StudioFourteen.Resources.Find("ICON_Type_Character")));
		this.CurrentType = this.Types[0];
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Dispatcher.Invoke(() =>
		{
		});
	}
}

public class SelectionType
{
	public SelectionType(object? icon)
	{
		this.Icon = icon;
	}

	public object? Icon { get; init; }
}

public partial class SelectionType<T> : SelectionType
	where T : SceneObjectBase
{
	private readonly SelectionScope scope;
	[Notify] private T? selection;

	public SelectionType(object? icon)
		: base(icon)
	{
		this.scope = ServiceManager.Instance.Selection.GetScope<T>();
		ServiceManager.Instance.Selection.SelectionChanged += this.OnServiceSelectionChanged;
		ServiceManager.Instance.Scene.ObjectAdded += this.OnObjectAddedToScene;
		ServiceManager.Instance.Scene.ObjectRemoved += this.OnObjectRemovedFromScene;

		this.Objects.Replace(ServiceManager.Instance.Scene.FindObjects<T>());
	}

	public FastObservableCollection<T> Objects { get; init; } = new();

	private void OnServiceSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? selectionSource)
	{
		if (newSelection is T tObj)
		{
			this.Selection = tObj;
		}
		else
		{
			this.Selection = null;
		}
	}

	private void OnSelectionChanged(T? oldValue, T? newValue)
	{
		ServiceManager.Instance.Selection.Select(newValue, this);
	}

	private void OnObjectRemovedFromScene(SceneObjectBase obj)
	{
		if (obj is T tObj)
		{
			this.Objects.Add(tObj);
		}
	}

	private void OnObjectAddedToScene(SceneObjectBase obj)
	{
		if (obj is T tObj)
		{
			this.Objects.Remove(tObj);
		}
	}
}