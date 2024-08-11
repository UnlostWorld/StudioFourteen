namespace ScreenshotStudio.Library.Executors;

using ScreenshotStudio;
using ScreenshotStudio.Library;
using ScreenshotStudio.Utilities;
using System.Threading.Tasks;

public class AppearanceExecutor(ILibraryEntry entry, ICharacterAppearance appearance)
	: EntryCharacterExecutor(entry)
{
	private readonly ICharacterAppearance appearance = appearance;

	public override bool CanExecute => this.HasValidTarget;
	public override bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex);

	public override async Task Execute()
	{
		await base.Execute();
		await this.appearance.Apply(this.TargetObjectIndex);
	}

	public override async Task Revert()
	{
		await base.Revert();

		if (!this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex))
			return;

		await this.Services.CharacterAppearance.Restore(this.TargetObjectIndex);
	}
}