namespace ScreenshotStudio.Appearance;

using ScreenshotStudio.Files;
using ScreenshotStudio.Library;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Panels;
using System.Windows;
using WpfUtils.Extensions;

public partial class CharacterPanel : CharacterPanelBase
{
	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.Target);

	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(ScreenshotStudio.Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.CharacterName);

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearance.Restore(this.TargetObjectIndex).Run();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryWindow.Open(LibraryWindow.LibraryTabs.Characters);
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		AppearanceFile file = new();
		file.Read(this.TargetObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.CharacterName}'s Appearance");
	}
}
