namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.Services;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Plugin;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;

public partial class CustomizeWindow : ActorWindow
{
	private CharaMakeType? makeType;
	private bool linkEyeColors = false;

	public DataSheet<Race>? Races => this.Services.Data.GetSheet<Race>();
	public DataSheet<Tribe>? Tribes => this.Services.Data.GetSheet<Tribe>();

	public IEnumerable<Race?>? AvailableRaces => this.Services.Data.GetSheet<Race>()?.GetFrom(1);

	[AutoNotify]
	public unsafe CharaMakeType? MakeType
	{
		get
		{
			if (this.makeType == null
				|| this.makeType.Tribe != this.Tribe
				|| this.makeType.Gender != this.Gender)
			{
				this.makeType = null;

				DataSheet<CharaMakeType>? charaMakeTypeSheet = this.Services.Data.GetSheet<CharaMakeType>();
				if (charaMakeTypeSheet == null)
					return null;

				foreach (CharaMakeType set in charaMakeTypeSheet)
				{
					if (set.Tribe != this.Tribe || set.Gender != this.Gender)
						continue;

					this.makeType = set;
					break;
				}
			}

			return this.makeType;
		}
	}

	[AutoNotify]
	public unsafe Race? Race
	{
		get => this.Races?.GetRow((int)this.Actor->GetCustomizeValue(CustomizeIndex.Race));
		set
		{
			if (value == null || value.Tribes.Count <= 0)
				return;

			Tribe? tribe = value.Tribes[0];
			if (tribe == null)
				return;

			this.Actor->SetCustomizeValue(CustomizeIndex.Race, (byte)value.RowId, false);
			this.Actor->SetCustomizeValue(CustomizeIndex.Tribe, (byte)tribe.RowId, false);

			if (!value.Genders.Contains(this.Gender))
				this.Actor->SetCustomizeValue(CustomizeIndex.Gender, (byte)value.Genders[0], false);

			if (this.Tribe?.Ages.Contains(this.Age) == false)
				this.Actor->SetCustomizeValue(CustomizeIndex.ModelType, (byte)Ages.Normal, false);

			this.Actor->UpdateCustomize(true);
		}
	}

	[AutoNotify]
	public unsafe Tribe? Tribe
	{
		get => this.Tribes?.GetRow((int)this.Actor->GetCustomizeValue(CustomizeIndex.Tribe));
		set
		{
			if (value == null)
				return;

			this.Actor->SetCustomizeValue(CustomizeIndex.Tribe, (byte)value.RowId, false);

			if (!value.Ages.Contains(this.Age))
				this.Actor->SetCustomizeValue(CustomizeIndex.ModelType, (byte)Ages.Normal, false);

			this.Actor->UpdateCustomize(true);
		}
	}

