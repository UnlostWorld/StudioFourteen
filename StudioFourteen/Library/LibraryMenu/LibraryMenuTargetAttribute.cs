// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Library.LibraryMenu;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public class LibraryMenuTargetAttribute : LibraryMenuAttributeBase
{
	public readonly IconChar? Icon;
	public readonly string Label;

	public LibraryMenuTargetAttribute(IconChar icon, string label)
	{
		this.Icon = icon;
		this.Label = StudioFourteen.Resources.Find(label, label);
	}

	public LibraryMenuTargetAttribute(string label)
	{
		this.Label = StudioFourteen.Resources.Find(label, label);
	}

	public override async Task<List<MenuEntry>> GetMenu(object methodTarget, MethodInfo method)
	{
		TargetService targetService = ServiceManager.Instance.Target;
		List<MenuEntry> results = new();

		// "Apply to Player Name"
		string label = $"{this.Label}: {targetService.CharacterName}";
		Action invoke = () => method.Invoke(methodTarget, [targetService.TargetObjectIndex]);
		results.Add(new(this.Icon, label, invoke));

		// "Apply to..."
		await Threads.FrameworkThread();

		MenuEntry applyToEntry = new(this.Icon, this.Label);
		results.Add(applyToEntry);

		bool isGroupPose = ServiceManager.Instance.Studio.IsOpenAndInGPose;
		int fromIndex = GroupPoseService.GPoseFirstCharacter;
		int toIndex = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;

		if (!isGroupPose)
		{
			fromIndex = 0;
			toIndex = Math.Min(targetService.ObjectTableCount, GroupPoseService.GPoseFirstCharacter);
		}

		unsafe
		{
			for (int i = fromIndex; i < toIndex; ++i)
			{
				if (i == targetService.TargetObjectIndex)
					continue;

				Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(i);
				if (pCharacter == null)
					continue;

				if (pCharacter->ObjectKind == ObjectKind.Ornament || pCharacter->ObjectKind == ObjectKind.Mount)
					continue;

				string? name = pCharacter->GetRoleOrDisplayName();
				if (string.IsNullOrEmpty(name))
					continue;

				Action invoke2 = () => method.Invoke(methodTarget, [i]);
				applyToEntry.Children.Add(new(IconChar.None, name, invoke2));
			}
		}

		return results;
	}
}