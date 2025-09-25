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

using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Scene;
using StudioFourteen.Settings;
using StudioFourteen;
using StudioFourteen.Extensions;
using StudioFourteen.Xaml.Silk;

public partial class Widget : Panel
{
	private bool isShowing = false;

	private Vector2 selectionCursorOffset;
	private Animator opening;
	private Animator closing;

	[Bind] public partial SceneObjectBase? Current { get; set; }
	[Bind] public partial bool Hide { get; set; }
	[Bind] public partial bool Expanded { get; set; }
	[Bind] public partial bool CanExpand { get; set; }
	public FastObservableCollection<GizmoGroup> Gizmos { get; init; } = new();

	public override void OnDeactivated()
	{
		base.OnDeactivated();
	}

	protected override void OnOpened()
	{
		this.CanExpand = this.Settings.WidgetMode == Configuration.WidgetModes.Inspector;

		this.opening = this.GetAnimator("Opening");
		this.closing = this.GetAnimator("Closing");

		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.SelectionExpanded += this.OnSelectionExpanded;
		this.Services.Settings.SettingChanged += this.OnSettingChanged;
		base.OnOpened();

		this.OnSelectionChanged(null, this.Services.Selection.Current, this);
	}

	protected override void OnClosed()
	{
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.SelectionExpanded -= this.OnSelectionExpanded;
		this.Services.Settings.SettingChanged -= this.OnSettingChanged;
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

		if (this.Current is TransformSceneObjectBase transformSelection)
		{
			double thisHeight = 380;

			Vector3 worldPos = Vector3.Transform(Vector3.Zero, transformSelection.WorldTransform.ToMatrix());
			Vector3 cameraPos = this.Services.Camera.WorldToCamera(worldPos);

			this.Dispatcher.BeginInvoke(() =>
			{
				Point pos = (cameraPos.ToVector2() + this.selectionCursorOffset).ToPoint();

				Rect clientRect = this.Services.Windows.GetXivWindowClientSize();
				double xPos = pos.X * clientRect.Width;
				double yPos = pos.Y * clientRect.Height;

				xPos += 75;
				yPos -= 50;

				if (yPos + thisHeight > clientRect.Height)
					yPos = clientRect.Height - thisHeight;

				pos.X = xPos / clientRect.Width;
				pos.Y = yPos / clientRect.Height;

				this.Position = pos;
			});
		}
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		Task.Run(() => this.ChangeSelection(newSelection, source));
	}

	private async Task ChangeSelection(SceneObjectBase? newSelection, object? source)
	{
		if (newSelection == null)
		{
			this.closing.Play();
			this.isShowing = false;
			this.Expanded = false;
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

			this.Gizmos.Clear();
			this.Gizmos.Add(this.Services.Gizmos.Transform);

			if (newSelection != null)
			{
				foreach (GizmoBase gizmo in newSelection.Gizmos)
				{
					if (gizmo is GizmoGroup group)
					{
						this.Gizmos.Add(group);
					}
				}
			}

			this.Current = newSelection;

			if (this.Settings.WidgetMode == Configuration.WidgetModes.Inspector)
				this.Expanded = this.Services.Selection.ExpandedSelection;

			if (source is SelectionHandle)
				this.selectionCursorOffset = this.Services.Selection.SelectionCursorOffset;

			this.Focus();
			this.Activate();

			this.opening.Play();
		}
	}

	private void OnSelectionExpanded(bool newValue)
	{
		this.Expanded = newValue;
	}

	private void OnSettingChanged(string settingName, object? newValue)
	{
		if (settingName == nameof(this.Settings.WidgetMode))
		{
			this.CanExpand = this.Settings.WidgetMode == Configuration.WidgetModes.Inspector;
		}
	}

	private void OnExpandedChanged(bool oldValue, bool newValue)
	{
		if (newValue && this.Settings.WidgetMode != Configuration.WidgetModes.Inspector)
		{
			this.Services.Selection.ExpandedSelection = false;
			this.Expanded = false;
			return;
		}

		this.Services.Selection.ExpandedSelection = newValue;
	}
}