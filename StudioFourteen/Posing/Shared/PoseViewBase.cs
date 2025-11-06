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
using System.Windows;
using StudioFourteen;
using System;
using System.Windows.Input;
using System.Windows.Media;
using DependencyPropertyGenerator;
using StudioFourteen.Services;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using Character = StudioFourteen.Scene.GameObjects.Characters.Character;

[DependencyProperty<bool>("Hide", DefaultValue = false)]
[DependencyProperty<bool>("UpdateWithAppearance", DefaultValue = false)]
[DependencyProperty<GameObject>("GameObject")]
public partial class PoseViewBase : View
{
	public const double MouseOverDistance = 20;

	private readonly Dictionary<string, List<SkeletonBoneControl>> controlNameLookup = new();
	private readonly Dictionary<BoneId, List<SkeletonBoneControl>> controlIdLookup = new();
	private readonly Dictionary<string, List<SkeletonBoneControl>> controlSelectionLookup = new();

	private List<SkeletonBoneControl>? controls;
	private PoseTabItem? parent;
	private bool isUpdatingTargets = false;

	public PoseViewBase()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);
	}

	public bool IsValid { get; private set; }

	public Character? Character { get; private set; }

	public List<SkeletonBoneControl>? GetTargets()
	{
		return this.controls;
	}

	public List<SkeletonBoneControl>? GetTargets(BoneId boneId)
	{
		this.controlIdLookup.TryGetValue(boneId, out List<SkeletonBoneControl>? views);
		return views;
	}

	public List<SkeletonBoneControl>? GetTargets(string name)
	{
		this.controlNameLookup.TryGetValue(name, out List<SkeletonBoneControl>? views);
		return views;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.controls == null)
			return;

		Point mousePos = Mouse.GetPosition(this);

		double closestDist = double.MaxValue;
		SkeletonBoneControl? closestLink = null;
		foreach (SkeletonBoneControl target in this.controls)
		{
			try
			{
				Point targetPos = target.TransformToAncestor(this).Transform(new Point(target.Width / 2, target.Height / 2));
				double distance = Point.Subtract(mousePos, targetPos).Length;
				if (distance < closestDist)
				{
					closestDist = distance;
					closestLink = target;
				}
			}
			catch (Exception)
			{
			}
		}

		if (closestLink != null && closestDist < MouseOverDistance)
		{
			this.Services.Selection.HoverSource = closestLink;
			this.Services.Selection.HoverSelection(closestLink.Selection, this);
		}
		else
		{
			this.Services.Selection.ClearHover();
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.Services.Selection.ClearHover();
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton != MouseButton.Left)
			return;

		if (this.Services.Selection.Hover != null)
		{
			ServiceManager.Instance.Selection.Select(this.Services.Selection.Hover, this);
			return;
		}
		else
		{
			ServiceManager.Instance.Selection.Select(this.GameObject, this);
		}

		e.Handled = true;
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (this.controls == null)
			return base.HitTestCore(hitTestParameters);

		double closestDist = double.MaxValue;
		SkeletonBoneControl? closestLink = null;
		foreach (SkeletonBoneControl link in this.controls)
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

		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.HoverChanged += this.OnHoverChanged;
		this.Services.CharacterAppearance.OnAppearanceChanged += this.OnAppearanceChanged;

		this.UpdateTargets();
	}

	protected override void OnUnloaded()
	{
		base.OnUnloaded();

		if (ServiceManager.ShutdownRequested)
			return;

		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.HoverChanged -= this.OnHoverChanged;
		this.Services.CharacterAppearance.OnAppearanceChanged -= this.OnAppearanceChanged;

		this.ClearTargets();
	}

	protected virtual void ClearTargets()
	{
		this.controls?.Clear();
		this.controlNameLookup.Clear();
		this.controlIdLookup.Clear();
		this.controlSelectionLookup.Clear();
	}

	protected void UpdateTargets()
	{
		this.UpdateTargetsAsync().RunAsynchronously();
	}

	protected virtual async Task UpdateTargetsAsync()
	{
		while (this.isUpdatingTargets)
			await Task.Delay(100);

		this.isUpdatingTargets = true;
		try
		{
			await Task.Delay(10);
			await this.MainThread();

			if (this.Hide)
			{
				this.Visibility = Visibility.Hidden;
				this.IsValid = false;
				this.parent?.OnViewIsValidChanged(this, this.IsValid);
				return;
			}

			this.controls = this.FindLogicalChildren<SkeletonBoneControl>();

			if (this.GameObject == null)
				return;

			// Try character
			bool isValid = await this.PopulateControl(this.GameObject);

			// Try ornaments
			/*if (!isValid)
			{
				await TickService.GameTick();
				int ornamentTableIndex = -1;
				unsafe
				{
					Character* character = this.Services.GameObjects.Get<Character>(this.ObjectTableIndex);
					if (character != null)
					{
						Ornament* ornament = character->OrnamentData.OrnamentObject;
						if (ornament != null)
						{
							ornamentTableIndex = ornament->ObjectIndex;
						}
					}
				}

				isValid = await this.PopulateControl(ornamentTableIndex);
			}

			// TODO: Mounts
			if (!isValid)
			{
			}*/

			await this.MainThread();

			if (!isValid)
			{
				this.Visibility = Visibility.Hidden;
				this.IsValid = false;
				this.parent?.OnViewIsValidChanged(this, this.IsValid);
			}
			else
			{
				this.Visibility = Visibility.Visible;
				this.OnHoverChanged(null, this.Services.Selection.Hover, null);
				this.OnSelectionChanged(null, this.Services.Selection.Current, null);
				this.IsValid = true;
				this.parent?.OnViewIsValidChanged(this, this.IsValid);
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error updating pose view");
		}
		finally
		{
			this.isUpdatingTargets = false;
		}
	}

	protected async Task<bool> PopulateControl(GameObject gameObject)
	{
		if (this.controls == null)
			return false;

		await TickService.GameTick();

		unsafe
		{
			foreach (SkeletonBoneControl control in this.controls)
			{
				this.PopulateControl(control, gameObject);
			}
		}

		await this.MainThread();

		int validCount = 0;
		foreach (SkeletonBoneControl control in this.controls)
		{
			if (control.IsSafeValid)
				validCount++;

			control.IsValid = control.IsSafeValid;
		}

		return validCount > 0;
	}

	protected virtual void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		try
		{
			this.Dispatcher.BeginInvoke(() =>
			{
				if (this.controls != null)
				{
					foreach (SkeletonBoneControl control in this.controls)
					{
						control.OnSelectionChanged(oldSelection, newSelection, source);
					}
				}
			});
		}
		catch (TaskCanceledException)
		{
		}
	}

	protected virtual void OnHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			if (this.controls != null)
			{
				foreach(SkeletonBoneControl control in this.controls)
				{
					control.OnHoverChanged(oldSelection, newSelection, source);
				}
			}
		});
	}

	partial void OnHideChanged()
	{
		this.UpdateTargets();
	}

	partial void OnGameObjectChanged(GameObject? newValue)
	{
		this.Character = newValue as Character;
		this.UpdateTargets();
	}

	private void OnAppearanceChanged(int objectTableIndex)
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			if (this.UpdateWithAppearance)
			{
				this.UpdateTargets();
			}
		});
	}

	private unsafe void PopulateControl(SkeletonBoneControl control, GameObject gameObject)
	{
		if (control.SafeName == null)
			return;

		try
		{
			if (this.Settings.HideGenitals && this.Services.Content.GenitalBones?.Contains(control.SafeName) == true)
				return;

			if (!this.controlNameLookup.ContainsKey(control.SafeName))
				this.controlNameLookup.Add(control.SafeName, new());

			this.controlNameLookup[control.SafeName].Add(control);

			if (control.SafeName == "character")
			{
				control.Selection = gameObject;
			}
			else if (gameObject is Skeleton skeleton)
			{
				SkeletonBone? selection = skeleton.FindBone(control.SafeName);
				if (selection != null)
				{
					foreach (BoneReference reference in selection.BoneReferences)
					{
						if (!this.controlIdLookup.ContainsKey(reference.Id))
							this.controlIdLookup.Add(reference.Id, new());

						this.controlIdLookup[reference.Id].Add(control);
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

				string selectionId = control.Selection.Id;
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