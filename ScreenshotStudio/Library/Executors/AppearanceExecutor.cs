namespace ScreenshotStudio.Library.Executors;

using ScreenshotStudio;
using ScreenshotStudio.Library;
using ScreenshotStudio.Utilities;

public class AppearanceExecutor(ILibraryEntry entry, ICharacterAppearance appearance)
	: EntryCharacterExecutor(entry)
{
	private readonly ICharacterAppearance appearance = appearance;

	public override bool CanExecute => this.HasValidTarget;
	public override bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex);

	public unsafe override void Execute()
	{
		base.Execute();

		Threads.RunOnFrameworkThread(() =>
		{
			this.appearance.Apply(this.Target);
		});
	}

	public override void Revert()
	{
		base.Revert();

		if (!this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex))
			return;

		this.Services.CharacterAppearance.Restore(this.TargetObjectIndex);
	}
}