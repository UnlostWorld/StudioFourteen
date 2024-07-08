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
	public CharaMakeType? MakeType
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
	public Race? Race
	{
		get => this.Races?.GetRow((int)this.Customize.GetValue(CustomizeIndex.Race));
		set
		{
			if (value == null || value.Tribes.Count <= 0)
				return;

			Tribe? tribe = value.Tribes[0];
			if (tribe == null)
				return;

			this.Customize.SetValue(CustomizeIndex.Race, (byte)value.RowId);
			this.NotifyPropertyChanged(nameof(CustomizeWindow.Race));

			this.Customize.SetValue(CustomizeIndex.Tribe, (byte)tribe.RowId);
			this.NotifyPropertyChanged(nameof(CustomizeWindow.Tribe));

			if (!value.Genders.Contains(this.Gender))
				this.Gender = value.Genders[0];

			if (this.Tribe?.Ages.Contains(this.Age) == false)
				this.Age = Ages.Normal;

			this.Apply(true);
		}
	}

	[AutoNotify]
	public Tribe? Tribe
	{
		get => this.Tribes?.GetRow((int)this.Customize.GetValue(CustomizeIndex.Tribe));
		set
		{
			if (value == null)
				return;

			this.Customize.SetValue(CustomizeIndex.Tribe, (byte)value.RowId);

			if (!value.Ages.Contains(this.Age))
				this.Age = Ages.Normal;

			this.Apply(true);
		}
	}

	[AutoNotify]
	public Genders Gender
	{
		get => (Genders)this.Customize.GetValue(CustomizeIndex.Gender);
		set
		{
			this.Customize.SetValue(CustomizeIndex.Gender, (byte)value);
			this.Apply(true);
		}
	}

	[AutoNotify]
	public Ages Age
	{
		get => (Ages)this.Customize.GetValue(CustomizeIndex.ModelType);
		set
		{
			this.Customize.SetValue(CustomizeIndex.ModelType, (byte)value);
			this.Apply(true);
		}
	}

	[AutoNotify]
	public byte ActorHeight
	{
		get => this.Customize.GetValue(CustomizeIndex.Height);
		set
		{
			this.Customize.SetValue(CustomizeIndex.Height, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Face
	{
		get => this.Customize.GetValue(CustomizeIndex.FaceType);
		set
		{
			this.Customize.SetValue(CustomizeIndex.FaceType, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Hair
	{
		get => this.Customize.GetValue(CustomizeIndex.HairStyle);
		set
		{
			this.Customize.SetValue(CustomizeIndex.HairStyle, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public bool EnableHighlights
	{
		get => this.Customize.GetValue(CustomizeIndex.HairColor2) != 0;
		set
		{
			this.Customize.SetValue(CustomizeIndex.HairColor2, value ? (byte)128 : (byte)0);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte SkinTone
	{
		get => this.Customize.GetValue(CustomizeIndex.SkinColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.SkinColor, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte RightEyeColor
	{
		get => this.Customize.GetValue(CustomizeIndex.EyeColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.EyeColor, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte HairTone
	{
		get => this.Customize.GetValue(CustomizeIndex.HairColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.HairColor, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Highlights
	{
		get => this.Customize.GetValue(CustomizeIndex.HairColor2);
		set
		{
			this.Customize.SetValue(CustomizeIndex.HairColor2, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public Structs.Customize.FacialFeatures FacialFeature
	{
		get => (Structs.Customize.FacialFeatures)this.Customize.GetValue(CustomizeIndex.FaceFeatures);
		set
		{
			this.Customize.SetValue(CustomizeIndex.FaceFeatures, (byte)value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public CharaMakeType.FacialFeatureOptions? FacialFeatureOptions => this.MakeType?.GetFacialFeatures(this.Face);

	[AutoNotify]
	public byte FacialFeatureColor
	{
		get => this.Customize.GetValue(CustomizeIndex.FaceFeaturesColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.FaceFeaturesColor, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Eyebrows
	{
		get => this.Customize.GetValue(CustomizeIndex.Eyebrows);
		set
		{
			this.Customize.SetValue(CustomizeIndex.Eyebrows, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte LeftEyeColor
	{
		get => this.Customize.GetValue(CustomizeIndex.EyeColor2);
		set
		{
			this.Customize.SetValue(CustomizeIndex.EyeColor2, value);
			this.Apply(false);
		}
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
	public byte Eyes
	{
		get => this.Customize.GetValue(CustomizeIndex.EyeShape);
		set
		{
			this.Customize.SetValue(CustomizeIndex.EyeShape, value);
			this.Apply(false);
		}
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
	public byte Nose
	{
		get => this.Customize.GetValue(CustomizeIndex.NoseShape);
		set
		{
			this.Customize.SetValue(CustomizeIndex.NoseShape, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Jaw
	{
		get => this.Customize.GetValue(CustomizeIndex.JawShape);
		set
		{
			this.Customize.SetValue(CustomizeIndex.JawShape, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte MouthId
	{
		get => this.Customize.GetValue(CustomizeIndex.LipStyle);
		set
		{
			this.Customize.SetValue(CustomizeIndex.LipStyle, value);
			this.Apply(false);
		}
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
	public byte LipsToneFurPattern
	{
		get => this.Customize.GetValue(CustomizeIndex.LipColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.LipColor, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte EarMuscleTailSize
	{
		get => this.Customize.GetValue(CustomizeIndex.RaceFeatureSize);
		set
		{
			this.Customize.SetValue(CustomizeIndex.RaceFeatureSize, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte TailEarsType
	{
		get => this.Customize.GetValue(CustomizeIndex.RaceFeatureType);
		set
		{
			this.Customize.SetValue(CustomizeIndex.RaceFeatureType, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Bust
	{
		get => this.Customize.GetValue(CustomizeIndex.BustSize);
		set
		{
			this.Customize.SetValue(CustomizeIndex.BustSize, value);
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte FacePaintId
	{
		get => this.Customize.GetValue(CustomizeIndex.Facepaint);
		set
		{
			this.Customize.SetValue(CustomizeIndex.Facepaint, value);
			this.Apply(false);
		}
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
	public byte FacePaintColor
	{
		get => this.Customize.GetValue(CustomizeIndex.FacepaintColor);
		set
		{
			this.Customize.SetValue(CustomizeIndex.FacepaintColor, value);
			this.Apply(false);
		}
	}

	protected unsafe ref Structs.Customize Customize => ref this.Actor->DrawData.Customize;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	private unsafe void Apply(bool redraw)
	{
		if (this.Actor == null)
			return;

		/*DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			if (!redraw)
			{
				bool result = this.Actor->UpdateCustomize();
				if (!result)
				{
					this.Log.Warning("Failed to update actor customize. falling back to redraw.");
					redraw = true;
				}
			}

			if (redraw)
			{
				this.Actor->GameObject.DisableDraw();
				this.Actor->GameObject.EnableDraw();
			}
		});*/
	}
}
