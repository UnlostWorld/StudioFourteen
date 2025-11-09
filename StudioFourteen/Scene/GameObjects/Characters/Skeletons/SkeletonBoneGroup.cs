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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using StudioFourteen.Posing;
using StudioFourteen.Scene;
using StudioFourteen.Xaml;

public class SkeletonBoneGroup : SceneObjectBase
{
	private readonly Skeleton skeleton;
	private readonly BoneGroup group;

	public SkeletonBoneGroup(Skeleton skeleton, BoneGroup group)
	{
		this.skeleton = skeleton;
		this.group = group;
		this.Name = group.Name;
	}

	public BoneGroup? BoneGroup => this.group;

	public override string Id => new($"BoneGroup:{this.group.Name}:{this.skeleton.ObjectIndex}");
	public override object? Icon => XamlResources.Find("ICON_Type_Bones");
	public override string TypeName => XamlResources.Find("LOC_Type_Bones", "Bones");

	public bool Contains(SceneObjectBase sceneObject)
	{
		if (sceneObject is SkeletonBone bone)
			return this.Contains(bone);

		return false;
	}

	public bool Contains(SkeletonBone bone)
	{
		return this.BoneGroup?.Bones.Contains(bone.BoneName) == true;
	}
}