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

namespace StudioFourteen.Posing;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Library;
using StudioFourteen.Posing.Shared;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects;
using StudioFourteen.Scene.GameObjects.Characters;
using WpfUtils;
using WpfUtils.Utils;

using Panel = StudioFourteen.Panels.Panel;

public partial class PosePanel : Panel
{
	private readonly FuncQueue showTooltipQueue;
	private SceneObjectBase? nextHover;

	[Notify] private GameObject? gameObject;
	[Notify] private string revertTooltip = string.Empty;
	[Notify] private SceneObjectBase? selection;
	[Notify] private SceneObjectBase? hover;
	[Notify] private bool isHoverTooltipOpen = false;
	[Notify] private UIElement? hoverTarget;

	public PosePanel()
	{
		this.showTooltipQueue = new(this.ShowTooltip, 500);
	}

	public Skeleton? Skeleton => this.GameObject as Skeleton;

	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedBodyTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedHandsTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedFaceTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public bool FlipSides
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	public void RegisterTabGroup(PoseTabItem tab)
	{
	}

	public void RegisterPoseView(PoseViewBase view)
	{
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Selection.GetScope<GameObject>().Attach(this.OnGameObjectSelectionChanged);
		this.Services.Selection.GetScope<TransformSceneObjectBase>().Attach(this.OnSceneSelectionChanged);
		this.Services.Selection.HoverChanged += this.OnSelectionHoverChanged;

		this.Selection = this.Services.Selection.Current;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.IsHoverTooltipOpen = false;
		this.Services.Selection.GetScope<GameObject>().Detach(this.OnGameObjectSelectionChanged);
		this.Services.Selection.GetScope<TransformSceneObjectBase>().Detach(this.OnSceneSelectionChanged);
		this.Services.Selection.HoverChanged -= this.OnSelectionHoverChanged;
	}

	protected virtual void OnGameObjectSelectionChanged(GameObject newSelection, object? source)
	{
		this.GameObject = newSelection;
	}

	private void OnSelectionHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.IsHoverTooltipOpen = false;
			this.showTooltipQueue.Cancel();

			if (newSelection != null && this.Services.Selection.HoverSource is SkeletonBoneControl target)
			{
				if (target.Dispatcher != this.Dispatcher)
					return;

				if (target.FindParent<PosePanel>() == this)
				{
					this.nextHover = newSelection;
					this.HoverTarget = target;
					this.showTooltipQueue.Invoke();
				}
			}
		});
	}

	private async Task ShowTooltip()
	{
		await this.MainThread();
		this.Hover = this.nextHover;
		this.IsHoverTooltipOpen = true;
	}

	private void OnSceneSelectionChanged(TransformSceneObjectBase? newSelection, object? source)
	{
		this.Selection = newSelection;
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.Reset();
	}

	private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.GameObject == null)
			return;

		this.Services.Selection.Select(this.GameObject, this);
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		if (this.GameObject == null)
			return;

		this.Services.Selection.Select(this.GameObject, this);
	}

	private void OnReferenceClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.SetToReferencePose();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryPanel.Open(this.GetContext());
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.Export();
	}

	private void OnFlipPoseClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.Flip();
	}
}

[DependencyProperty<string>("Category")]
public partial class PoseTabItem : TabItem
{
	private List<PoseViewBase>? views;

	public PoseTabItem()
	{
		this.Loaded += this.OnLoaded;
	}

	public bool IsValid { get; private set; }

	public void OnViewIsValidChanged(PoseViewBase view, bool newValue)
	{
		this.CheckViews();
	}

	public void OnTabIsValidChanged(PoseTabItem view, bool newValue)
	{
		this.CheckViews();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.views = this.FindLogicalChildren<PoseViewBase>();
	}

	private void CheckViews()
	{
		int visibleCount = 0;

		if (this.views != null)
		{
			foreach (PoseViewBase view in this.views)
			{
				if (view.IsValid)
				{
					visibleCount++;
				}
			}
		}

		if (visibleCount <= 0)
		{
			this.Visibility = Visibility.Hidden;
			this.IsValid = false;
		}
		else
		{
			this.Visibility = Visibility.Visible;
			this.IsValid = true;
		}
	}
}