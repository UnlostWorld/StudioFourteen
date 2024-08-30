namespace ScreenshotStudio.Windows;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Services;

public abstract class CharacterPanelBase : Panel
{
	public unsafe Character* Target => this.Services.Target.Target;
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