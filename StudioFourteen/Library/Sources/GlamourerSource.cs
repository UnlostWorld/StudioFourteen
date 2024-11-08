namespace StudioFourteen.Library.Sources;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Mvm.Commands;
using StudioFourteen.Plugin;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using CustomizeFacialFeatures = FFXIVClientStructs.FFXIV.Client.Game.Character.CustomizeDataExtensions.FacialFeatures;

public class GlamourerSource : SourceBase
{
	public override string Name => "Glamourer";

	protected override string GetInternalId() => "Glamourer";

	protected override void Scan()
	{
		DirectoryInfo? configsDir = DalamudServices.PluginInterface?.ConfigDirectory;
		configsDir = configsDir?.Parent;

		DirectoryInfo dir = new($"{configsDir?.FullName}/Glamourer/designs/");
		if (!dir.Exists)
			return;

		FileInfo[] files = dir.GetFiles("*.json");
		foreach (FileInfo file in files)
		{
			try
			{
				string json = File.ReadAllText(file.FullName);
				GlamourerDesign? design = Serialization.Serializer.Deserialize<GlamourerDesign>(json);

				if (design == null)
					throw new Exception();

				this.Add(new GlamourerEntry(this, design));
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Error reading glamourer file: {file.Name}");
			}
		}
	}
}

public class GlamourerEntry
	: LibraryEntryBase, ICharacterAppearance
{
	private readonly GlamourerDesign design;

	public GlamourerEntry(GlamourerSource source, GlamourerDesign design)
		: base(source)
	{
		this.design = design;
		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new RevertTargetAppearanceCommand();
		this.SpawnCommand = new TargetCommand(this.Spawn, true);

		this.design.GetAutoTags(this.Tags);
	}

	public override string Name => this.design.Name ?? "Unknown";
	public ICommand ApplyCommand { get; init; }
	public ICommand RevertCommand { get; init; }
	public ICommand SpawnCommand { get; init; }

	public Task Spawn(int objectTableIndex)
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		unsafe
		{
			Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);

			bool redraw = false;

			if (this.design.Customize != null)
			{
				foreach (CustomizeIndex index in Enum.GetValues<CustomizeIndex>())
				{
					byte? value = this.design.Customize.GetValue(index);
					if (value == null)
						continue;

					character->SetCustomizeValue(CustomizeIndex.Race, (byte)value, CharacterExtensions.UpdateSource.Library, false);
				}

				character->UpdateCustomize(redraw, CharacterExtensions.UpdateSource.Library);
			}

			if (this.design.Equipment != null)
			{
				foreach (EquipmentSlot index in Enum.GetValues<EquipmentSlot>())
				{
					EquipmentModelId? id = this.design.Equipment.GetValue(index);
					if (id == null)
						continue;

					character->UpdateEquipment(index, (EquipmentModelId)id, CharacterExtensions.UpdateSource.Library);
				}

				foreach (WeaponSlot index in Enum.GetValues<WeaponSlot>())
				{
					WeaponModelId? id = this.design.Equipment.GetValue(index);
					if (id == null)
						continue;

					character->UpdateWeapon(index, (WeaponModelId)id, CharacterExtensions.UpdateSource.Library);
				}
			}

			character->SetDisplayName(this.Name);
		}
	}

	protected override string GetInternalId() => this.design.Identifier ?? "Unknown";
}

