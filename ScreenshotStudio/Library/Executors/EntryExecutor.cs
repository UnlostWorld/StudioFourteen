namespace ScreenshotStudio.Library.Executors;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Services;

public abstract class EntryExecutor(ILibraryEntry entry)
	: ViewModel
{
	public ILibraryEntry Entry = entry;

	[AutoNotify] public abstract string? Label { get; }
	[AutoNotify] public abstract bool CanExecute { get; }
	[AutoNotify] public abstract bool CanRevert { get; }

	public virtual void OnSelect()
	{
		AutoPropertyNotifyService.Register(this);
	}

	public virtual void OnDeselect()
	{
		AutoPropertyNotifyService.Remove(this);
	}

	public virtual void OnFrameworkUpdate()
	{
	}

	public virtual void Execute()
	{
	}

	public virtual void Revert()
	{
	}
}

public abstract class EntryCharacterExecutor(ILibraryEntry entry)
	: EntryExecutor(entry)
{
	public unsafe Character* Target => this.Services.Target.Target;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public override string? Label => $"Apply to {this.CharacterName}";

	public override void OnSelect()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		this.OnTargetChanged();
		base.OnSelect();
	}

	public override void OnDeselect()
	{
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		base.OnDeselect();
	}

	protected virtual void OnTargetChanged()
	{
	}
}