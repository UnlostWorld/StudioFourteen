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

namespace StudioFourteen.Settings;

using Dalamud.Configuration;
using StudioFourteen.Input;
using StudioFourteen.Library;
using StudioFourteen.Photos;
using System;
using System.Collections.Generic;
using System.IO;

[NotifyPropertyChanged]
public partial class Configuration : IPluginConfiguration
{
	public Configuration()
	{
		this.Persistence = new();
		this.HasConfirmedReShadeVersion = -1;
		this.DefaultVersion = "1.0";
		this.PhotoFormat = PhotosService.Formats.Jpeg;
		this.PhotoIncludeMetaData = true;
		this.PhotoAnimationFlash = true;
		this.PhotoAnimationPreview = true;
		this.SendErrorReports = true;
		this.HideGenitals = true;
		this.ShowOverlays = true;
		this.Overlays = new();
		this.OpenPanels = new();
		this.ResourcePacks = new();
		this.WidgetMode = WidgetModes.Inspector;
		this.EnableInspector = true;
		this.EnableDedicatedInspectors = true;
		this.AllInOne = AioModes.Optional;
		this.EnableTargetBar = true;
		this.AllowKeyboardCapture = true;
		this.AllowMouseCapture = true;
		this.CustomBinds = new();
		this.Favorites = new();
		this.LibraryPreviewMode = PreviewModes.Temporary;
		this.TrustedScripts = new();
	}

	public enum WidgetModes
	{
		Disabled,
		GizmoControls,
		Inspector,
	}

	public enum AioModes
	{
		Disabled,
		Optional,
		Always,
	}

	public int Version { get; set; } = 0;

	[Bind] public partial Dictionary<string, string> Persistence { get; set; }
	[Bind] public partial int HasConfirmedReShadeVersion { get; set; }

	// Filepublic partial
	[Bind] public partial string? LastSaveDirectory { get; set; }
	[Bind] public partial string? DefaultAuthor { get; set; }
	[Bind] public partial string? DefaultVersion { get; set; }

	// Photpublic partial
	[Bind] public partial string? PhotoDirectory { get; set; }
	[Bind] public partial PhotosService.Formats PhotoFormat { get; set; }
	[Bind] public partial bool PhotoIncludeMetaData { get; set; }
	[Bind] public partial bool PhotoCaptureDepth { get; set; }

	[Bind] public partial bool PhotoAnimationFlash { get; set; }
	[Bind] public partial bool PhotoAnimationPreview { get; set; }

	// Analpublic partial
	[Bind] public partial bool HasConfirmedAnalyticOptions { get; set; }
	[Bind] public partial bool SendOptionalAnalytics { get; set; }
	[Bind] public partial bool SendErrorReports { get; set; }

	// Intepublic partial
	[Bind] public partial bool HideLauncherButton { get; set; }
	[Bind] public partial bool OpenGroupPose { get; set; }
	[Bind] public partial bool HideGenitals { get; set; }
	[Bind] public partial bool ShowOverlays { get; set; }
	[Bind] public partial Dictionary<string, int> Overlays { get; set; }
	[Bind] public partial List<string> OpenPanels { get; set; }
	[Bind] public partial bool IsAioWindowOpen { get; set; }
	[Bind] public partial bool UseSystemCursors { get; set; }
	[Bind] public partial List<string> ResourcePacks { get; set; }

	[Bind] public partial WidgetModes WidgetMode { get; set; }
	[Bind] public partial bool EnableInspector { get; set; }
	[Bind] public partial bool EnableDedicatedInspectors { get; set; }
	[Bind] public partial AioModes AllInOne { get; set; }
	[Bind] public partial bool EnableTargetBar { get; set; }

	// Inpupublic partial
	[Bind] public partial bool AllowKeyboardCapture { get; set; }
	[Bind] public partial bool AllowMouseCapture { get; set; }
	[Bind] public partial Dictionary<InputAction, List<Bind>> CustomBinds { get; set; }

	// Librpublic partial
	[Bind] public partial HashSet<string> Favorites { get; set; }
	[Bind] public partial PreviewModes LibraryPreviewMode { get; set; }

	// Scripublic partial
	[Bind] public partial Dictionary<string, string> TrustedScripts { get; set; }

	public void Validate()
	{
		if (this.PhotoDirectory == null)
		{
			this.PhotoDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Studio Fourteen");
		}
	}
}