public class GlamourerDesign
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Identifier { get; set; }
	public GlamourerEquipment? Equipment { get; set; }
	public GlamourerCustomize? Customize { get; set; }
	public GlamourerParameters? Parameters { get; set; }

	public void GetAutoTags(TagCollection tags)
	{
		Race.RaceRows? raceRow = (Race.RaceRows?)this.Customize?.Race?.Value;
		Race? race = raceRow != null ? GameDataService.GetRow<Race>((byte)raceRow) : null;
		tags.Add(race?.ToTags());

		Tribe.TribeRows? tribeRow = (Tribe.TribeRows?)this.Customize?.Clan?.Value;
		Tribe? tribe = tribeRow != null ? GameDataService.GetRow<Tribe>((byte)tribeRow) : null;
		tags.Add(tribe?.ToTags());

		Genders? gender = (Genders?)this.Customize?.Gender?.Value;
		tags.Add(gender?.ToTags());
	}

	public class GlamourerEquipment
	{
		public GlamourerItem? MainHand { get; set; }
		public GlamourerItem? OffHand { get; set; }
		public GlamourerItem? Head { get; set; }
		public GlamourerItem? Body { get; set; }
		public GlamourerItem? Hands { get; set; }
		public GlamourerItem? Legs { get; set; }
		public GlamourerItem? Feet { get; set; }
		public GlamourerItem? Ears { get; set; }
		public GlamourerItem? Neck { get; set; }
		public GlamourerItem? Wrists { get; set; }
		public GlamourerItem? RFinger { get; set; }
		public GlamourerItem? LFinger { get; set; }
		public GlamourerShow? Hat { get; set; }
		public GlamourerVisor? Visor { get; set; }
		public GlamourerShow? Weapon { get; set; }

		public EquipmentModelId? GetValue(EquipmentSlot index)
		{
			switch (index)
			{
				case EquipmentSlot.Head: return this.Head?.GetModelId();
				case EquipmentSlot.Body: return this.Body?.GetModelId();
				case EquipmentSlot.Hands: return this.Hands?.GetModelId();
				case EquipmentSlot.Legs: return this.Legs?.GetModelId();
				case EquipmentSlot.Feet: return this.Feet?.GetModelId();
				case EquipmentSlot.Ears: return this.Ears?.GetModelId();
				case EquipmentSlot.Neck: return this.Neck?.GetModelId();
				case EquipmentSlot.Wrists: return this.Wrists?.GetModelId();
				case EquipmentSlot.RFinger: return this.RFinger?.GetModelId();
				case EquipmentSlot.LFinger: return this.LFinger?.GetModelId();
			}

			throw new Exception($"Unrecognized equipment slot {index}");
		}

		public WeaponModelId? GetValue(WeaponSlot index)
		{
			switch (index)
			{
				case WeaponSlot.MainHand: return this.MainHand?.GetWeaponId();
				case WeaponSlot.OffHand: return this.OffHand?.GetWeaponId();
			}

			return null;
		}

		public class GlamourerItem
		{
			public uint ItemId;
			public byte Stain;
			public byte Stain2;
			public bool Crest;
			public bool Apply;
			public bool ApplyStain;
			public bool ApplyCrest;

			public EquipmentModelId? GetModelId()
			{
				if (!this.Apply)
					return null;

				DataSheet<Item>? sheet = ServiceManager.Instance.GameData.GetSheet<Item>();
				if (sheet == null)
					return null;

				Item? item = sheet.GetRow(this.ItemId);
				if (item == null)
					return null;

				EquipmentModelId id = default;
				id.Id = item.ModelBase;
				id.Variant = (byte)item.ModelVariant;
				id.Stain0 = this.ApplyStain ? this.Stain : (byte)0;
				id.Stain1 = this.ApplyStain ? this.Stain2 : (byte)0;
				return id;
			}

			public WeaponModelId? GetWeaponId()
			{
				if (!this.Apply)
					return null;

				DataSheet<Item>? sheet = ServiceManager.Instance.GameData.GetSheet<Item>();
				if (sheet == null)
					return null;

				Item? item = sheet.GetRow(this.ItemId);
				if (item == null)
					return null;

				WeaponModelId id = default;
				id.Id = item.ModelSet;
				id.Type = item.ModelBase;
				id.Variant = item.ModelVariant;
				id.Stain0 = this.ApplyStain ? this.Stain : (byte)0;
				id.Stain1 = this.ApplyStain ? this.Stain2 : (byte)0;
				return id;
			}
		}

		public class GlamourerShow
		{
			public bool Show { get; set; }
			public bool Apply { get; set; }
		}

		public class GlamourerVisor
		{
			public bool IsToggled { get; set; }
			public bool Apply { get; set; }
		}
	}

	public class GlamourerCustomize
	{
		public GlamourerValue? Race { get; set; }
		public GlamourerValue? Gender { get; set; }
		public GlamourerValue? BodyType { get; set; }
		public GlamourerValue? Height { get; set; }
		public GlamourerValue? Clan { get; set; }
		public GlamourerValue? Face { get; set; }
		public GlamourerValue? Hairstyle { get; set; }
		public GlamourerValue? Highlights { get; set; }
		public GlamourerValue? SkinColor { get; set; }
		public GlamourerValue? EyeColorRight { get; set; }
		public GlamourerValue? HairColor { get; set; }
		public GlamourerValue? HighlightsColor { get; set; }
		public GlamourerValue? FacialFeature1 { get; set; }
		public GlamourerValue? FacialFeature2 { get; set; }
		public GlamourerValue? FacialFeature3 { get; set; }
		public GlamourerValue? FacialFeature4 { get; set; }
		public GlamourerValue? FacialFeature5 { get; set; }
		public GlamourerValue? FacialFeature6 { get; set; }
		public GlamourerValue? FacialFeature7 { get; set; }
		public GlamourerValue? LegacyTattoo { get; set; }
		public GlamourerValue? TattooColor { get; set; }
		public GlamourerValue? Eyebrows { get; set; }
		public GlamourerValue? EyeColorLeft { get; set; }
		public GlamourerValue? EyeShape { get; set; }
		public GlamourerValue? SmallIris { get; set; }
		public GlamourerValue? Nose { get; set; }
		public GlamourerValue? Jaw { get; set; }
		public GlamourerValue? Mouth { get; set; }
		public GlamourerValue? Lipstick { get; set; }
		public GlamourerValue? LipColor { get; set; }
		public GlamourerValue? MuscleMass { get; set; }
		public GlamourerValue? TailShape { get; set; }
		public GlamourerValue? BustSize { get; set; }
		public GlamourerValue? FacePaint { get; set; }
		public GlamourerValue? FacePaintReversed { get; set; }
		public GlamourerValue? FacePaintColor { get; set; }
		public GlamourerBoolValue? Wetness { get; set; }

		public byte? GetValue(CustomizeIndex index)
		{
			switch (index)
			{
				case CustomizeIndex.Race: return this.Race?.Apply == true ? this.Race.Value : null;
				case CustomizeIndex.Gender: return this.Gender?.Apply == true ? this.Gender.Value : null;
				case CustomizeIndex.Tribe: return this.Clan?.Apply == true ? this.Clan.Value : null;
				case CustomizeIndex.Height: return this.Height?.Apply == true ? this.Height.Value : null;
				case CustomizeIndex.FaceType: return this.Face?.Apply == true ? this.Face.Value : null;
				case CustomizeIndex.HairStyle: return this.Hairstyle?.Apply == true ? this.Hairstyle.Value : null;
				case CustomizeIndex.HasHighlights: return this.Highlights?.Apply == true ? this.Highlights.Value : null;
				case CustomizeIndex.SkinColor: return this.SkinColor?.Apply == true ? this.SkinColor.Value : null;
				case CustomizeIndex.EyeColor: return this.EyeColorLeft?.Apply == true ? this.EyeColorLeft.Value : null;
				case CustomizeIndex.HairColor: return this.HairColor?.Apply == true ? this.HairColor.Value : null;
				case CustomizeIndex.HairColor2: return this.HighlightsColor?.Apply == true ? this.HighlightsColor.Value : null;
				case CustomizeIndex.FaceFeaturesColor: return this.TattooColor?.Apply == true ? this.TattooColor.Value : null;
				case CustomizeIndex.Eyebrows: return this.Eyebrows?.Apply == true ? this.Eyebrows.Value : null;
				case CustomizeIndex.EyeColor2: return this.EyeColorRight?.Apply == true ? this.EyeColorRight.Value : null;
				case CustomizeIndex.EyeShape: return this.EyeShape?.Apply == true ? this.EyeShape.Value : null;
				case CustomizeIndex.NoseShape: return this.Nose?.Apply == true ? this.Nose.Value : null;
				case CustomizeIndex.JawShape: return this.Jaw?.Apply == true ? this.Jaw.Value : null;
				case CustomizeIndex.LipStyle: return this.Lipstick?.Apply == true ? this.Lipstick.Value : null;
				case CustomizeIndex.LipColor: return this.LipColor?.Apply == true ? this.LipColor.Value : null;
				case CustomizeIndex.RaceFeatureSize: return this.MuscleMass?.Apply == true ? this.MuscleMass.Value : null;
				case CustomizeIndex.RaceFeatureType: return this.TailShape?.Apply == true ? this.TailShape.Value : null;
				case CustomizeIndex.BustSize: return this.BustSize?.Apply == true ? this.BustSize.Value : null;
				case CustomizeIndex.Facepaint: return this.FacePaint?.Apply == true ? this.FacePaint.Value : null;
				case CustomizeIndex.FacepaintColor: return this.FacePaintColor?.Apply == true ? this.FacePaintColor.Value : null;

				case CustomizeIndex.FaceFeatures:
				{
					if (this.FacialFeature1?.Apply != true)
						return null;

					CustomizeFacialFeatures val = 0;
					if (this.FacialFeature1?.Value == 1)
						val |= CustomizeFacialFeatures.First;
					if (this.FacialFeature2?.Value == 1)
						val |= CustomizeFacialFeatures.Second;
					if (this.FacialFeature3?.Value == 1)
						val |= CustomizeFacialFeatures.Third;
					if (this.FacialFeature4?.Value == 1)
						val |= CustomizeFacialFeatures.Fourth;
					if (this.FacialFeature5?.Value == 1)
						val |= CustomizeFacialFeatures.Fifth;
					if (this.FacialFeature6?.Value == 1)
						val |= CustomizeFacialFeatures.Sixth;
					if (this.FacialFeature7?.Value == 1)
						val |= CustomizeFacialFeatures.Seventh;
					if (this.LegacyTattoo?.Value == 1)
						val |= CustomizeFacialFeatures.LegacyTattoo;

					return (byte)val;
				}
			}

			return null;
		}

		public class GlamourerValue
		{
			public byte Value { get; set; }
			public bool Apply { get; set; }
		}

		public class GlamourerBoolValue
		{
			public bool Value { get; set; }
			public bool Apply { get; set; }
		}
	}

	public class GlamourerParameters
	{
		public GlamourerParameter? FacePaintUvMultiplier { get; set; }
		public GlamourerParameter? FacePaintUvOffset { get; set; }
		public GlamourerPercentageParameter? MuscleTone { get; set; }
		public GlamourerColorParameter? SkinDiffuse { get; set; }
		public GlamourerColorParameter? SkinSpecular { get; set; }
		public GlamourerColorParameter? HairDiffuse { get; set; }
		public GlamourerColorParameter? HairSpecular { get; set; }
		public GlamourerColorParameter? HairHighlight { get; set; }
		public GlamourerColorParameter? LeftEye { get; set; }
		public GlamourerColorParameter? RightEye { get; set; }
		public GlamourerColorParameter? FeatureColor { get; set; }
		public GlamourerColorAParameter? LipDiffuse { get; set; }
		public GlamourerColorAParameter? DecalColor { get; set; }

		public class GlamourerParameter
		{
			public float Value { get; set; }
			public bool Apply { get; set; }
		}

		public class GlamourerPercentageParameter
		{
			public float Percentage { get; set; }
			public bool Apply { get; set; }
		}

		public class GlamourerColorParameter
		{
			public float Red { get; set; }
			public float Green { get; set; }
			public float Blue { get; set; }
			public bool Apply { get; set; }
		}

		public class GlamourerColorAParameter
		{
			public float Red { get; set; }
			public float Green { get; set; }
			public float Blue { get; set; }
			public float Alpha { get; set; }
			public bool Apply { get; set; }
		}
	}
}