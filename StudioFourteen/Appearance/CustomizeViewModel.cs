namespace StudioFourteen.Appearance;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Utilities;
using System;

using static Lumina.Excel.Sheets.CharaMakeType;

public partial class CustomizeViewModel : AutoViewModel
{
	private bool linkEyeColors = false;

	public unsafe Character* Target => this.Services.Target.Target;

	[AutoNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AutoNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public ExcelSheetLibrarySource<RaceLibraryEntry>? RaceSource => this.Services.GameData.GetLibrarySource<RaceLibraryEntry>();
	public ExcelSheetLibrarySource<TribeLibraryEntry>? Tribes => this.Services.GameData.GetLibrarySource<TribeLibraryEntry>();

	[AutoNotify]
	public RaceLibraryEntry? Race
	{
		get => this.RaceSource?.GetRow(this.GetCustomizeValue(CustomizeIndex.Race));
		set
		{
			this.Log.Information($"Change Race {value}");

			if (value == null)
				return;

			RaceLibraryEntry? oldRace = this.Race;

			if (value.RowId == oldRace?.RowId)
				return;

			// Get new tribe for the new race
			TribeLibraryEntry newTribe;
			{
				int tribeIndex = 0;
				TribeLibraryEntry? oldTribe = this.Tribe;
				if (oldRace != null && oldTribe != null)
				{
					tribeIndex = oldRace.GetTribeIndex(oldTribe);
					if (tribeIndex == -1)
					{
						tribeIndex = 0;
					}
				}

				newTribe = value.Tribes[tribeIndex];
			}

			// Get new model type for the new race
			ModelTypes newModelType = this.ModelType;
			{
				if (!newTribe.ModelTypes.Contains(newModelType))
				{
					newModelType = ModelTypes.Normal;
				}
			}

			Threads.RunOnFrameworkThread(() =>
			{
				this.SetCustomizeValue(CustomizeIndex.Race, (byte)value.RowId, false);
				this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)newTribe.RowId, false);
				this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)newModelType, false);
				this.UpdateCustomize(true);
			});
		}
	}

	[AutoNotify]
	public TribeLibraryEntry? Tribe
	{
		get => this.Tribes?.GetRow(this.GetCustomizeValue(CustomizeIndex.Tribe));
		set
		{
			if (value == null)
				return;

			// Get new model type for the new tribe
			ModelTypes newModelType = this.ModelType;
			{
				if (!value.ModelTypes.Contains(newModelType))
				{
					newModelType = ModelTypes.Normal;
				}
			}

			Threads.RunOnFrameworkThread(() =>
			{
				this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)value.RowId, false);
				this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)newModelType, false);
				this.UpdateCustomize(true);
			});
		}
	}

	[AutoNotify]
	public Genders Gender
	{
		get => (Genders)this.GetCustomizeValue(CustomizeIndex.Gender);
		set => this.SetCustomizeValue(CustomizeIndex.Gender, (byte)value);
	}

	[AutoNotify]
	public ModelTypes ModelType
	{
		get => (ModelTypes)this.GetCustomizeValue(CustomizeIndex.ModelType);
		set => this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)value);
	}

	[AutoNotify]
	public byte CharacterHeight
	{
		get => this.GetCustomizeValue(CustomizeIndex.Height);
		set => this.SetCustomizeValue(CustomizeIndex.Height, value);
	}

	[AutoNotify]
	public byte Face
	{
		get => this.GetCustomizeValue(CustomizeIndex.FaceType);
		set => this.SetCustomizeValue(CustomizeIndex.FaceType, value);
	}

	[AutoNotify]
	public byte Hair
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairStyle);
		set => this.SetCustomizeValue(CustomizeIndex.HairStyle, value);
	}

	[AutoNotify]
	public bool EnableHighlights
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor2) != 0;
		set => this.SetCustomizeValue(CustomizeIndex.HairColor2, value ? (byte)128 : (byte)0);
	}

	[AutoNotify]
	public byte SkinTone
	{
		get => this.GetCustomizeValue(CustomizeIndex.SkinColor);
		set => this.SetCustomizeValue(CustomizeIndex.SkinColor, value);
	}

	[AutoNotify]
	public byte RightEyeColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.EyeColor);
		set => this.SetCustomizeValue(CustomizeIndex.EyeColor, value);
	}

	[AutoNotify]
	public byte HairTone
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor);
		set => this.SetCustomizeValue(CustomizeIndex.HairColor, value);
	}

	[AutoNotify]
	public byte Highlights
	{
		get => this.GetCustomizeValue(CustomizeIndex.HairColor2);
		set => this.SetCustomizeValue(CustomizeIndex.HairColor2, value);
	}

	[AutoNotify]
	public CustomizeDataExtensions.FacialFeatures FacialFeature
	{
		get => (CustomizeDataExtensions.FacialFeatures)this.GetCustomizeValue(CustomizeIndex.FaceFeatures);
		set => this.SetCustomizeValue(CustomizeIndex.FaceFeatures, (byte)value);
	}

	[AutoNotify]
	public FacialFeatureOptionStruct FacialFeatureOptions
	{
		get
		{
			uint faceId = this.Face;

			// I'm not sure why Hrothgar's face Id's are off by 4. =/
			if (this.Race?.RowId == (uint)RaceRows.Hrothgar)
				faceId -= 4;

			////return this.MakeType?.GetFacialFeatures(faceId);
			return default;
		}
	}

	[AutoNotify]
	public byte FacialFeatureColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.FaceFeaturesColor);
		set => this.SetCustomizeValue(CustomizeIndex.FaceFeaturesColor, value);
	}

	[AutoNotify]
	public byte Eyebrows
	{
		get => this.GetCustomizeValue(CustomizeIndex.Eyebrows);
		set => this.SetCustomizeValue(CustomizeIndex.Eyebrows, value);
	}

	[AutoNotify]
	public byte LeftEyeColor
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
	public byte Eyes
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
	public byte Nose
	{
		get => this.GetCustomizeValue(CustomizeIndex.NoseShape);
		set => this.SetCustomizeValue(CustomizeIndex.NoseShape, value);
	}

	[AutoNotify]
	public byte Jaw
	{
		get => this.GetCustomizeValue(CustomizeIndex.JawShape);
		set => this.SetCustomizeValue(CustomizeIndex.JawShape, value);
	}

	[AutoNotify]
	public byte MouthId
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
	public byte LipsToneFurPattern
	{
		get => this.GetCustomizeValue(CustomizeIndex.LipColor);
		set => this.SetCustomizeValue(CustomizeIndex.LipColor, value);
	}

	[AutoNotify]
	public byte EarMuscleTailSize
	{
		get => this.GetCustomizeValue(CustomizeIndex.RaceFeatureSize);
		set => this.SetCustomizeValue(CustomizeIndex.RaceFeatureSize, value);
	}

	[AutoNotify]
	public byte TailEarsType
	{
		get => this.GetCustomizeValue(CustomizeIndex.RaceFeatureType);
		set => this.SetCustomizeValue(CustomizeIndex.RaceFeatureType, value);
	}

	[AutoNotify]
	public byte Bust
	{
		get => this.GetCustomizeValue(CustomizeIndex.BustSize);
		set => this.SetCustomizeValue(CustomizeIndex.BustSize, value);
	}

	[AutoNotify]
	public byte FacePaintId
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
	public byte FacePaintColor
	{
		get => this.GetCustomizeValue(CustomizeIndex.FacepaintColor);
		set
		{
			this.SetCustomizeValue(CustomizeIndex.FacepaintColor, value);
		}
	}

	public unsafe byte GetCustomizeValue(CustomizeIndex option)
	{
		if (!this.HasValidTarget)
			return 0;

		return this.Target->GetCustomizeValue(option);
	}

	public unsafe void SetCustomizeValue(CustomizeIndex option, byte value, bool apply = true)
	{
		if (!this.HasValidTarget)
			return;

		Threads.RunOnFrameworkThread(() =>
		{
			this.Target->SetCustomizeValue(option, value, CharacterExtensions.UpdateSource.Interface, apply);
		});
	}

	public unsafe void UpdateCustomize(bool redraw)
	{
		if (!this.HasValidTarget)
			return;

		Threads.RunOnFrameworkThread(() =>
		{
			this.Target->UpdateCustomize(redraw, CharacterExtensions.UpdateSource.Interface);
		});
	}

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	private CharaMakeStructStruct? GetMakeTypeEntry(CustomizeIndex customizeIndex)
	{
		CharaMakeType? makeType = this.GetMakeType();

		if (makeType == null)
			return null;

		foreach(CharaMakeStructStruct makeTypeEntry in makeType.Value.CharaMakeStruct)
		{
			if (makeTypeEntry.Customize == (uint)customizeIndex)
			{
				return makeTypeEntry;
			}
		}

		return null;
	}

	private CharaMakeType? GetMakeType()
	{
		if (this.Tribe == null)
			return null;

		ExcelSheet<CharaMakeType>? charaMakeTypeSheet = this.Services.GameData.GetSheet<CharaMakeType>();
		if (charaMakeTypeSheet == null)
			return null;

		foreach (CharaMakeType makeType in charaMakeTypeSheet)
		{
			if (!makeType.Tribe.IsRow(this.Tribe.RowId) || makeType.Gender != (sbyte)this.Gender)
				continue;

			return makeType;
		}

		return null;
	}
}