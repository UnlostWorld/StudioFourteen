// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.Services;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Plugin;
using System.Collections.Generic;

public partial class CustomizeWindow : PanelWindow
{
	public DataSheet<Race>? Races => this.Services.Data.GetSheet<Race>();
	public DataSheet<Tribe>? Tribes => this.Services.Data.GetSheet<Tribe>();

	public IEnumerable<Race?>? AvailableRaces => this.Services.Data.GetSheet<Race>()?.GetFrom(1);

	[AutoNotify] public unsafe bool HasValidTarget => this.Actor != null;
	[AutoNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Target->Name : "Nobody";

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
		set { }
	}

	[AutoNotify]
	public byte Hair
	{
		get => this.Customize.Hair;
		set { }
	}

	[AutoNotify]
	public byte HighlightType
	{
		get => this.Customize.HighlightType;
		set { }
	}

	[AutoNotify]
	public byte SkinTone
	{
		get => this.Customize.SkinTone;
		set { }
	}

	[AutoNotify]
	public byte RightEyeColor
	{
		get => this.Customize.RightEyeColor;
		set { }
	}

	[AutoNotify]
	public byte HairTone
	{
		get => this.Customize.HairTone;
		set { }
	}

	[AutoNotify]
	public byte Highlights
	{
		get => this.Customize.Highlights;
		set { }
	}

	[AutoNotify]
	public Customize.FacialFeatures FacialFeature
	{
		get => this.Customize.FacialFeature;
		set { }
	}

	[AutoNotify]
	public byte FacialFeatureColor
	{
		get => this.Customize.FacialFeatureColor;
		set { }
	}

	[AutoNotify]
	public byte Eyebrows
	{
		get => this.Customize.Eyebrows;
		set { }
	}

	[AutoNotify]
	public byte LeftEyeColor
	{
		get => this.Customize.LeftEyeColor;
		set { }
	}

	[AutoNotify]
	public byte Eyes
	{
		get => this.Customize.Eyes;
		set { }
	}

	[AutoNotify]
	public byte Nose
	{
		get => this.Customize.Nose;
		set { }
	}

	[AutoNotify]
	public byte Jaw
	{
		get => this.Customize.Jaw;
		set { }
	}

	[AutoNotify]
	public byte MouthId
	{
		get => this.Customize.MouthId;
		set { }
	}

	[AutoNotify]
	public byte LipsToneFurPattern
	{
		get => this.Customize.LipsToneFurPattern;
		set { }
	}

	[AutoNotify]
	public byte EarMuscleTailSize
	{
		get => this.Customize.EarMuscleTailSize;
		set { }
	}

	[AutoNotify]
	public byte TailEarsType
	{
		get => this.Customize.TailEarsType;
		set { }
	}

	[AutoNotify]
	public byte Bust
	{
		get => this.Customize.Bust;
		set { }
	}

	[AutoNotify]
	public byte FacePaintId
	{
		get => this.Customize.FacePaintId;
		set { }
	}

	[AutoNotify]
	public byte FacePaintColor
	{
		get => this.Customize.FacePaintColor;
		set { }
	}

	protected unsafe Actor* Actor => this.Services.Targets.GPoseTarget;
	protected unsafe ref Customize Customize => ref this.Actor->DrawData.Customize;

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
			if (redraw)
			{
				this.Actor->GameObject.DisableDraw();
				this.Actor->GameObject.EnableDraw();
			}
			else
			{
				////Customize custom = this.Actor->DrawData.Customize;
				////this.Actor->DrawData.Customize = custom;

				bool result = this.Actor->UpdateCustomize();

				if (!result)
				{
					this.Log.Error("Failed to update actor customize");
				}
			}
		});
	}
}
