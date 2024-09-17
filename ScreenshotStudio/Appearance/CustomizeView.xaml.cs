namespace ScreenshotStudio.Appearance;

using DependencyPropertyGenerator;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ScreenshotStudio.Panels;

[DependencyProperty<CharacterPanelBase>("Panel")]
public partial class CustomizeView : View
{
	private CharaMakeType? makeType;
	private bool linkEyeColors = false;

	public unsafe Character* Target => this.Services.Target.Target;

	[AutoNotify]
	public bool HasValidTarget => this.Panel?.HasValidTarget == true;

	[AutoNotify]
	public int TargetObjectIndex => this.Panel?.TargetObjectIndex ?? -1;

	public DataSheet<Race>? Races => this.Services.GameData.GetSheet<Race>();
	public DataSheet<Tribe>? Tribes => this.Services.GameData.GetSheet<Tribe>();

	public IEnumerable<Race?>? AvailableRaces => this.Services.GameData.GetSheet<Race>()?.GetFrom(1);
	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.Target);

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

				DataSheet<CharaMakeType>? charaMakeTypeSheet = this.Services.GameData.GetSheet<CharaMakeType>();
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
		get => this.Races?.GetRow((int)this.GetCustomizeValue(CustomizeIndex.Race));
		set
		{
			if (value == null || value.Tribes.Count <= 0)
				return;

			int tribeIndex = 0;
			Tribe? tribe = value.Tribes[tribeIndex];
			if (tribe == null)
				return;

			Threads.RunOnFrameworkThread(() =>
			{
				this.SetCustomizeValue(CustomizeIndex.Race, (byte)value.RowId, false);
				this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)tribe.RowId, false);

				if (!value.Genders.Contains(this.Gender))
					this.SetCustomizeValue(CustomizeIndex.Gender, (byte)value.Genders[0], false);

				if (this.Tribe?.ModelTypes.Contains(this.ModelType) == false)
					this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)ModelTypes.Normal, false);

				this.UpdateCustomize(true);

				this.Dispatcher.Invoke(() =>
				{
					this.TribeSelector.SelectedIndex = tribeIndex;
				});
			});
		}
	}

	[AutoNotify]
	public Tribe? Tribe
	{
		get => this.Tribes?.GetRow((int)this.GetCustomizeValue(CustomizeIndex.Tribe));
		set
		{
			if (value == null)
				return;

			Threads.RunOnFrameworkThread(() =>
			{
				this.SetCustomizeValue(CustomizeIndex.Tribe, (byte)value.RowId, false);

				if (!value.ModelTypes.Contains(this.ModelType))
					this.SetCustomizeValue(CustomizeIndex.ModelType, (byte)ModelTypes.Normal, false);

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
	public CharaMakeType.FacialFeatureOptions? FacialFeatureOptions => this.MakeType?.GetFacialFeatures(this.Face);

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

		this.Log.Error("Attempt to change customize value");

		Threads.RunOnFrameworkThread(() =>
		{
			////this.Target->SetCustomizeValue(option, value, CharacterExtensions.UpdateSource.Interface, apply);
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

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (!this.HasValidTarget)
			return;

		Task.Run(() => this.Services.CharacterAppearance.Restore(this.TargetObjectIndex));
	}
}