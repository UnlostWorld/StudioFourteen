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
using StudioFourteen.Scene;
using StudioFourteen.Extensions;
using StudioFourteen.Xaml;
using System.Collections.Generic;

public partial class SelectionType<T> : SelectionTypeBase
	where T : SceneObjectBase
{
	private readonly SelectionListener<T> scope;

	public SelectionType()
	{
		this.scope = new(this.OnScopeSelectionChanged);
		this.Objects = new(ServiceManager.Instance.Scene.FindObjects<T>());

		this.scope.Enable();
		this.Selection = this.scope.Current;
	}

	[Bind] public partial T? Selection { get; set; }
	public List<T> Objects { get; init; } = new();

	public override Type Type => typeof(T);
	public override string? Name => XamlResources.Find($"LOC_Type_{typeof(T).Name}s", typeof(T).Name);
	public override object? Icon => XamlResources.Find($"ICON_Type_{typeof(T).Name}");

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

	public override void OnSelectionTypeActivated()
	{
		ServiceManager.Instance.Selection.Select(this.Selection, this);
	}

	partial void OnSelectionPropertyChanged(T oldValue, T newValue)
	{
		ServiceManager.Instance.Selection.Select(newValue, this);
	}

	private void OnScopeSelectionChanged(T? oldSelection, T? selection, object? source)
	{
		this.Selection = selection;
	}
}