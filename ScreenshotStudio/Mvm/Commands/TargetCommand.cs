namespace ScreenshotStudio.Mvm.Commands;

using System;
using System.Threading.Tasks;

public class TargetCommand : AutoCommand
{
	private readonly Func<int, Task> function;

	public TargetCommand(Func<int, Task> func)
	{
		this.function = func;
	}

	protected override Task Execute()
	{
		int index = ServiceManager.Instance.Target.TargetObjectIndex;
		return this.function.Invoke(index);
	}
}
