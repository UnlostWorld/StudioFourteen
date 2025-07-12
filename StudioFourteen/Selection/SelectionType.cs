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
using StudioFourteen.Scene;
using WpfUtils.Extensions;

using static StudioFourteen.Selection.SelectionService;

public partial class SelectionType<T> : SelectionTypeBase
	where T : SceneObjectBase
{
	private readonly SelectionScope<T> scope;
	[Notify] private T? selection;

	public SelectionType()
	{
		this.scope = ServiceManager.Instance.Selection.GetScope<T>();
		this.Objects.Replace(ServiceManager.Instance.Scene.FindObjects<T>());

		this.scope.Attach(this.OnScopeSelectionChanged);
		this.Selection = this.scope.Selection;
	}

	public FastObservableCollection<T> Objects { get; init; } = new();

	public override Type Type => typeof(T);
	public override string? Name => StudioFourteen.Resources.Find($"LOC_Type_{typeof(T).Name}s", typeof(T).Name);
	public override object? Icon => StudioFourteen.Resources.Find($"ICON_Type_{typeof(T).Name}");

	public override void OnObjectAddedToScene(SceneObjectBase obj)
	{
		if (obj is T tObj)
		{
			this.Objects.Add(tObj);
		}
	}

	public override void OnObjectRemovedFromScene(SceneObjectBase obj)
	{
		if (obj is T tObj)
		{
			this.Objects.Remove(tObj);
		}
	}

	private void OnSelectionChanged(T? oldValue, T? newValue)
	{
		ServiceManager.Instance.Selection.Select(newValue, this);
	}

	private void OnScopeSelectionChanged(T selection, object? source)
	{
		this.Selection = selection;
	}
}