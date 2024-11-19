namespace StudioFourteen.Panels;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Mvm;

public abstract class CharacterPanelBase : Panel
{
	[AlwaysNotify] public bool IsTargetLoading => this.Services.Target.IsTargetLoading;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	protected override void OnOpened()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		base.OnClosed();
	}

	protected virtual void OnTargetChanged()
	{
	}
}