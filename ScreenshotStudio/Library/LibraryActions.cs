namespace ScreenshotStudio.Library;

using System.Threading.Tasks;

public interface ILibraryActions
{
	public Task Apply(int objectTableIndex);

	/* public unsafe Character* Target => this.Services.Target.Target;

	[AutoNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AutoNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AutoNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;
	[AutoNotify] public override bool CanExecute => this.HasValidTarget;
	[AutoNotify] public override string? Label => $"Apply to {this.CharacterName}"; */
}