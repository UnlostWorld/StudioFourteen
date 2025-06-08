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

using StudioFourteen.Library;
using StudioFourteen.Panels;
using StudioFourteen.Posing.Shared;
using StudioFourteen.Selection;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using WpfUtils.Utils;
using System.Threading.Tasks;
using System;
using WpfUtils;

public partial class PosePanel : CharacterPanelBase
{
	private readonly FuncQueue showTooltipQueue;
	private SelectionBase? nextHover;

	[Notify] private string revertTooltip = string.Empty;
	[Notify] private SelectionBase? selection;
	[Notify] private bool isSelectionTransform;
	[Notify] private bool isSelectionBlend;
	[Notify] private bool isSelectionEye;
	[Notify] private SelectionBase? hover;
	[Notify] private bool isHoverTooltipOpen = false;
	[Notify] private UIElement? hoverTarget;

	public PosePanel()
	{
		this.showTooltipQueue = new(this.ShowTooltip, 500);
	}

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

		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.HoverChanged += this.OnHoverChanged;

		if (this.Services.Selection.Current == null && this.TargetObjectIndex >= 0)
		{
			this.Services.Selection.Current = new ObjectTableSelection((ushort)this.TargetObjectIndex);
		}

		this.Selection = this.Services.Selection.Current;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.IsHoverTooltipOpen = false;
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.HoverChanged -= this.OnHoverChanged;
	}

	protected override void OnTargetChanged(int objectTableIndex)
	{
		base.OnTargetChanged(objectTableIndex);
		this.RevertTooltip = StudioFourteen.Resources.Format("LOC_Pose_RevertPose", this.CharacterName);
	}

	private void OnHoverChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.IsHoverTooltipOpen = false;
			this.showTooltipQueue.Cancel();

			if (newSelection != null && this.Services.Selection.HoverSource is PoseSelectionControl target)
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

	private void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		this.Selection = newSelection;
		this.IsSelectionTransform = this.Selection is TransformSelectionBase;
		this.IsSelectionBlend = this.Selection is BlendSelection;
		this.IsSelectionEye = this.Selection is EyeSelection;
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Pose.FlushBoneReferences((ushort)this.TargetObjectIndex);
		this.Services.Selection.Current = new ObjectTableSelection((ushort)this.TargetObjectIndex);
	}

	private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Selection.Current = new ObjectTableSelection((ushort)this.TargetObjectIndex);
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Selection.Current = new ObjectTableSelection((ushort)this.TargetObjectIndex);
	}

	private async void OnReferenceClicked(object sender, RoutedEventArgs e)
	{
		await this.Services.Pose.SetToReferencePose(this.TargetObjectIndex);
	}

	private void OnResetSelectionClicked(object sender, RoutedEventArgs e)
	{
		if (this.Selection == null)
			return;

		this.Selection.Reset();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryPanel.Open(this.GetContext());
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		await this.Services.Pose.ExportPose(this.TargetObjectIndex);
	}

	private void OnFlipPoseClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Flip(this.TargetObjectIndex);
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