	[AutoNotify]
	public unsafe Genders Gender
	{
		get => (Genders)this.Actor->GetCustomizeValue(CustomizeIndex.Gender);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.Gender, (byte)value);
	}

	[AutoNotify]
	public unsafe Ages Age
	{
		get => (Ages)this.Actor->GetCustomizeValue(CustomizeIndex.ModelType);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.ModelType, (byte)value);
	}

	[AutoNotify]
	public unsafe byte ActorHeight
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.Height);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.Height, value);
	}

	[AutoNotify]
	public unsafe byte Face
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.FaceType);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.FaceType, value);
	}

	[AutoNotify]
	public unsafe byte Hair
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.HairStyle);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.HairStyle, value);
	}

	[AutoNotify]
	public unsafe bool EnableHighlights
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.HairColor2) != 0;
		set => this.Actor->SetCustomizeValue(CustomizeIndex.HairColor2, value ? (byte)128 : (byte)0);
	}

	[AutoNotify]
	public unsafe byte SkinTone
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.SkinColor);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.SkinColor, value);
	}

	[AutoNotify]
	public unsafe byte RightEyeColor
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.EyeColor);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.EyeColor, value);
	}

	[AutoNotify]
	public unsafe byte HairTone
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.HairColor);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.HairColor, value);
	}

	[AutoNotify]
	public unsafe byte Highlights
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.HairColor2);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.HairColor2, value);
	}

	[AutoNotify]
	public unsafe Structs.Customize.FacialFeatures FacialFeature
	{
		get => (Structs.Customize.FacialFeatures)this.Actor->GetCustomizeValue(CustomizeIndex.FaceFeatures);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.FaceFeatures, (byte)value);
	}

	[AutoNotify]
	public CharaMakeType.FacialFeatureOptions? FacialFeatureOptions => this.MakeType?.GetFacialFeatures(this.Face);

	[AutoNotify]
	public unsafe byte FacialFeatureColor
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.FaceFeaturesColor);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.FaceFeaturesColor, value);
	}

	[AutoNotify]
	public unsafe byte Eyebrows
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.Eyebrows);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.Eyebrows, value);
	}

	[AutoNotify]
	public unsafe byte LeftEyeColor
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.EyeColor2);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.EyeColor2, value);
	}

	[AutoNotify]
	public byte MainEyeColor
	{
		get => this.LeftEyeColor;
		set
		{
			if (this.LinkEyeColors)
				this.RightEyeColor = value;

			this.LeftEyeColor = value;
		}
	}

	[AutoNotify]
	public bool LinkEyeColors
	{
		get => this.linkEyeColors;
		set
		{
			this.linkEyeColors = value;
			if (value)
			{
				this.RightEyeColor = this.LeftEyeColor;
			}
		}
	}

	[AutoNotify]
	public unsafe byte Eyes
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.EyeShape);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.EyeShape, value);
	}

	[AutoNotify]
	public bool SmallIris
	{
		get => this.Eyes >= 128;
		set => this.Eyes = (byte)(this.EyeShape + (value ? 128 : 0));
	}

	[AutoNotify]
	public byte EyeShape
	{
		get => (byte)(this.Eyes - (this.SmallIris ? 128 : 0));
		set => this.Eyes = (byte)(value + (this.SmallIris ? 128 : 0));
	}

	[AutoNotify]
	public unsafe byte Nose
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.NoseShape);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.NoseShape, value);
	}

	[AutoNotify]
	public unsafe byte Jaw
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.JawShape);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.JawShape, value);
	}

	[AutoNotify]
	public unsafe byte MouthId
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.LipStyle);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.LipStyle, value);
	}

	[AutoNotify]
	public byte Mouth
	{
		get => (byte)(this.EnableLipColor ? this.MouthId - 128 : this.MouthId);
		set => this.MouthId = (byte)(this.EnableLipColor ? value - 128 : value);
	}

	[AutoNotify]
	public bool EnableLipColor
	{
		get => this.MouthId >= 128;
		set => this.MouthId = (byte)(this.Mouth + (value ? 128 : 0));
	}

	[AutoNotify]
	public unsafe byte LipsToneFurPattern
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.LipColor);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.LipColor, value);
	}

	[AutoNotify]
	public unsafe byte EarMuscleTailSize
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureSize);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.RaceFeatureSize, value);
	}

	[AutoNotify]
	public unsafe byte TailEarsType
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.RaceFeatureType, value);
	}

	[AutoNotify]
	public unsafe byte Bust
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.BustSize);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.BustSize, value);
	}

	[AutoNotify]
	public unsafe byte FacePaintId
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.Facepaint);
		set => this.Actor->SetCustomizeValue(CustomizeIndex.Facepaint, value);
	}

	[AutoNotify]
	public bool FlipFacePaint
	{
		get => this.FacePaintId > 128;
		set => this.FacePaintId = (byte)(this.FacePaint + (value ? 128 : 0));
	}

	[AutoNotify]
	public byte FacePaint
	{
		get => (byte)(this.FacePaintId - (this.FlipFacePaint ? 128 : 0));
		set => this.FacePaintId = (byte)(value + (this.FlipFacePaint ? 128 : 0));
	}

	[AutoNotify]
	public unsafe byte FacePaintColor
	{
		get => this.Actor->GetCustomizeValue(CustomizeIndex.FacepaintColor);
		set
		{
			this.Actor->SetCustomizeValue(CustomizeIndex.FacepaintColor, value);
		}
	}

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}
}
