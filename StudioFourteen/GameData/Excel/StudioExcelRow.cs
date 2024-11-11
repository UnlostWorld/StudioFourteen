namespace StudioFourteen.GameData.Excel;

using Lumina.Excel;
using Serilog;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class StudioExcelRow : ExcelRow, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public virtual bool IsValid => true;
	public string RowName => $"{this.GetType().Name} #{this.RowId}";

	protected ServiceManager Services => ServiceManager.Instance;
	protected ILogger Log => Logging.ForContext(this.GetType());

	public override string ToString()
	{
		return this.RowName;
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}