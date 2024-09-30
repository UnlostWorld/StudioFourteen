namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("WeatherRate", 0x474abce2)]
public class WeatherRate : StudioExcelRow
{
	public WeatherInstance[]? Weathers { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Weathers = new WeatherInstance[8];
		for (int i = 0; i < 8; i++)
		{
			this.Weathers[i] = default(WeatherInstance);
			this.Weathers[i].Weather = parser.ReadColumn<int>(i * 2);
			this.Weathers[i].Rate = parser.ReadColumn<byte>((i * 2) + 1);
		}
	}

	public struct WeatherInstance
	{
		public int Weather;
		public byte Rate;
	}
}
