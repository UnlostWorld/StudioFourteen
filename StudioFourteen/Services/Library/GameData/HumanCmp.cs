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

namespace StudioFourteen.Services.Library.GameData;

using System;
using System.Collections.Generic;
using global::Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Data;
using StudioFourteen.Services.Rendering;

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;

public static class HumanCmp
{
	private static readonly Entry[] Colors;

	static HumanCmp()
	{
		List<Entry> colors = new List<Entry>();

		try
		{
			byte[]? buffer = Studio.DataManager.GetFile<FileResource>("chara/xls/charamake/human.cmp")?.Data;

			if (buffer == null)
				throw new Exception("Failed to load human.cmp");

			int at = 0;
			while (at < buffer.Length)
			{
				int r = buffer[at + 0] & 0xFF;
				int g = buffer[at + 1] & 0xFF;
				int b = buffer[at + 2] & 0xFF;
				int a = buffer[at + 3] & 0xFF;

				Entry entry = default;
				entry.Color = new Color(buffer[at + 3], buffer[at + 0], buffer[at + 1], buffer[at + 2]);
				colors.Add(entry);

				at += 4;
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Failed to read game color data.");
		}

		Colors = colors.ToArray();
	}

	public static Entry[]? Get(CharaMakeType makeType, CharaMakeType.CharaMakeMenu menu)
	{
		return Get((CustomizeIndex)menu.Customize, (byte)makeType.Tribe.RowId, (Genders)makeType.Gender);
	}

	public static Entry[] Get(CustomizeIndex index, byte tribe, Genders gender)
	{
		switch (index)
		{
			case CustomizeIndex.SkinColor: return Span(GetTribeSkinStartIndex(tribe, gender), 192);
			case CustomizeIndex.EyeColor: return Span(0, 192);
			case CustomizeIndex.HairColor: return Span(GetTribeHairStartIndex(tribe, gender), 192);
			case CustomizeIndex.HairColor2: return Span(256, 192);
			case CustomizeIndex.FaceFeaturesColor: return Span(0, 192);
			case CustomizeIndex.LipColor: return GetLipColors();
			case CustomizeIndex.FacepaintColor: return Span(512, 224);
		}

		throw new NotSupportedException($"HumanCmp color not supported for customization index: {index}");
	}

	public static Entry[] GetLipColors()
	{
		List<Entry> entries = new List<Entry>();
		entries.AddRange(Span(512, 96));

		for (int i = 0; i < 32; i++)
		{
			Entry entry = default;
			entry.Skip = true;
			entries.Add(entry);
		}

		entries.AddRange(Span(1792, 96));

		return entries.ToArray();
	}

	private static Entry[] Span(int from, int count)
	{
		Entry[] entries = new Entry[count];

		if (Colors.Length <= 0)
			return entries;

		Array.Copy(Colors, from, entries, 0, count);
		return entries;
	}

	private static int GetTribeSkinStartIndex(byte tribe, Genders gender)
	{
		bool isMasculine = gender == Genders.Masculine;

		int genderValue = isMasculine ? 0 : 1;
		int listIndex = ((((int)tribe * 2) + genderValue) * 5) + 3;
		return listIndex * 256;
	}

	private static int GetTribeHairStartIndex(byte tribe, Genders gender)
	{
		bool isMasculine = gender == Genders.Masculine;

		int genderValue = isMasculine ? 0 : 1;
		int listIndex = ((((int)tribe * 2) + genderValue) * 5) + 4;
		return listIndex * 256;
	}

	public struct Entry
	{
		public string Hex => $"#{this.Color.R:X2}{this.Color.G:X2}{this.Color.B:X2}";
		public Color Color { get; set; }
		public bool Skip { get; set; }
	}
}
