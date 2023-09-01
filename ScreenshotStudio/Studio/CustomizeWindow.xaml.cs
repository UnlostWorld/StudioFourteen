// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.Services;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ImGuiNET;
using System.ComponentModel;
using ScreenshotStudio.Plugin;

public partial class CustomizeWindow : PanelWindow
{
	public CustomizeWindow()
	{
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public DataSheet<Race>? Races => this.Services.Data.GetSheet<Race>();
	public DataSheet<Tribe>? Tribes => this.Services.Data.GetSheet<Tribe>();

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
			this.Customize.Tribe = (Tribe.TribeRows)tribe.RowId;
		}
	}

	[AutoNotify]
	public Genders Gender
	{
		get => this.Customize.Gender;
		set { }
	}

	[AutoNotify]
	public Customize.Ages Age
	{
		get => this.Customize.Age;
		set { }
	}

	[AutoNotify]
	public byte ActorHeight
	{
		get => this.Customize.Height;
		set { }
	}

	[AutoNotify]
	public Tribe? Tribe
	{
		get => this.Tribes?.GetRow((int)this.Customize.Tribe);
		set => this.Customize.Tribe = value != null ? (Tribe.TribeRows)value.RowId : Tribe.TribeRows.Midlander;
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

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.Log.Information($"Changed {e.PropertyName}");

		bool needsRedraw = e.PropertyName == nameof(CustomizeWindow.Race)
			|| e.PropertyName == nameof(CustomizeWindow.Tribe)
			|| e.PropertyName == nameof(CustomizeWindow.Gender)
			|| e.PropertyName == nameof(CustomizeWindow.Face);

		this.Apply(needsRedraw);
	}

	private unsafe void Apply(bool redraw)
	{
		if (this.Actor == null)
			return;

		DalamudServices.Framework.RunOnFrameworkThread(() =>
		{
			this.Log.Information($"Apply... Redraw: {redraw}");

			if (redraw)
			{
				this.Actor->GameObject.DisableDraw();
				this.Actor->GameObject.EnableDraw();
			}
			else
			{
				Customize* custom = &this.Actor->DrawData.Customize;
				bool result = ((Human*)this.Actor)->UpdateDrawData((byte*)custom, true);
				if (!result)
				{
					this.Log.Error("Failed to update actor customize");
				}
			}
		});
	}
}
