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

public class PoseViewBase : View
{
	public const double MouseOverDistance = 20;

	private readonly Dictionary<string, List<PoseSelectionControl>> targetNameLookup = new();
	private readonly Dictionary<BoneId, List<PoseSelectionControl>> targetIdLookup = new();

	private List<PoseSelectionControl>? targets;
	private PoseSelectionControl? mouseOver;

	public PoseViewBase()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;

		this.Background = new SolidColorBrush(Colors.Transparent);
	}

	public PoseSelectionControl? MouseOver
	{
		get => this.mouseOver;
		set
		{
			if (this.mouseOver == value)
				return;

			if (this.mouseOver != null)
				this.mouseOver.IsMouseHover = false;

			this.mouseOver = value;

			if (this.mouseOver != null)
				this.mouseOver.IsMouseHover = true;
		}
	}

	public List<PoseSelectionControl>? GetTargets()
	{
		return this.targets;
	}

	public List<PoseSelectionControl>? GetTargets(BoneId boneId)
	{
		this.targetIdLookup.TryGetValue(boneId, out List<PoseSelectionControl>? views);
		return views;
	}

	public List<PoseSelectionControl>? GetTargets(string name)
	{
		this.targetNameLookup.TryGetValue(name, out List<PoseSelectionControl>? views);
		return views;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.targets == null)
			return;

		Point mousePos = Mouse.GetPosition(this);

		double closestDist = double.MaxValue;
		PoseSelectionControl? closestLink = null;
		foreach (PoseSelectionControl target in this.targets)
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
			this.MouseOver = closestLink;
		}
		else
		{
			this.MouseOver = null;
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.MouseOver = null;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (this.MouseOver != null)
		{
			ServiceManager.Instance.Selection.Current = this.MouseOver.Selection;
			e.Handled = true;
		}
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (this.targets == null)
			return base.HitTestCore(hitTestParameters);

		double closestDist = double.MaxValue;
		PoseSelectionControl? closestLink = null;
		foreach (PoseSelectionControl link in this.targets)
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
		this.UpdateTargets();
	}

	protected void UpdateTargets()
	{
		this.UpdateTargetsAsync().Run();
	}

	protected virtual async Task UpdateTargetsAsync()
	{
		this.targets = this.FindChildren<PoseSelectionControl>();

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

			foreach (PoseSelectionControl view in this.targets)
			{
				this.PopulateView(view, pCharacter);
			}
		}

		await this.MainThread();
	}

	private void OnTargetChanged(int objectTableIndex)
	{
		this.UpdateTargets();
	}

	private unsafe bool PopulateView(PoseSelectionControl view, Character* pCharacter)
	{
		if (view.SafeName == null)
			return false;

		try
		{
			if (!this.targetNameLookup.ContainsKey(view.SafeName))
				this.targetNameLookup.Add(view.SafeName, new());

			this.targetNameLookup[view.SafeName].Add(view);

			if (view.SafeName == "character")
			{
				view.Selection = new GameObjectSelection(pCharacter->ObjectIndex);
				return true;
			}

			BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(pCharacter, view.SafeName);
			if (selection != null)
			{
				foreach (BoneId boneId in selection.BoneIds)
				{
					if (!this.targetIdLookup.ContainsKey(boneId))
						this.targetIdLookup.Add(boneId, new());

					this.targetIdLookup[boneId].Add(view);
				}

				view.Selection = selection;
				return true;
			}

			return false;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error getting bone selection for BoneView: {view.SafeName}");
			return false;
		}
	}

	/*private void OnSelectionChanged(object? newSelection)
	{
		if (newSelection is SelectionBase selection)
		{
			this.Dispatcher.Invoke(() =>
			{
				foreach (var link in this.boneButtons)
				{
					link.IsSelected = link.Selection.Equals(selection);
				}
			});
		}
	}*/
}