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

namespace StudioFourteen.Widgets;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Scene.Gizmos;
using StudioFourteen.Selection;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Silk;

public partial class SelectionWidget : Panel
{
	private bool isShowing = false;
	[Notify] private SelectionBase? current;
	[Notify] private SelectionGizmoBase? selectedGizmo = null;
	[Notify] private bool hide;
	[Notify] private bool expanded;

	private Vector2 selectionCursorOffset;
	private Animator opening;
	private Animator closing;

	public FastObservableCollection<SelectionGizmoBase> Gizmos { get; init; } = new();

	public override void OnDeactivated()
	{
		base.OnDeactivated();
	}

	protected override void OnOpened()
	{
		this.opening = this.GetAnimator("Opening");
		this.closing = this.GetAnimator("Closing");

		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged += this.OnSelectionGizmoChanged;
		this.Services.Selection.SelectionExpanded += this.OnSelectionExpanded;
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged -= this.OnSelectionGizmoChanged;
		this.Services.Selection.SelectionExpanded -= this.OnSelectionExpanded;
		base.OnClosed();
	}

	protected override void OnGameTick()
	{
		base.OnGameTick();

		if (!this.opening.IsPlaying && !this.closing.IsPlaying)
		{
			this.Hide = this.Services.Input.Mouse?.GetButton(MouseButton.Left) == true;
		}
		else
		{
			this.Hide = false;
		}

		if (this.Current is TransformSelectionBase transformSelection)
		{
			Vector3 worldPos = Vector3.Transform(Vector3.Zero, transformSelection.WorldTransform.ToMatrix());
			Vector3 cameraPos = this.Services.Camera.WorldToCamera(worldPos);

			this.Dispatcher.Invoke(() =>
			{
				var pos = (cameraPos.ToVector2() + this.selectionCursorOffset).ToPoint();
				pos.X += 0.04;
				pos.Y -= 0.06;
				this.Position = pos;
			});
		}
	}

	private void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		Task.Run(() => this.ChangeSelection(newSelection));
	}

	private async Task ChangeSelection(SelectionBase? newSelection)
	{
		if (newSelection == null)
		{
			this.closing.Play();
			this.isShowing = false;
			return;
		}
		else
		{
			if (this.isShowing)
			{
				this.closing.Play();
				await Task.Delay(150);
			}

			this.isShowing = true;

			await this.MainThread();

			List<SelectionGizmoBase> gizmos = this.Services.Selection.GetValidGizmos();
			this.Gizmos.Replace(gizmos);
			this.Current = newSelection;
			this.Expanded = this.Services.Selection.ExpandedSelection;
			this.selectionCursorOffset = this.Services.Selection.SelectionCursorOffset;
			this.Focus();
			this.Activate();

			this.opening.Play();
		}
	}

	private void OnSelectionGizmoChanged(SelectionGizmoBase? oldGizmo, SelectionGizmoBase? newGizmo)
	{
		this.SelectedGizmo = newGizmo;
	}

	private void OnSelectedGizmoChanged(SelectionGizmoBase? oldGizmo, SelectionGizmoBase? newGizmo)
	{
		this.Services.Selection.Gizmo = newGizmo;
	}

	private void OnSelectionExpanded(bool newValue)
	{
		this.Expanded = newValue;
	}
}
