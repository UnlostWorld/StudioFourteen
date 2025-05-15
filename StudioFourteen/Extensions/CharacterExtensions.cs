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

namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using global::System;
using StudioFourteen;
using StudioFourteen.Interop.Structs;
using StudioFourteen.Rendering;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

public enum RenderMode : uint
{
	Draw = 0,
	Unload = 2,
	Load = 4,
}

[Flags]
public enum CharacterFlags : byte
{
	None = 0,
	WeaponsVisible = 1,
	WeaponsDrawn = 2,
	VisorToggle = 8,
}

public static partial class CharacterExtensions
{
	public static bool CanDraw(ref this Character self)
	{
		if (!self.IsReadyToDraw())
			return false;

		return self.RenderFlags == (int)RenderMode.Draw;
	}

	public static unsafe string? GetRoleOrDisplayName(ref this Character self)
	{
		string? nickname = ServiceManager.Instance.Roles.GetRole(self.ObjectIndex);
		if (nickname != null)
			return nickname;

		return self.GetDisplayName();
	}

	public static unsafe void SetDisplayName(ref this Character self, string displayName)
	{
		self.GameObject.SetDisplayName(displayName);
	}

	public static unsafe string GetDisplayName(ref this Character self)
	{
		string selfName = self.GameObject.GetDisplayName();

		if (self.CompanionOwnerId > 0)
		{
			Character* pOwner = (Character*)CharacterManager.Instance()->LookupBattleCharaByEntityId(self.CompanionOwnerId);
			if (pOwner != null)
			{
				string? ownersName = pOwner->GetDisplayName();
				if (ownersName != null)
				{
					ownersName = ownersName.Split(' ')[0];
					return $"{ownersName}'s {selfName}";
				}
			}
		}

		return selfName;
	}

	public static unsafe CharacterBase* GetCharacterBase(ref this Character self)
	{
		return (CharacterBase*)self.DrawObject;
	}

	public static unsafe float GetCharacterScale(ref this Character self)
	{
		CharacterBaseScale* pscale = (CharacterBaseScale*)self.DrawObject;

		if (pscale == null)
			return 1.0f;

		return pscale->ScaleFactor;
	}

	public static CharaMakeType? GetCharaMakeType(ref readonly this Character self)
	{
		return self.DrawData.CustomizeData.GetMakeType();
	}

	public static byte GetCustomizeValue(ref readonly this Character self, CustomizeIndex option)
	{
		return self.DrawData.CustomizeData.GetValue(option);
	}

	public static ObjectKind GetKind(ref this Character self)
	{
		return (ObjectKind)self.ObjectKind;
	}
}