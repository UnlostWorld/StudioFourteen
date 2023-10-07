namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.GameData.Excel;
using System;
using System.Collections.Generic;

public class CharaMakeCustomizeSheet : DataSheet<CharaMakeCustomize>
{
	public enum Features
	{
		Hair,
		FacePaint,
	}

	public List<CharaMakeCustomize> GetFeatureOptions(Features featureType, Tribe tribe, Genders gender)
	{
		List<CharaMakeCustomize> results = new List<CharaMakeCustomize>();
		uint fromIndex = (uint)this.GetFeatureStartIndex(featureType, tribe, gender);
		int count = this.GetFeatureLength(featureType);

		for (int i = 1; i < 200; i++)
		{
			CharaMakeCustomize? feature = this.FindFeatureById(fromIndex, count, (byte)i);

			if (feature == null)
				continue;

			results.Add(feature);
		}

		return results;
	}

	public CharaMakeCustomize? GetFeature(Features featureType, Tribe tribe, Genders gender, byte featureId)
	{
		List<CharaMakeCustomize> hairs = this.GetFeatureOptions(featureType, tribe, gender);
		foreach (CharaMakeCustomize hair in hairs)
		{
			if (hair.FeatureId == featureId)
			{
				return hair;
			}
		}

		return null;
	}

	private int GetFeatureStartIndex(Features featureType, Tribe tribe, Genders gender)
	{
		bool isMasc = gender == Genders.Masculine;

		if (featureType == Features.Hair)
		{
			switch ((Tribe.TribeRows)tribe.RowId)
			{
				case Tribe.TribeRows.Midlander: return isMasc ? 0 : 100;
				case Tribe.TribeRows.Highlander: return isMasc ? 200 : 300;
				case Tribe.TribeRows.Wildwood: return isMasc ? 400 : 500;
				case Tribe.TribeRows.Duskwight: return isMasc ? 400 : 500;
				case Tribe.TribeRows.Plainsfolk: return isMasc ? 600 : 700;
				case Tribe.TribeRows.Dunesfolk: return isMasc ? 600 : 700;
				case Tribe.TribeRows.SeekerOfTheSun: return isMasc ? 800 : 900;
				case Tribe.TribeRows.KeeperOfTheMoon: return isMasc ? 800 : 900;
				case Tribe.TribeRows.SeaWolf: return isMasc ? 1000 : 1100;
				case Tribe.TribeRows.Hellsguard: return isMasc ? 1000 : 1100;
				case Tribe.TribeRows.Raen: return isMasc ? 1200 : 1300;
				case Tribe.TribeRows.Xaela: return isMasc ? 1200 : 1300;
				case Tribe.TribeRows.Helions: return isMasc ? 1400 : 1500;
				case Tribe.TribeRows.TheLost: return isMasc ? 1400 : 1500;
				case Tribe.TribeRows.Rava: return isMasc ? 1600 : 1700;
				case Tribe.TribeRows.Veena: return isMasc ? 1600 : 1700;
			}
		}
		else if (featureType == Features.FacePaint)
		{
			switch ((Tribe.TribeRows)tribe.RowId)
			{
				case Tribe.TribeRows.Midlander: return isMasc ? 2000 : 2050;
				case Tribe.TribeRows.Highlander: return isMasc ? 2100 : 2150;
				case Tribe.TribeRows.Wildwood: return isMasc ? 2200 : 2250;
				case Tribe.TribeRows.Duskwight: return isMasc ? 2300 : 2350;
				case Tribe.TribeRows.Plainsfolk: return isMasc ? 2400 : 2450;
				case Tribe.TribeRows.Dunesfolk: return isMasc ? 2500 : 2550;
				case Tribe.TribeRows.SeekerOfTheSun: return isMasc ? 2600 : 2650;
				case Tribe.TribeRows.KeeperOfTheMoon: return isMasc ? 2700 : 2750;
				case Tribe.TribeRows.SeaWolf: return isMasc ? 2800 : 2850;
				case Tribe.TribeRows.Hellsguard: return isMasc ? 2900 : 2950;
				case Tribe.TribeRows.Raen: return isMasc ? 3000 : 3050;
				case Tribe.TribeRows.Xaela: return isMasc ? 3100 : 3150;
				case Tribe.TribeRows.Helions: return isMasc ? 3200 : 3250;
				case Tribe.TribeRows.TheLost: return isMasc ? 3300 : 3350;
				case Tribe.TribeRows.Rava: return isMasc ? 3400 : 3450;
				case Tribe.TribeRows.Veena: return isMasc ? 3500 : 3550;
			}
		}

		Logging.Shared.Error("Unrecognized tribe: " + tribe);
		return 0;
	}

	private int GetFeatureLength(Features featureType)
	{
		switch (featureType)
		{
			case Features.Hair: return 100;
			case Features.FacePaint: return 50;
		}

		throw new Exception("Unrecognized feature: " + featureType);
	}

	private CharaMakeCustomize? FindFeatureById(uint from, int length, byte value)
	{
		for (uint i = from; i < from + length; i++)
		{
			CharaMakeCustomize? feature = this.GetRow(i);
			if (feature == null)
				continue;

			if (feature.FeatureId == value)
			{
				return feature;
			}
		}

		return null;
	}
}
