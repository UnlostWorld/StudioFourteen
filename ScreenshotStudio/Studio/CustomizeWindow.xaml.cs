namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.Services;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Plugin;
using System.Collections.Generic;

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
		get => this.Races?.GetRow((int)this.Customize.Race);
		set
		{
			if (value == null || value.Tribes.Count <= 0)
				return;

			Tribe? tribe = value.Tribes[0];
			if (tribe == null)
				return;

			this.Customize.Race = (Race.RaceRows)value.RowId;
			this.NotifyPropertyChanged(nameof(CustomizeWindow.Race));

			this.Customize.Tribe = (Tribe.TribeRows)tribe.RowId;
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
		get => this.Tribes?.GetRow((int)this.Customize.Tribe);
		set
		{
			if (value == null)
				return;

			this.Customize.Tribe = (Tribe.TribeRows)value.RowId;

			if (!value.Ages.Contains(this.Age))
				this.Age = Ages.Normal;

			this.Apply(true);
		}
	}

	[AutoNotify]
	public Genders Gender
	{
		get => this.Customize.Gender;
		set
		{
			this.Customize.Gender = value;
			this.Apply(true);
		}
	}

	[AutoNotify]
	public Ages Age
	{
		get => this.Customize.Age;
		set
		{
			this.Customize.Age = value;
			this.Apply(true);
		}
	}

	[AutoNotify]
	public byte ActorHeight
	{
		get => this.Customize.Height;
		set
		{
			this.Customize.Height = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Face
	{
		get => this.Customize.Face;
		set
		{
			this.Customize.Face = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Hair
	{
		get => this.Customize.Hair;
		set
		{
			this.Customize.Hair = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public bool EnableHighlights
	{
		get => this.Customize.HighlightType != 0;
		set
		{
			this.Customize.HighlightType = value ? (byte)128 : (byte)0;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte SkinTone
	{
		get => this.Customize.SkinTone;
		set
		{
			this.Customize.SkinTone = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte RightEyeColor
	{
		get => this.Customize.RightEyeColor;
		set
		{
			this.Customize.RightEyeColor = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte HairTone
	{
		get => this.Customize.HairTone;
		set
		{
			this.Customize.HairTone = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Highlights
	{
		get => this.Customize.Highlights;
		set
		{
			this.Customize.Highlights = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public Structs.Customize.FacialFeatures FacialFeature
	{
		get => this.Customize.FacialFeature;
		set
		{
			this.Customize.FacialFeature = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public CharaMakeType.FacialFeatureOptions? FacialFeatureOptions => this.MakeType?.GetFacialFeatures(this.Face);

	[AutoNotify]
	public byte FacialFeatureColor
	{
		get => this.Customize.FacialFeatureColor;
		set
		{
			this.Customize.FacialFeatureColor = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Eyebrows
	{
		get => this.Customize.Eyebrows;
		set
		{
			this.Customize.Eyebrows = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte LeftEyeColor
	{
		get => this.Customize.LeftEyeColor;
		set
		{
			this.Customize.LeftEyeColor = value;
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
		get => this.Customize.Eyes;
		set
		{
			this.Customize.Eyes = value;
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
		get => this.Customize.Nose;
		set
		{
			this.Customize.Nose = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Jaw
	{
		get => this.Customize.Jaw;
		set
		{
			this.Customize.Jaw = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte MouthId
	{
		get => this.Customize.MouthId;
		set
		{
			this.Customize.MouthId = value;
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
		get => this.Customize.LipsToneFurPattern;
		set
		{
			this.Customize.LipsToneFurPattern = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte EarMuscleTailSize
	{
		get => this.Customize.EarMuscleTailSize;
		set
		{
			this.Customize.EarMuscleTailSize = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte TailEarsType
	{
		get => this.Customize.TailEarsType;
		set
		{
			this.Customize.TailEarsType = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte Bust
	{
		get => this.Customize.Bust;
		set
		{
			this.Customize.Bust = value;
			this.Apply(false);
		}
	}

	[AutoNotify]
	public byte FacePaintId
	{
		get => this.Customize.FacePaintId;
		set
		{
			this.Customize.FacePaintId = value;
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
		get => this.Customize.FacePaintColor;
		set
		{
			this.Customize.FacePaintColor = value;
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

		DalamudServices.Framework.RunOnFrameworkThread(() =>
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
		});
	}
}
