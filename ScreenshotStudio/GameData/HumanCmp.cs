namespace ScreenshotStudio.GameData;

using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Data;
using ScreenshotStudio.GameData.Excel;
using Serilog;
using System.Windows.Media;

public static class HumanCmp
{
	private static readonly Entry[] Colors;

	static HumanCmp()
	{
		List<Entry> colors = new List<Entry>();

		try
		{
			byte[]? buffer = GameDataService.GetFile<FileResource>("chara/xls/charamake/human.cmp")?.Data;

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

				// Hey do this
				////entry.CmColor = new cmColor(r / 255.0f, g / 255.0f, b / 255.0f);

				Color c2 = (Color)entry.Color;
				c2.R = buffer[at + 0];
				c2.G = buffer[at + 1];
				c2.B = buffer[at + 2];
				c2.A = buffer[at + 3];
				entry.Color = c2;

				////= new Color.FromArgb((a << 24) | (r << 16) | (g << 8) | b);

				colors.Add(entry);

				at += 4;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to read game color data.");
		}

		Colors = colors.ToArray();
	}

	public static Entry[]? Get(CharaMakeType.Menu menu)
	{
		if (menu.Race == null || menu.Tribe == null)
			return null;

		return Get(menu.CustomizationIndex, menu.Tribe, menu.Gender);
	}

	public static Entry[] Get(CustomizeIndex index, Tribe tribe, Genders gender)
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

	private static int GetTribeSkinStartIndex(Tribe tribe, Genders gender)
	{
		bool isMasculine = gender == Genders.Masculine;

		int genderValue = isMasculine ? 0 : 1;
		int listIndex = ((((int)tribe.RowId * 2) + genderValue) * 5) + 3;
		return listIndex * 256;
	}

	private static int GetTribeHairStartIndex(Tribe tribe, Genders gender)
	{
		bool isMasculine = gender == Genders.Masculine;

		int genderValue = isMasculine ? 0 : 1;
		int listIndex = ((((int)tribe.RowId * 2) + genderValue) * 5) + 4;
		return listIndex * 256;
	}

	public struct Entry
	{
		public string Hex => $"#{this.Color.R:X2}{this.Color.G:X2}{this.Color.B:X2}";
		public Color Color { get; set; }
		public bool Skip { get; set; }
	}
}
