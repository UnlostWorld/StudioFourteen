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

namespace StudioFourteen.Selection;

using Dalamud.Plugin.Services;
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.History;
using StudioFourteen.Mvm;
using StudioFourteen.Posing;
using System;

public abstract partial class SelectionBase : ViewModel, IHistoryTarget
{
	[Notify] private string name = string.Empty;
	[Notify] private string? subtitle;
	[Notify] private string? description;
	[Notify] private bool isReady = false;

	public abstract IconChar Icon { get; }
	public abstract string TypeName { get; }

	public virtual bool CanMirror => false;
	[History] public virtual MirrorModes MirrorMode { get; set; }

	public abstract bool CanReset { get; }
	public abstract ISelectionId Id { get; }

	public virtual void Reset()
	{
	}

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}

	public virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	public virtual bool Equals(SelectionBase? other)
	{
		return this == other;
	}

	public Operation CreateHistoryOperation()
	{
		return new SelectionObjectOperation();
	}

	public virtual void FinalizeHistoryOperation(ref Operation operation)
	{
	}

	public class SelectionObjectOperation : Operation
	{
		public override IHistoryTarget GetTarget()
		{
			if (ServiceManager.Instance.Selection.Current == null)
				throw new Exception("No selection");

			return ServiceManager.Instance.Selection.Current;
		}

		public override bool IsTarget(IHistoryTarget target)
		{
			return ServiceManager.Instance.Selection.Current == target;
		}
	}
}