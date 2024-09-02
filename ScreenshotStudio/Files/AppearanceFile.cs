namespace ScreenshotStudio.Files;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Newtonsoft.Json;
using ScreenshotStudio.Appearance;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Mvm.Commands;
using ScreenshotStudio.Tags;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomizeFacialFeatures = FFXIVClientStructs.FFXIV.Client.Game.Character.CustomizeDataExtensions.FacialFeatures;

public class AppearanceFileTypeInfo : JsonFileTypeInfoBase<AppearanceFile>
{
	public override string Extension => ".chara";
	public override string TypeName => "Appearance File";
}

[Serializable]
public class AppearanceFile : FileBase, ICharacterAppearance
{
	public AppearanceFile()
	{
		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new RevertTargetAppearanceCommand();
	}

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

	[JsonIgnore] public ICommand ApplyCommand { get; init; }
	[JsonIgnore] public ICommand RevertCommand { get; init; }

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

	public byte Glasses { get; set; }
	public string? Name { get; }

	public override void GetAutoTags(TagCollection tags)
	{
		base.GetAutoTags(tags);

		Race? race = this.Race != null ? GameDataService.GetRow<Race>((byte)this.Race) : null;
		Tribe? tribe = this.Tribe != null ? GameDataService.GetRow<Tribe>((byte)this.Tribe) : null;

		tags.Add(race?.ToTags());
		tags.Add(this.Gender?.ToTags());
		tags.Add(tribe?.ToTags());
	}

	public Task Apply(int objectTableIndex)
	{
		throw new NotImplementedException();
	}

	public struct ItemSave
	{
		public ushort ModelBase { get; set; }
		public byte ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public static implicit operator EquipmentModelId(ItemSave save) => new()
		{
			Id = save.ModelBase,
			Variant = save.ModelVariant,
			Stain0 = save.DyeId,
			Stain1 = save.DyeId2,
		};

		public static implicit operator ItemSave(EquipmentModelId modelId) => new()
		{
			ModelBase = modelId.Id,
			ModelVariant = modelId.Variant,
			DyeId = modelId.Stain0,
			DyeId2 = modelId.Stain1,
		};
	}

	public struct WeaponSave
	{
		public Vector3 Color { get; set; }
		public Vector3 Scale { get; set; }
		public ushort ModelSet { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public byte DyeId { get; set; }
		public byte DyeId2 { get; set; }

		public static implicit operator WeaponModelId(WeaponSave save) => new()
		{
			Id = save.ModelSet,
			Variant = save.ModelVariant,
			Type = save.ModelBase,
			Stain0 = save.DyeId,
			Stain1 = save.DyeId2,
		};

		public static implicit operator WeaponSave(WeaponModelId modelId) => new()
		{
			ModelSet = modelId.Id,
			ModelVariant = modelId.Variant,
			ModelBase = modelId.Type,
			DyeId = modelId.Stain0,
			DyeId2 = modelId.Stain1,
		};
	}
}