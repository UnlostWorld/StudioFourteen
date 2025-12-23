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

namespace StudioFourteen.GameData.Sheets;

using Lumina.Excel;
using Lumina.Excel.Sheets;

[Sheet("CharaMakeType", 0x50CDBEEF)]
public readonly unsafe struct CharaMakeType(ExcelPage page, uint offset, uint row) : IExcelRow<CharaMakeType>
{
	public ExcelPage ExcelPage => page;
	public uint RowOffset => offset;
	public uint RowId => row;

	public readonly Collection<CharaMakeMenu> CharaMakeStruct => new(page, offset, offset, &CharaMakeMenuCtor, 28);
	public readonly Collection<byte> VoiceStruct => new(page, offset, offset, &VoiceStructCtor, 12);
	public readonly Collection<FaceTypeOptions> FacialFeatureOption => new(page, offset, offset, &FaceTypeOptionsCtor, 8);
	public readonly Collection<EquipmentStruct> Equipment => new(page, offset, offset, &EquipmentCtor, 3);
	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadInt32(offset + 13064), page.Language);
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadInt32(offset + 13068), page.Language);
	public readonly sbyte Gender => page.ReadInt8(offset + 13072);

	static CharaMakeType IExcelRow<CharaMakeType>.Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);

	private static CharaMakeMenu CharaMakeMenuCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, parentOffset, offset + (i * 452));
	private static byte VoiceStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt8(offset + 12656 + i);
	private static FaceTypeOptions FaceTypeOptionsCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, offset + 12668 + (i * 28));
	private static EquipmentStruct EquipmentCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, offset + 12896 + (i * 56));

	public readonly struct CharaMakeMenu(ExcelPage page, uint parentOffset, uint offset)
	{
		public readonly RowRef<Lobby> Menu => new(page.Module, page.ReadUInt32(offset), page.Language);
		public readonly uint SubMenuMask => page.ReadUInt32(offset + 4);
		public readonly uint Customize => page.ReadUInt32(offset + 8);
		public readonly Collection<uint> SubMenuParam => new(page, parentOffset, offset, &SubMenuParamCtor, 100);
		public readonly byte InitVal => page.ReadUInt8(offset + 436);
		public readonly byte SubMenuType => page.ReadUInt8(offset + 437);
		public readonly byte SubMenuNum => page.ReadUInt8(offset + 438);
		public readonly byte LookAt => page.ReadUInt8(offset + 439);
		public readonly Collection<byte> SubMenuGraphic => new(page, parentOffset, offset, &SubMenuGraphicCtor, 10);

		private static uint SubMenuParamCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt32(offset + 12 + (i * 4));
		private static byte SubMenuGraphicCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt8(offset + 416 + i);
	}

	public readonly struct FaceTypeOptions(ExcelPage page, uint offset)
	{
		public readonly int Option1 => page.ReadInt32(offset);
		public readonly int Option2 => page.ReadInt32(offset + 4);
		public readonly int Option3 => page.ReadInt32(offset + 8);
		public readonly int Option4 => page.ReadInt32(offset + 12);
		public readonly int Option5 => page.ReadInt32(offset + 16);
		public readonly int Option6 => page.ReadInt32(offset + 20);
		public readonly int Option7 => page.ReadInt32(offset + 24);

		public readonly Collection<int> Options => new(page, offset, offset, &FacialFeatureOptionCtor, 7);

		private static int FacialFeatureOptionCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadInt32(offset + (i * 4));
	}

	public readonly struct EquipmentStruct(ExcelPage page, uint offset)
	{
		public readonly ulong Helmet => page.ReadUInt64(offset);
		public readonly ulong Top => page.ReadUInt64(offset + 8);
		public readonly ulong Gloves => page.ReadUInt64(offset + 16);
		public readonly ulong Legs => page.ReadUInt64(offset + 24);
		public readonly ulong Shoes => page.ReadUInt64(offset + 32);
		public readonly ulong Weapon => page.ReadUInt64(offset + 40);
		public readonly ulong SubWeapon => page.ReadUInt64(offset + 48);
	}
}