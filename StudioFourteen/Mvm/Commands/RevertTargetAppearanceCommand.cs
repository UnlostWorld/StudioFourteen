namespace StudioFourteen.Mvm.Commands;

using System.Threading.Tasks;

public class RevertTargetAppearanceCommand : AutoCommand
{
	protected override bool CanExecute()
	{
		if (!base.CanExecute())
			return false;

		int index = ServiceManager.Instance.Target.TargetObjectIndex;
		return ServiceManager.Instance.CharacterAppearance.CanRestore(index);
	}

	protected override async Task Execute()
	{
		await base.Execute();
		int index = ServiceManager.Instance.Target.TargetObjectIndex;
		await ServiceManager.Instance.CharacterAppearance.Restore(index);
	}
}
