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

namespace StudioFourteen.Posing.Shared;

using System.Collections.Generic;
using System.Threading.Tasks;
using StudioFourteen.Mvm;
using StudioFourteen.Utilities;
using System.Windows;
using WpfUtils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using WpfUtils.Extensions;
using System.Windows.Input;
using System.Windows.Media;
using StudioFourteen.Selection;
using DependencyPropertyGenerator;

[DependencyProperty<bool>("Hide", DefaultValue = false)]
public partial class PoseViewBase : View
{
	public const double MouseOverDistance = 20;

	private readonly Dictionary<string, List<PoseSelectionControl>> controlNameLookup = new();
	private readonly Dictionary<BoneId, List<PoseSelectionControl>> controlIdLookup = new();
	private readonly Dictionary<ISelectionId, List<PoseSelectionControl>> controlSelectionLookup = new();

	private List<PoseSelectionControl>? controls;
	private PoseTabItem? parent;

	public PoseViewBase()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);
	}

	public bool IsValid { get; private set; }

	public List<PoseSelectionControl>? GetTargets()
	{
		return this.controls;
	}

	public List<PoseSelectionControl>? GetTargets(BoneId boneId)
	{
		this.controlIdLookup.TryGetValue(boneId, out List<PoseSelectionControl>? views);
		return views;
	}

	public List<PoseSelectionControl>? GetTargets(string name)
	{
		this.controlNameLookup.TryGetValue(name, out List<PoseSelectionControl>? views);
		return views;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.controls == null)
			return;

		Point mousePos = Mouse.GetPosition(this);

		double closestDist = double.MaxValue;
		PoseSelectionControl? closestLink = null;
		foreach (PoseSelectionControl target in this.controls)
		{
			Point targetPos = target.TransformToAncestor(this).Transform(new Point(target.Width / 2, target.Height / 2));
			double distance = Point.Subtract(mousePos, targetPos).Length;
			if (distance < closestDist)
			{
				closestDist = distance;
				closestLink = target;
			}
		}

		if (closestLink != null && closestDist < MouseOverDistance)
		{
			this.Services.Selection.Hover = closestLink.Selection;
		}
		else
		{
			this.Services.Selection.Hover = null;
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.Services.Selection.Hover = null;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (this.Services.Selection.Hover != null)
		{
			ServiceManager.Instance.Selection.Current = this.Services.Selection.Hover;
			e.Handled = true;
		}
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (this.controls == null)
			return base.HitTestCore(hitTestParameters);

		double closestDist = double.MaxValue;
		PoseSelectionControl? closestLink = null;
		foreach (PoseSelectionControl link in this.controls)
		{
			Point targetPos = link.TransformToAncestor(this).Transform(new Point(link.ActualWidth / 2, link.ActualHeight));
			double distance = Point.Subtract(hitTestParameters.HitPoint, targetPos).Length;
			if (distance < closestDist)
			{
				closestDist = distance;
				closestLink = link;
			}
		}

		if (closestLink != null && closestDist < MouseOverDistance)
		{
			return base.HitTestCore(hitTestParameters);
		}
		else
		{
			return null;
		}
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.parent = this.FindLogicalParent<PoseTabItem>();

		this.Services.Target.TargetChanged += this.OnTargetChanged;
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.HoverChanged += this.OnHoverChanged;

		this.UpdateTargets();
	}

	protected override void OnUnloaded()
	{
		base.OnUnloaded();

		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.HoverChanged -= this.OnHoverChanged;
	}

	protected void UpdateTargets()
	{
		this.UpdateTargetsAsync().Run();
	}

	protected virtual async Task UpdateTargetsAsync()
	{
		if (this.Hide)
		{
			this.Visibility = Visibility.Hidden;
			this.IsValid = false;
			this.parent?.OnViewIsValidChanged(this, this.IsValid);
			return;
		}

		this.controls = this.FindLogicalChildren<PoseSelectionControl>();

		await Threads.FrameworkThread();

		int objectTableIndex = this.Services.Target.TargetObjectIndex;

		/*
		// Check our object table index
		bool result = await this.LoadFromTable(objectTableIndex, definition);

		// check for ornaments
		if (!result)
		{
			int ornamentTableIndex = -1;
			unsafe
			{
				Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
				Ornament* ornament = character->OrnamentData.OrnamentObject;

				if (ornament != null)
				{
					ornamentTableIndex = ornament->ObjectIndex;
				}
			}

			result = await this.LoadFromTable(ornamentTableIndex, definition);
		}

		// TODO: check for mounts?
		}*/

		unsafe
		{
			Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);
			if (pCharacter == null)
				return;

			foreach (PoseSelectionControl control in this.controls)
			{
				this.PopulateControl(control, pCharacter);
			}
		}

		await this.MainThread();

		int validCount = 0;
		foreach (PoseSelectionControl control in this.controls)
		{
			if (control.IsSafeValid)
				validCount++;

			control.IsValid = control.IsSafeValid;
		}

		if (validCount <= 0)
		{
			this.Visibility = Visibility.Hidden;
			this.IsValid = false;
			this.parent?.OnViewIsValidChanged(this, this.IsValid);
		}
		else
		{
			this.Visibility = Visibility.Visible;
			this.OnHoverChanged(null, this.Services.Selection.Hover);
			this.OnSelectionChanged(null, this.Services.Selection.Current);
			this.IsValid = true;
			this.parent?.OnViewIsValidChanged(this, this.IsValid);
		}
	}

	partial void OnHideChanged()
	{
		this.UpdateTargets();
	}

	private void OnTargetChanged(int objectTableIndex)
	{
		this.UpdateTargets();
	}

	private void ForEachControlInSelection(SelectionBase? selection, Action<PoseSelectionControl> action)
	{
		if (selection != null)
		{
			if (this.controlSelectionLookup.TryGetValue(selection.Id, out List<PoseSelectionControl>? controls) && controls != null)
			{
				foreach(PoseSelectionControl control in controls)
				{
					action.Invoke(control);
				}
			}
		}
	}

	private void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.ForEachControlInSelection(oldSelection, (c) => c.IsSelected = false);
			this.ForEachControlInSelection(newSelection, (c) => c.IsSelected = true);
		});
	}

	private void OnHoverChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.ForEachControlInSelection(oldSelection, (c) => c.IsMouseHover = false);
			this.ForEachControlInSelection(newSelection, (c) => c.IsMouseHover = true);
		});
	}

	private unsafe void PopulateControl(PoseSelectionControl control, Character* pCharacter)
	{
		if (control.SafeName == null)
			return;

		try
		{
			if (!this.controlNameLookup.ContainsKey(control.SafeName))
				this.controlNameLookup.Add(control.SafeName, new());

			this.controlNameLookup[control.SafeName].Add(control);

			if (control.SafeName == "character")
			{
				control.Selection = new GameObjectSelection(pCharacter->ObjectIndex);
			}
			else
			{
				BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(pCharacter, control.SafeName);
				if (selection != null)
				{
					foreach (BoneId boneId in selection.BoneIds)
					{
						if (!this.controlIdLookup.ContainsKey(boneId))
							this.controlIdLookup.Add(boneId, new());

						this.controlIdLookup[boneId].Add(control);
					}

					control.Selection = selection;
				}
			}

			if (control.Selection == null)
			{
				control.IsSafeValid = false;
			}
			else
			{
				control.IsSafeValid = true;

				ISelectionId selectionId = control.Selection.Id;
				if (!this.controlSelectionLookup.ContainsKey(selectionId))
					this.controlSelectionLookup.Add(selectionId, new());

				this.controlSelectionLookup[selectionId].Add(control);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error getting bone selection for BoneView: {control.SafeName}");
		}
	}
}