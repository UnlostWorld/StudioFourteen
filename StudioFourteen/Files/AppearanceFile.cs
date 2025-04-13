// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Files;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using StudioFourteen.Appearance;
using StudioFourteen.DragAndDrop;
using StudioFourteen.GameData;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomizeFacialFeatures = FFXIVClientStructs.FFXIV.Client.Game.Character.CustomizeDataExtensions.FacialFeatures;

public class AppearanceFileTypeInfo : JsonFileTypeInfoBase<AppearanceFile>
{
	public override string Extension => ".chara";
	public override string TypeName => "Appearance File";
	public override object? Icon => Resources.Find("ICON_Library_Entry_Appearance");
}

[Serializable]
public class AppearanceFile : FileBase, ICharacterAppearance
{
	public enum Races : byte
	{
		Hyur = 1,
		Elezen = 2,
		Lalafel = 3,
		Miqote = 4,
		Roegadyn = 5,
		AuRa = 6,
		Hrothgar = 7,
		Viera = 8,
	}

	public enum Tribes : byte
	{
		Midlander = 1,
		Highlander = 2,
		Wildwood = 3,
		Duskwight = 4,
		Plainsfolk = 5,
		Dunesfolk = 6,
		SeekerOfTheSun = 7,
		KeeperOfTheMoon = 8,
		SeaWolf = 9,
		Hellsguard = 10,
		Raen = 11,
		Xaela = 12,
		Helions = 13,
		TheLost = 14,
		Rava = 15,
		Veena = 16,
	}

	public enum BodyTypes : byte
	{
		Normal = 1,
		Old = 3,
		Young = 4,
	}

	public uint? ModelType { get; set; } = 0;
	public Races? Race { get; set; }
	public Genders? Gender { get; set; }
	public BodyTypes? Age { get; set; }
	public Tribes? Tribe { get; set; }
	public byte? Height { get; set; }
	public byte? Head { get; set; }
	public byte? Hair { get; set; }
	public bool? EnableHighlights { get; set; }
	public byte? Skintone { get; set; }
	public byte? REyeColor { get; set; }
	public byte? HairTone { get; set; }
	public byte? Highlights { get; set; }
	public CustomizeFacialFeatures? FacialFeatures { get; set; }
	public byte? LimbalEyes { get; set; }
	public byte? Eyebrows { get; set; }
	public byte? LEyeColor { get; set; }
	public byte? Eyes { get; set; }
	public byte? Nose { get; set; }
	public byte? Jaw { get; set; }
	public byte? Mouth { get; set; }
	public byte? LipsToneFurPattern { get; set; }
	public byte? EarMuscleTailSize { get; set; }
	public byte? TailEarsType { get; set; }
	public byte? Bust { get; set; }
	public byte? FacePaint { get; set; }
	public byte? FacePaintColor { get; set; }
	public WeaponSave? MainHand { get; set; }
	public WeaponSave? OffHand { get; set; }
	public ItemSave? HeadGear { get; set; }
	public ItemSave? Body { get; set; }
	public ItemSave? Hands { get; set; }
	public ItemSave? Legs { get; set; }
	public ItemSave? Feet { get; set; }
	public ItemSave? Ears { get; set; }
	public ItemSave? Neck { get; set; }
	public ItemSave? Wrists { get; set; }
	public ItemSave? LeftRing { get; set; }
	public ItemSave? RightRing { get; set; }

	// extended appearance
	public Vector3? SkinColor { get; set; }
	public Vector3? SkinGloss { get; set; }
	public Vector3? LeftEyeColor { get; set; }
	public Vector3? RightEyeColor { get; set; }
	public Vector3? LimbalRingColor { get; set; }
	public Vector3? HairColor { get; set; }
	public Vector3? HairGloss { get; set; }
	public Vector3? HairHighlight { get; set; }
	public Vector4? MouthColor { get; set; }
	public Vector3? BustScale { get; set; }
	public float? Transparency { get; set; }
	public float? MuscleTone { get; set; }
	public float? HeightMultiplier { get; set; }

	public GlassesSave Glasses { get; set; }
	public string? Name { get; }

	public override void GetAutoTags(TagCollection tags)
	{
		base.GetAutoTags(tags);

		tags.Add("Appearance").WithAlias("Character");

		Race? race = this.Race != null ? ServiceManager.Instance.GameData.GetRow<Race>((byte)this.Race) : null;
		Tribe? tribe = this.Tribe != null ? ServiceManager.Instance.GameData.GetRow<Tribe>((byte)this.Tribe) : null;

		tags.Add(race?.ToTags());
		tags.Add(this.Gender?.ToTags());
		tags.Add(tribe?.ToTags());
	}

