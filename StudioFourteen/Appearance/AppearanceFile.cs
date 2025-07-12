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

namespace StudioFourteen.Appearance;

using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.DragAndDrop;
using StudioFourteen.Files;
using StudioFourteen.GameData;
using StudioFourteen.Scene.GameObjects;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;
using StudioFourteen.Tags;

using Character = StudioFourteen.Scene.GameObjects.Characters.Character;
using CustomizeFacialFeatures = FFXIVClientStructs.FFXIV.Client.Game.Character.CustomizeDataExtensions.FacialFeatures;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

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

	public override Task Execute()
	{
		if (ServiceManager.Instance.Selection.Current is Character character)
		{
			return this.Apply(character, UpdateSource.Interface);
		}

		return Task.CompletedTask;
	}

	public Task Create()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this, UpdateSource.Interface);
	}

	public async Task Apply(Character character, UpdateSource source)
	{
		await TickService.GameTick();

		character.SetCustomizeValue(CustomizeIndex.Race, (byte)(this.Race ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.Gender, (byte)(this.Gender ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.ModelType, (byte)(this.ModelType ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.Tribe, (byte)(this.Tribe ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.Height, (byte)(this.Height ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.FaceType, (byte)(this.Head ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.HairStyle, (byte)(this.Hair ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.HasHighlights, (byte)(this.EnableHighlights == true ? 1 : 0), source);
		character.SetCustomizeValue(CustomizeIndex.SkinColor, (byte)(this.Skintone ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.EyeColor, (byte)(this.LEyeColor ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.HairColor, (byte)(this.HairTone ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.HairColor2, (byte)(this.Highlights ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.FaceFeatures, (byte)(this.FacialFeatures ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.FaceFeaturesColor, (byte)(this.LimbalEyes ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.Eyebrows, (byte)(this.Eyebrows ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.EyeColor2, (byte)(this.REyeColor ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.EyeShape, (byte)(this.Eyes ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.NoseShape, (byte)(this.Nose ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.JawShape, (byte)(this.Jaw ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.LipStyle, (byte)(this.Mouth ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.LipColor, (byte)(this.LipsToneFurPattern ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.RaceFeatureSize, (byte)(this.EarMuscleTailSize ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.RaceFeatureType, (byte)(this.TailEarsType ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.BustSize, (byte)(this.Bust ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.Facepaint, (byte)(this.FacePaint ?? 0), source);
		character.SetCustomizeValue(CustomizeIndex.FacepaintColor, (byte)(this.FacePaintColor ?? 0), source);

		character.SetEquipment(DrawDataContainer.EquipmentSlot.Head, this.HeadGear, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Body, this.Body, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Hands, this.Hands, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Legs, this.Legs, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Feet, this.Feet, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Ears, this.Ears, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Neck, this.Neck, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.Wrists, this.Wrists, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.LFinger, this.LeftRing, source);
		character.SetEquipment(DrawDataContainer.EquipmentSlot.RFinger, this.RightRing, source);

		character.SetWeapon(DrawDataContainer.WeaponSlot.MainHand, this.MainHand, source);
		character.SetWeapon(DrawDataContainer.WeaponSlot.OffHand, this.OffHand, source);
	}

	public async Task ReadAsync(Character character)
	{
		await TickService.GameTick();
		this.Read(character);
	}

	public void Read(Character character)
	{
		TickService.VerifyGameTickThread();

		this.ModelType = character.GetModelCharaId();
		this.Race = (Races)character.GetCustomizeValue(CustomizeIndex.Race);
		this.Gender = (Genders)character.GetCustomizeValue(CustomizeIndex.Gender);
		this.Age = (BodyTypes)character.GetCustomizeValue(CustomizeIndex.ModelType);
		this.Tribe = (Tribes)character.GetCustomizeValue(CustomizeIndex.Tribe);
		this.Height = character.GetCustomizeValue(CustomizeIndex.Height);
		this.Head = character.GetCustomizeValue(CustomizeIndex.FaceType);
		this.Hair = character.GetCustomizeValue(CustomizeIndex.HairStyle);
		this.EnableHighlights = character.GetCustomizeValue(CustomizeIndex.HasHighlights) != 0;
		this.Skintone = character.GetCustomizeValue(CustomizeIndex.SkinColor);
		this.REyeColor = character.GetCustomizeValue(CustomizeIndex.EyeColor);
		this.HairTone = character.GetCustomizeValue(CustomizeIndex.HairColor);
		this.Highlights = character.GetCustomizeValue(CustomizeIndex.HairColor2);
		this.FacialFeatures = (CustomizeFacialFeatures)character.GetCustomizeValue(CustomizeIndex.FaceFeatures);
		this.LimbalEyes = character.GetCustomizeValue(CustomizeIndex.FaceFeaturesColor);
		this.Eyebrows = character.GetCustomizeValue(CustomizeIndex.Eyebrows);
		this.LEyeColor = character.GetCustomizeValue(CustomizeIndex.EyeColor2);
		this.Eyes = character.GetCustomizeValue(CustomizeIndex.EyeShape);
		this.Nose = character.GetCustomizeValue(CustomizeIndex.NoseShape);
		this.Jaw = character.GetCustomizeValue(CustomizeIndex.JawShape);
		this.Mouth = character.GetCustomizeValue(CustomizeIndex.LipStyle);
		this.LipsToneFurPattern = character.GetCustomizeValue(CustomizeIndex.LipColor);
		this.EarMuscleTailSize = character.GetCustomizeValue(CustomizeIndex.RaceFeatureSize);
		this.TailEarsType = character.GetCustomizeValue(CustomizeIndex.RaceFeatureType);
		this.Bust = character.GetCustomizeValue(CustomizeIndex.BustSize);
		this.FacePaint = character.GetCustomizeValue(CustomizeIndex.Facepaint);
		this.FacePaintColor = character.GetCustomizeValue(CustomizeIndex.FacepaintColor);

		////this.HeightMultiplier = pCharacter->Height;

		this.MainHand = character.GetWeapon(DrawDataContainer.WeaponSlot.MainHand);
		this.OffHand = character.GetWeapon(DrawDataContainer.WeaponSlot.OffHand);

		this.HeadGear = character.GetEquipment(DrawDataContainer.EquipmentSlot.Head);
		this.Body = character.GetEquipment(DrawDataContainer.EquipmentSlot.Body);
		this.Hands = character.GetEquipment(DrawDataContainer.EquipmentSlot.Hands);
		this.Legs = character.GetEquipment(DrawDataContainer.EquipmentSlot.Legs);
		this.Feet = character.GetEquipment(DrawDataContainer.EquipmentSlot.Feet);
		this.Ears = character.GetEquipment(DrawDataContainer.EquipmentSlot.Ears);
		this.Neck = character.GetEquipment(DrawDataContainer.EquipmentSlot.Neck);
		this.Wrists = character.GetEquipment(DrawDataContainer.EquipmentSlot.Wrists);
		this.LeftRing = character.GetEquipment(DrawDataContainer.EquipmentSlot.LFinger);
		this.RightRing = character.GetEquipment(DrawDataContainer.EquipmentSlot.RFinger);
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

		public static unsafe implicit operator WeaponSave?(DrawObjectData drawData)
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