namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.Services;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Plugin;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.Utilities;
using FFXIVClientStructs.FFXIV.Common.Lua;
using ScreenshotStudio.Structs.Extensions;

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
		get => this.Races?.GetRow((int)this.GetCustomizeValue(CustomizeIndex.Race));
		set
		{
			if (value == null || value.Tribes.Count <= 0)
				return;

			Tribe? tribe = value.Tribes[0];
			if (tribe == null)
				return;

			this.SetCustomizeValue(CustomizeIndex.Race, (byte)value.RowId, false);
			this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)tribe.RowId, false);

			if (!value.Genders.Contains(this.Gender))
				this.SetCustomizeValue(CustomizeIndex.Gender, (byte)value.Genders[0], false);

			if (this.Tribe?.Ages.Contains(this.Age) == false)
				this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)Ages.Normal, false);

			this.UpdateCustomize(true);
		}
	}

	[AutoNotify]
	public unsafe Tribe? Tribe
	{
		get => this.Tribes?.GetRow((int)this.GetCustomizeValue(CustomizeIndex.Tribe));
		set
		{
			if (value == null)
				return;

			this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)value.RowId, false);

			if (!value.Ages.Contains(this.Age))
				this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)Ages.Normal, false);

			this.UpdateCustomize(true);
		}
	}

	[AutoNotify]
	public unsafe Genders Gender
	{
		get => (Genders)this.GetCustomizeValue(CustomizeIndex.Gender);
		set => this.SetCustomizeValue(CustomizeIndex.Gender, (byte)value);
	}

	[AutoNotify]
	public unsafe Ages Age
	{
		get => (Ages)this.GetCustomizeValue(CustomizeIndex.ModelType);
		set => this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)value);
	}

	[AutoNotify]
	public unsafe byte ActorHeight
	{
		get => this.GetCustomizeValue(CustomizeIndex.Height);
		set => this.SetCustomizeValue(CustomizeIndex.Height, value);
	}

	[AutoNotify]
	public unsafe byte Face
	{
		get => this.GetCustomizeValue(CustomizeIndex.FaceType);
		set => this.SetCustomizeValue(CustomizeIndex.FaceType, value);
	}

	[AutoNotify]
	public unsafe byte Hair
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairStyle);
		set => this.SetCustomizeValue(CustomizeIndex.HairStyle, value);
	}

	[AutoNotify]
	public unsafe bool EnableHighlights
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor2) != 0;
		set => this.SetCustomizeValue(CustomizeIndex.HairColor2, value ? (byte)128 : (byte)0);
	}

	[AutoNotify]
	public unsafe byte SkinTone
	{
		get => this.GetCustomizeValue(CustomizeIndex.SkinColor);
		set => this.SetCustomizeValue(CustomizeIndex.SkinColor, value);
	}

	[AutoNotify]
	public unsafe byte RightEyeColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.EyeColor);
		set => this.SetCustomizeValue(CustomizeIndex.EyeColor, value);
	}

	[AutoNotify]
	public unsafe byte HairTone
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor);
		set => this.SetCustomizeValue(CustomizeIndex.HairColor, value);
	}

	[AutoNotify]
	public unsafe byte Highlights
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor2);
		set => this.SetCustomizeValue(CustomizeIndex.HairColor2, value);
	}

	[AutoNotify]
	public unsafe Structs.Customize.FacialFeatures FacialFeature
	{
		get => (Structs.Customize.FacialFeatures)this.GetCustomizeValue(CustomizeIndex.FaceFeatures);
		set => this.SetCustomizeValue(CustomizeIndex.FaceFeatures, (byte)value);
	}

	[AutoNotify]
	public CharaMakeType.FacialFeatureOptions? FacialFeatureOptions => this.MakeType?.GetFacialFeatures(this.Face);

	[AutoNotify]
	public unsafe byte FacialFeatureColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.FaceFeaturesColor);
		set => this.SetCustomizeValue(CustomizeIndex.FaceFeaturesColor, value);
	}

	[AutoNotify]
	public unsafe byte Eyebrows
	{
		get => this.GetCustomizeValue(CustomizeIndex.Eyebrows);
		set => this.SetCustomizeValue(CustomizeIndex.Eyebrows, value);
	}

	[AutoNotify]
	public unsafe byte LeftEyeColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.EyeColor2);
		set => this.SetCustomizeValue(CustomizeIndex.EyeColor2, value);
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
		get => this.GetCustomizeValue(CustomizeIndex.EyeShape);
		set => this.SetCustomizeValue(CustomizeIndex.EyeShape, value);
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
		get => this.GetCustomizeValue(CustomizeIndex.NoseShape);
		set => this.SetCustomizeValue(CustomizeIndex.NoseShape, value);
	}

	[AutoNotify]
	public unsafe byte Jaw
	{
		get => this.GetCustomizeValue(CustomizeIndex.JawShape);
		set => this.SetCustomizeValue(CustomizeIndex.JawShape, value);
	}

	[AutoNotify]
	public unsafe byte MouthId
	{
		get => this.GetCustomizeValue(CustomizeIndex.LipStyle);
		set => this.SetCustomizeValue(CustomizeIndex.LipStyle, value);
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
		get => this.GetCustomizeValue(CustomizeIndex.LipColor);
		set => this.SetCustomizeValue(CustomizeIndex.LipColor, value);
	}

	[AutoNotify]
	public unsafe byte EarMuscleTailSize
	{
		get => this.GetCustomizeValue(CustomizeIndex.RaceFeatureSize);
		set => this.SetCustomizeValue(CustomizeIndex.RaceFeatureSize, value);
	}

	[AutoNotify]
	public unsafe byte TailEarsType
	{
		get => this.GetCustomizeValue(CustomizeIndex.RaceFeatureType);
		set => this.SetCustomizeValue(CustomizeIndex.RaceFeatureType, value);
	}

	[AutoNotify]
	public unsafe byte Bust
	{
		get => this.GetCustomizeValue(CustomizeIndex.BustSize);
		set => this.SetCustomizeValue(CustomizeIndex.BustSize, value);
	}

	[AutoNotify]
	public unsafe byte FacePaintId
	{
		get => this.GetCustomizeValue(CustomizeIndex.Facepaint);
		set => this.SetCustomizeValue(CustomizeIndex.Facepaint, value);
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
		get => this.GetCustomizeValue(CustomizeIndex.FacepaintColor);
		set
		{
			this.SetCustomizeValue(CustomizeIndex.FacepaintColor, value);
		}
	}

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	public unsafe byte GetCustomizeValue(CustomizeIndex option) => this.Actor->GetCustomizeValue(option);
	public unsafe bool SetCustomizeValue(CustomizeIndex option, byte value, bool apply = true) => this.Actor->SetCustomizeValue(option, value, Structs.Actor.UpdateSource.Interface, apply);
	public unsafe void UpdateCustomize(bool redraw) => this.Actor->UpdateCustomize(redraw, Structs.Actor.UpdateSource.Interface);
}