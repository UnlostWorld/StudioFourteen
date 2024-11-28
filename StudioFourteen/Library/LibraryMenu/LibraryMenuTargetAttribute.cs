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

	public LibraryMenuTargetAttribute(IconChar icon, string label, Func<object, int, bool> enabledCallback)
	{
		this.Icon = icon;
		this.Label = StudioFourteen.Resources.Find(label, label);
	}

	public override async Task GetMenu(object methodTarget, MethodInfo method, ILibraryContextMenu menu)
	{
		TargetService targetService = ServiceManager.Instance.Target;
		List<MenuEntry> results = new();

		string canMethodName = $"Can{method.Name}";
		MethodInfo? canMethod = methodTarget.GetType().GetMethod(canMethodName, BindingFlags.Public | BindingFlags.Instance);

		// "Apply to Player Name"
		string label = $"{this.Label}: {targetService.CharacterName}";
		Action invoke = () => method.Invoke(methodTarget, [targetService.TargetObjectIndex]);

		MenuEntry newMenu = menu.AddMenu(this.Icon, label, invoke);

		if (canMethod != null)
		{
			object? can = canMethod.Invoke(methodTarget, [targetService.TargetObjectIndex]);
			if (can is Task<bool> taskCan)
			{
				newMenu.IsEnabled = await taskCan;
			}
		}

		// "Apply to..."
		await Threads.FrameworkThread();

		MenuEntry applyToEntry = menu.AddMenu(this.Icon, this.Label);

		bool isGroupPose = ServiceManager.Instance.Studio.IsOpenAndInGPose;
		int fromIndex = GroupPoseService.GPoseFirstCharacter;
		int toIndex = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;

		if (!isGroupPose)
		{
			fromIndex = 0;
			toIndex = Math.Min(targetService.ObjectTableCount, GroupPoseService.GPoseFirstCharacter);
		}

		List<(MenuEntry, int)> targets = new();
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
				targets.Add((applyToEntry.AddChild(IconChar.None, name, invoke2), i));
			}
		}

		int count = 0;
		foreach((MenuEntry target, int objectTableIndex) in targets)
		{
			if (canMethod != null)
			{
				object? can = canMethod.Invoke(methodTarget, [objectTableIndex]);
				if (can is Task<bool> taskCan)
				{
					newMenu.IsEnabled = await taskCan;

					if (newMenu.IsEnabled)
					{
						count++;
					}
				}
			}
			else
			{
				count++;
			}
		}

		applyToEntry.IsEnabled = count > 0;
	}
}