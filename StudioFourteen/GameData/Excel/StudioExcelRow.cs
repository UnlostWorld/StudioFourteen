namespace StudioFourteen.GameData.Excel;

using Lumina.Excel;
using Serilog;

public class StudioExcelRow : ExcelRow
{
	protected readonly ILogger Log;

	public StudioExcelRow()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public virtual bool IsValid => true;
	public string RowName => $"{this.GetType().Name} #{this.RowId}";

	public override string ToString()
	{
		return this.RowName;
	}
}