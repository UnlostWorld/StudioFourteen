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
		this.Label = label;
	}

	public LibraryMenuTargetAttribute(string label)
	{
		this.Label = label;
	}

	public override async Task<List<MenuEntry>> GetMenu(LibraryEntryBase entry, MethodInfo method)
	{
		TargetService targetService = ServiceManager.Instance.Target;
		List<MenuEntry> results = new();

		if (DalamudServices.ObjectTable == null)
			return results;

		// "Apply to Player Name"
		string label = $"{this.Label}: {targetService.CharacterName}";
		Action invoke = () => method.Invoke(entry, [targetService.TargetObjectIndex]);
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
			toIndex = Math.Min(DalamudServices.ObjectTable.Length, GroupPoseService.GPoseFirstCharacter);
		}

		unsafe
		{
			for (int i = fromIndex; i < toIndex; ++i)
			{
				if (i == targetService.TargetObjectIndex)
					continue;

				Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(i);
				if (pCharacter == null)
					continue;

				if (pCharacter->ObjectKind == ObjectKind.Ornament || pCharacter->ObjectKind == ObjectKind.Mount)
					continue;

				string? name = pCharacter->GetRoleOrDisplayName();
				if (string.IsNullOrEmpty(name))
					continue;

				Action invoke2 = () => method.Invoke(entry, [i]);
				applyToEntry.Children.Add(new(IconChar.None, name, invoke2));
			}
		}

		return results;
	}
}