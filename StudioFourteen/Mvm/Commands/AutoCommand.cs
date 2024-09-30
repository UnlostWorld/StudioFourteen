namespace ScreenshotStudio.Mvm.Commands;

using ScreenshotStudio.Mvm;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WpfUtils.Commands;

public class AutoCommand : SimpleCommand, IAutoNotify
{
	public AutoCommand()
	{
		AutoPropertyNotifyService.Register(this);
	}

	public AutoCommand(Action action)
		: this()
	{
		this.action = action;
	}

	public AutoCommand(Func<Task> func)
		: this()
	{
		this.asyncFunc = func;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[AutoNotify] public virtual bool CanCommandExecute => this.CanExecute(null);

	public void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));

		if (propertyName == nameof(this.CanCommandExecute))
		{
			this.RaiseCanExecuteChanged();
		}
	}

	public bool ShouldTickAutoProperties()
	{
		return true;
	}
}