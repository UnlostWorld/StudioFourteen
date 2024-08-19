namespace ScreenshotStudio.Services;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterNicknameService : ServiceBase
{
	private readonly Dictionary<int, string> nicknames = new();
	private readonly Dictionary<int, string> defaultNicknames = new();

	public string? GetNicknameOrDefault(int objectTableIndex)
	{
		string? nickname = this.GetNickname(objectTableIndex);
		if (nickname == null)
			return this.GetDefaultNickname(objectTableIndex);

		return nickname;
	}

	public string? GetNickname(int objectTableIndex)
	{
		string? name;
		this.nicknames.TryGetValue(objectTableIndex, out name);
		return name;
	}

	public void SetNickname(int objectTableIndex, string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			this.nicknames.Remove(objectTableIndex);
			return;
		}

		if (!this.nicknames.ContainsKey(objectTableIndex))
			this.nicknames.Add(objectTableIndex, name);

		this.nicknames[objectTableIndex] = name;
	}

	public string? GetDefaultNickname(int objectTableIndex)
	{
		string? name;
		this.defaultNicknames.TryGetValue(objectTableIndex, out name);
		return name;
	}

	public override Task Start()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		return base.Start();
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			return;

		for (int i = 0; i < DalamudServices.ObjectTable.Length; ++i)
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(i);
			if (pCharacter == null)
			{
				if (this.nicknames.ContainsKey(i))
					this.nicknames.Remove(i);

				if (this.defaultNicknames.ContainsKey(i))
					this.defaultNicknames.Remove(i);

				continue;
			}

			ObjectKind kind = pCharacter->GetKind();
			if (kind == ObjectKind.Player)
			{
				string raceName = this.GenerateTribeName(pCharacter);
				if (!this.defaultNicknames.ContainsKey(i))
					this.defaultNicknames.Add(i, raceName);

				this.defaultNicknames[i] = raceName;
			}
		}
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		this.nicknames.Remove(objectTableIndex);
	}

	private unsafe string GenerateTribeName(Character* character)
	{
		Threads.VerifyFrameworkThread();

		byte raceId = character->GetCustomizeValue(CustomizeIndex.Race);
		byte tribeId = character->GetCustomizeValue(CustomizeIndex.Tribe);
		Genders gender = (Genders)character->GetCustomizeValue(CustomizeIndex.Gender);

		Race? race = GameDataService.GetRow<Race>(raceId);
		Tribe? tribe = GameDataService.GetRow<Tribe>(tribeId);
		if (tribe != null && race != null)
		{
			string raceName;
			string tribeName;
			string genderName;
			if (gender == Genders.Feminine)
			{
				raceName = race.Feminine;
				tribeName = tribe.Feminine;
				genderName = "F";
			}
			else
			{
				raceName = race.Masculine;
				tribeName = tribe.Masculine;
				genderName = "M";
			}

			// Shorten Miqo tribe names. (in english...)
			tribeName = tribeName.Replace(" of the Moon", string.Empty);
			tribeName = tribeName.Replace(" of the Sun", string.Empty);

			return $"{raceName} {tribeName} {genderName}";
		}

		return "Unknown";
	}
}
