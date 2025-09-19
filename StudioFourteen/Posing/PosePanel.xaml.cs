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
using StudioFourteen.Library;
using StudioFourteen.Posing.Shared;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;
using StudioFourteen.Selection;
using StudioFourteen;
using StudioFourteen.Extensions;
using StudioFourteen.Utils;

using Panel = StudioFourteen.Panels.Panel;

public partial class PosePanel : Panel
{
	private readonly SelectionListener<Skeleton> skeletonSelectionListener;
	private readonly SelectionListener<TransformSceneObjectBase> sceneObjectSelectionListener;

	private readonly FuncQueue showTooltipQueue;
	private SceneObjectBase? nextHover;

	public PosePanel()
	{
		this.skeletonSelectionListener = new(this.OnSkeletonSelectionChanged);
		this.sceneObjectSelectionListener = new(this.OnSceneSelectionChanged);
		this.showTooltipQueue = new(this.ShowTooltip, 500);
		this.RevertTooltip = string.Empty;
	}

	[Bind] public partial Skeleton? Skeleton { get; set; }
	[Bind] public partial string RevertTooltip { get; set; }
	[Bind] public partial SceneObjectBase? Selection { get; set; }
	[Bind] public partial SceneObjectBase? Hover { get; set; }
	[Bind] public partial bool IsHoverTooltipOpen { get; set; }
	[Bind] public partial UIElement? HoverTarget { get; set; }

	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public bool FlipSides
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	public bool CollapseInspector
	{
		get => this.GetPersistence<bool>();
		set
		{
			this.SetPersistence(value);
			this.Host?.Resize(value ? -200 : 200, 0);
		}
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

		this.skeletonSelectionListener.Enable();
		this.sceneObjectSelectionListener.Enable();
		this.Services.Selection.HoverChanged += this.OnSelectionHoverChanged;

		this.Skeleton = this.skeletonSelectionListener.Current;
		this.Selection = this.sceneObjectSelectionListener.Current;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		if (!ServiceManager.ShutdownRequested)
			this.Services.Selection.HoverChanged -= this.OnSelectionHoverChanged;

		this.IsHoverTooltipOpen = false;
		this.skeletonSelectionListener.Disable();
		this.sceneObjectSelectionListener.Disable();
	}

	protected virtual void OnSkeletonSelectionChanged(
		Skeleton? oldSelection,
		Skeleton? newSelection,
		object? source)
	{
		this.Skeleton = newSelection;
	}

	private void OnSceneSelectionChanged(
		TransformSceneObjectBase? oldSelection,
		TransformSceneObjectBase? newSelection,
		object? source)
	{
		this.Selection = newSelection;
	}

	private void OnSelectionHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Dispatcher.BeginInvoke(() =>
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

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Skeleton.ResetPose();
	}

	private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Services.Selection.Select(this.Skeleton, this);
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		this.Services.Selection.Select(this.Skeleton, this);
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

		this.Skeleton.SavePose().RunAsynchronously();
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