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
using StudioFourteen.Appearance;
using StudioFourteen.Panels;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects.Characters;
using WpfUtils.Extensions;

public partial class SelectionPanel : Panel
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

	public object? SelectedAdd
	{
		get => null;
		set => this.CreateObject(value);
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Types.Add(new SelectionType<Character>());
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

	private void CreateObject(object? obj)
	{
		if (obj == null)
			return;

		if (obj is ICharacterAppearance appearance)
		{
			this.Services.CharacterLifecycle.CreateAsync(appearance, UpdateSource.Interface).Run();
		}
	}
}