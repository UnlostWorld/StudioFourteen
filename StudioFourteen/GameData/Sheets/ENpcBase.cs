namespace StudioFourteen.GameData.Sheets;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;

[Sheet("ENpcBase", 0x464052CD)]
public readonly unsafe struct ENpcBase(ExcelPage page, uint offset, uint row)
	: IExcelRow<ENpcBase>
{
	public uint RowId => row;

	public readonly CustomizeData CustomizeData
	{
		get
		{
			CustomizeData c = default;

			for (int i = 0; i < CustomizeDataExtensions.NumOptions; i++)
			{
				CustomizeIndex index = (CustomizeIndex)i;
				byte val = page.ReadUInt8((nuint)(offset + 202 + i));
				c.SetValue(index, val);
			}

			return c;
		}
	}

	static ENpcBase IExcelRow<ENpcBase>.Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);
}