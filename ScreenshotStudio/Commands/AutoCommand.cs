namespace ScreenshotStudio.Commands;

using ScreenshotStudio.Services;
using System.ComponentModel;
using WpfUtils.Commands;

public class AutoCommand : SimpleCommand, IAutoNotify
{
	public AutoCommand()
	{
		AutoPropertyNotifyService.Register(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[AutoNotify] public virtual bool CanCommandExecute => this.CanExecute(null);

	public void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));

		if (propertyName == nameof(AutoCommand.CanCommandExecute))
		{
			this.RaiseCanExecuteChanged();
		}
	}

	public bool ShouldTickAutoProperties()
	{
		return true;
	}
}