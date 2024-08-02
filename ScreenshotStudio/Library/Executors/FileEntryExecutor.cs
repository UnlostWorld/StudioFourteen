namespace ScreenshotStudio.Library.Executors;

using ScreenshotStudio;
using ScreenshotStudio.Files;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Utilities;

public class FileEntryExecutor(ILibraryEntry entry)
	: EntryExecutor(entry)
{
	private FileEntry? fileEntry;
	private FileTypeInfoBase? typeInfo;

	public override bool CanExecute
	{
		get
		{
			if (this.fileEntry == null)
				return false;

			if (this.typeInfo == null)
				return false;

			return this.typeInfo.LoadsType.IsAssignableTo(typeof(ICharacterApplicable));
		}
	}

	public override bool CanRevert
	{
		get
		{
			if (this.fileEntry == null)
				return false;

			if (this.typeInfo == null)
				return false;

			return this.typeInfo.LoadsType.IsAssignableTo(typeof(ICharacterRevertible));
		}
	}

	public override string? Label
	{
		get
		{
			if (this.typeInfo != null && this.typeInfo.LoadsType.IsAssignableTo(typeof(ICharacterApplicable)))
			{
				return $"Apply to {this.Services.Target.CharacterName}";
			}

			return null;
		}
	}

	public override void OnSelect()
	{
		base.OnSelect();

		this.fileEntry = this.Entry as FileEntry;
		this.typeInfo = this.fileEntry?.TypeInfo;
	}

	public unsafe override void Execute()
	{
		base.Execute();

		if (this.typeInfo == null)
			return;

		if (this.typeInfo.LoadsType.IsAssignableTo(typeof(ICharacterApplicable)))
		{
			Threads.RunOnFrameworkThread(() =>
			{
				ICharacterApplicable? file = this.fileEntry?.File as ICharacterApplicable;
				file?.Apply(this.Services.Target.Target);
			});
		}
	}

	public unsafe override void Revert()
	{
		base.Revert();

		if (this.typeInfo == null)
			return;

		if (this.typeInfo.LoadsType.IsAssignableTo(typeof(ICharacterRevertible)))
		{
			Threads.RunOnFrameworkThread(() =>
			{
				ICharacterRevertible? file = this.fileEntry?.File as ICharacterRevertible;
				file?.Revert(this.Services.Target.Target);
			});
		}
	}
}
