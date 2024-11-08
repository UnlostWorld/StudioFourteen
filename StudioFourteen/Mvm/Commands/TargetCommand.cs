namespace StudioFourteen.Mvm.Commands;

using PropertyChanged.SourceGenerator;
using System;
using System.Threading.Tasks;
using WpfUtils.Commands;

public partial class TargetCommand : SimpleCommand
{
	private readonly Func<int, Task>? function;
	private readonly bool groupPoseOnly;

	[Notify] private string? targetName;
	[Notify] private int objectIndex = -1;

	public TargetCommand(Func<int, Task> func, bool groupPoseOnly = false)
	{
		this.function = func;
		this.groupPoseOnly = groupPoseOnly;

		ServiceManager.Instance.Target.TargetChanged += this.OnTargetChanged;
		ServiceManager.Instance.GroupPose.StateChanged += this.OnGroupPoseStateChanged;

		this.OnTargetChanged();
	}

	protected TargetCommand()
	{
	}

	protected override bool CanExecute()
	{
		if (!base.CanExecute())
			return false;

		if (this.groupPoseOnly && !ServiceManager.Instance.GroupPose.IsGroupPosing)
			return false;

		if (this.objectIndex == -1)
			return false;

		return true;
	}

	protected override Task Execute()
	{
		if (this.function == null)
			return Task.CompletedTask;

		return this.function.Invoke(this.ObjectIndex);
	}

	private void OnTargetChanged()
	{
		this.TargetName = ServiceManager.Instance.Target.CharacterName;
		this.ObjectIndex = ServiceManager.Instance.Target.TargetObjectIndex;
		this.RaiseCanExecuteChanged();
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.RaiseCanExecuteChanged();
	}
}