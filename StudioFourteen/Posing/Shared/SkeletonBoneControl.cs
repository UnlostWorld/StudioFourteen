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

using System.Windows.Controls;
using DependencyPropertyGenerator;
using StudioFourteen.Scene;

[DependencyProperty<string>("SelectionName")]
[DependencyProperty<string>("Label")]
[DependencyProperty<bool>("IsMouseHover")]
[DependencyProperty<bool>("IsSelected")]
[DependencyProperty<bool>("IsParentSelected")]
[DependencyProperty<bool>("IsValid")]
public partial class SkeletonBoneControl : Control
{
	public SceneObjectBase? Selection { get; set; }
	public string? SafeName { get; private set; }
	public bool IsSafeValid { get; set; }

	public void OnHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		if (newSelection == null || this.Selection == null)
		{
			this.IsMouseHover = false;
			return;
		}

		bool isHover = newSelection.Id == this.Selection.Id;
		this.IsMouseHover = isHover;
	}

	public void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		if (newSelection == null || this.Selection == null)
		{
			this.IsSelected = false;
			return;
		}

		this.IsSelected = newSelection.Id == this.Selection.Id;

		this.IsParentSelected = false;

		// TODO
		/*if (this.Selection is SkeletonBone boneSelection && newSelection is SkeletonBone newBoneSelection)
		{
			foreach((BoneId selectedBoneId, _) in newBoneSelection.BonePaths)
			{
				foreach((BoneId boneId, List<BoneId> pathToRoot) in boneSelection.BonePaths)
				{
					if (pathToRoot.Contains(selectedBoneId))
					{
						this.IsParentSelected = true;
						return;
					}
				}
			}
		}*/
	}

	partial void OnSelectionNameChanged(string? newValue)
	{
		this.SafeName = newValue;
	}
}
