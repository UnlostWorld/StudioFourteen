namespace ScreenshotStudio.Library.Executors;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Services;
using System.Threading.Tasks;

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

	public virtual Task Execute()
	{
		return Task.CompletedTask;
	}

	public virtual Task Revert()
	{
		return Task.CompletedTask;
	}
}

public abstract class EntryCharacterExecutor(ILibraryEntry entry)
	: EntryExecutor(entry)
{
	public unsafe Character* Target => this.Services.Target.Target;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public override bool CanExecute => this.HasValidTarget;

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