	public IDragSceneInstance CreateSceneInstance() => new CharacterAppearanceDragSceneInstance(this);

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task<int> Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	public Task<bool> CanSpawn() => Task.FromResult(ServiceManager.Instance.CharacterLifecycle.CanSpawn);

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await this.Apply(objectTableIndex, CharacterExtensions.UpdateSource.Library);
	}

	public async Task Apply(int objectTableIndex, CharacterExtensions.UpdateSource source)
	{
		await TickService.GameTick();

		CharacterAppearanceService appearanceService = ServiceManager.Instance.CharacterAppearance;
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Race, (byte)(this.Race ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Gender, (byte)(this.Gender ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.ModelType, (byte)(this.ModelType ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Tribe, (byte)(this.Tribe ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Height, (byte)(this.Height ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.FaceType, (byte)(this.Head ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.HairStyle, (byte)(this.Hair ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.HasHighlights, (byte)(this.EnableHighlights == true ? 1 : 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.SkinColor, (byte)(this.Skintone ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.EyeColor, (byte)(this.LEyeColor ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.HairColor, (byte)(this.HairTone ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.HairColor2, (byte)(this.Highlights ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.FaceFeatures, (byte)(this.FacialFeatures ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.FaceFeaturesColor, (byte)(this.LimbalEyes ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Eyebrows, (byte)(this.Eyebrows ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.EyeColor2, (byte)(this.REyeColor ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.EyeShape, (byte)(this.Eyes ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.NoseShape, (byte)(this.Nose ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.JawShape, (byte)(this.Jaw ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.LipStyle, (byte)(this.Mouth ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.LipColor, (byte)(this.LipsToneFurPattern ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.RaceFeatureSize, (byte)(this.EarMuscleTailSize ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.RaceFeatureType, (byte)(this.TailEarsType ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.BustSize, (byte)(this.Bust ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.Facepaint, (byte)(this.FacePaint ?? 0), source);
		appearanceService.SetCustomizeValue(objectTableIndex, CustomizeIndex.FacepaintColor, (byte)(this.FacePaintColor ?? 0), source);

		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Head, this.HeadGear, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Body, this.Body, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Hands, this.Hands, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Legs, this.Legs, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Feet, this.Feet, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Ears, this.Ears, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Neck, this.Neck, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.Wrists, this.Wrists, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.LFinger, this.LeftRing, source);
		appearanceService.SetEquipment(objectTableIndex, DrawDataContainer.EquipmentSlot.RFinger, this.RightRing, source);

		appearanceService.SetWeapon(objectTableIndex, DrawDataContainer.WeaponSlot.MainHand, this.MainHand, source);
		appearanceService.SetWeapon(objectTableIndex, DrawDataContainer.WeaponSlot.OffHand, this.OffHand, source);
	}

	public async Task Read(int objectTableIndex)
	{
		await TickService.GameTick();

		unsafe
		{
			Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(objectTableIndex);
			this.ModelType = (uint)pCharacter->ModelContainer.ModelCharaId;
			this.Race = (Races)pCharacter->GetCustomizeValue(CustomizeIndex.Race);
			this.Gender = (Genders)pCharacter->GetCustomizeValue(CustomizeIndex.Gender);
			this.Age = (BodyTypes)pCharacter->GetCustomizeValue(CustomizeIndex.ModelType);
			this.Tribe = (Tribes)pCharacter->GetCustomizeValue(CustomizeIndex.Tribe);
			this.Height = pCharacter->GetCustomizeValue(CustomizeIndex.Height);
			this.Head = pCharacter->GetCustomizeValue(CustomizeIndex.FaceType);
			this.Hair = pCharacter->GetCustomizeValue(CustomizeIndex.HairStyle);
			this.EnableHighlights = pCharacter->GetCustomizeValue(CustomizeIndex.HasHighlights) != 0;
			this.Skintone = pCharacter->GetCustomizeValue(CustomizeIndex.SkinColor);
			this.REyeColor = pCharacter->GetCustomizeValue(CustomizeIndex.EyeColor);
			this.HairTone = pCharacter->GetCustomizeValue(CustomizeIndex.HairColor);
			this.Highlights = pCharacter->GetCustomizeValue(CustomizeIndex.HairColor2);
			this.FacialFeatures = (CustomizeFacialFeatures)pCharacter->GetCustomizeValue(CustomizeIndex.FaceFeatures);
			this.LimbalEyes = pCharacter->GetCustomizeValue(CustomizeIndex.FaceFeaturesColor);
			this.Eyebrows = pCharacter->GetCustomizeValue(CustomizeIndex.Eyebrows);
			this.LEyeColor = pCharacter->GetCustomizeValue(CustomizeIndex.EyeColor2);
			this.Eyes = pCharacter->GetCustomizeValue(CustomizeIndex.EyeShape);
			this.Nose = pCharacter->GetCustomizeValue(CustomizeIndex.NoseShape);
			this.Jaw = pCharacter->GetCustomizeValue(CustomizeIndex.JawShape);
			this.Mouth = pCharacter->GetCustomizeValue(CustomizeIndex.LipStyle);
			this.LipsToneFurPattern = pCharacter->GetCustomizeValue(CustomizeIndex.LipColor);
			this.EarMuscleTailSize = pCharacter->GetCustomizeValue(CustomizeIndex.RaceFeatureSize);
			this.TailEarsType = pCharacter->GetCustomizeValue(CustomizeIndex.RaceFeatureType);
			this.Bust = pCharacter->GetCustomizeValue(CustomizeIndex.BustSize);
			this.FacePaint = pCharacter->GetCustomizeValue(CustomizeIndex.Facepaint);
			this.FacePaintColor = pCharacter->GetCustomizeValue(CustomizeIndex.FacepaintColor);

			////this.HeightMultiplier = pCharacter->Height;

			DrawObjectData mainHand = pCharacter->DrawData.Weapon(DrawDataContainer.WeaponSlot.MainHand);
			this.MainHand = WeaponSave.FromDrawData(mainHand);

			DrawObjectData offHand = pCharacter->DrawData.Weapon(DrawDataContainer.WeaponSlot.OffHand);
			this.OffHand = WeaponSave.FromDrawData(offHand);

			this.HeadGear = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Head);
			this.Body = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Body);
			this.Hands = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Hands);
			this.Legs = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Legs);
			this.Feet = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Feet);
			this.Ears = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Ears);
			this.Neck = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Neck);
			this.Wrists = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.Wrists);
			this.LeftRing = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.LFinger);
			this.RightRing = pCharacter->DrawData.Equipment(DrawDataContainer.EquipmentSlot.RFinger);
		}
	}

	public struct ItemSave
	{
		public ushort ModelBase { get; set; }
		public byte ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public static implicit operator EquipmentModelId(ItemSave? save) => new()
		{
			Id = save?.ModelBase ?? 0,
			Variant = save?.ModelVariant ?? 0,
			Stain0 = save?.DyeId ?? 0,
			Stain1 = save?.DyeId2 ?? 0,
		};

		public static implicit operator ItemSave?(EquipmentModelId modelId)
		{
			if (modelId.Value == 0)
				return null;

			return new()
			{
				ModelBase = modelId.Id,
				ModelVariant = modelId.Variant,
				DyeId = modelId.Stain0,
				DyeId2 = modelId.Stain1,
			};
		}
	}

	public struct WeaponSave
	{
		public Vector3? Color { get; set; }
		public Vector3? Scale { get; set; }
		public ushort ModelSet { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public static implicit operator WeaponModelId(WeaponSave? save) => new()
		{
			Id = save?.ModelSet ?? 0,
			Variant = save?.ModelVariant ?? 0,
			Type = save?.ModelBase ?? 0,
			Stain0 = save?.DyeId ?? 0,
			Stain1 = save?.DyeId2 ?? 0,
		};

		public static unsafe WeaponSave? FromDrawData(DrawObjectData drawData)
		{
			if (drawData.ModelId.Value == 0)
				return null;

			return new()
			{
				ModelSet = drawData.ModelId.Id,
				ModelVariant = drawData.ModelId.Variant,
				ModelBase = drawData.ModelId.Type,
				DyeId = drawData.ModelId.Stain0,
				DyeId2 = drawData.ModelId.Stain1,
				////Color = color,
				Scale = drawData.DrawObject->Scale,
			};
		}
	}

	public struct GlassesSave
	{
		public ushort GlassesId { get; set; }

		public static implicit operator byte(GlassesSave save) => (byte)save.GlassesId;

		public static implicit operator GlassesSave(byte save) => new()
		{
			GlassesId = save,
		};
	}
}