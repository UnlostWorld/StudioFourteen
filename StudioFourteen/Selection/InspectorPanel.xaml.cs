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
using System.Collections.Generic;
using StudioFourteen.Panels;
using StudioFourteen.Scene;
using StudioFourteen.Scene.Cameras;
using StudioFourteen.Scene.GameObjects.Characters;
using WpfUtils.Extensions;

public partial class InspectorPanel : Panel
{
	private SelectionTypeBase? currentType;

	public FastObservableCollection<SelectionTypeBase> Types { get; init; } = new();

	public SelectionTypeBase? CurrentType
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

		this.Types.Replace(this.GetSelectionTypes());
		this.CurrentType = this.Types[0];

		this.Services.Scene.ObjectAdded += this.OnObjectAddedToScene;
		this.Services.Scene.ObjectRemoved += this.OnObjectRemovedFromScene;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.Scene.ObjectAdded -= this.OnObjectAddedToScene;
		this.Services.Scene.ObjectRemoved -= this.OnObjectRemovedFromScene;
	}

	protected virtual List<SelectionTypeBase> GetSelectionTypes()
	{
		List<SelectionTypeBase> selectionTypes = new();
		selectionTypes.Add(new SelectionType<SceneObjectBase>());
		selectionTypes.Add(new SelectionType<Character>());
		selectionTypes.Add(new SelectionType<Camera>());
		return selectionTypes;
	}

	private void OnObjectRemovedFromScene(SceneObjectBase obj)
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			foreach (SelectionTypeBase type in this.Types)
			{
				type.OnObjectRemovedFromScene(obj);
			}
		});
	}

	private void OnObjectAddedToScene(SceneObjectBase obj)
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			foreach (SelectionTypeBase type in this.Types)
			{
				type.OnObjectAddedToScene(obj);
			}
		});
	}
}

public class InspectorPanel<T> : InspectorPanel
	where T : SceneObjectBase
{
	protected override List<SelectionTypeBase> GetSelectionTypes()
	{
		List<SelectionTypeBase> selectionTypes = new();
		selectionTypes.Add(new SelectionType<T>());
		return selectionTypes;
	}
}