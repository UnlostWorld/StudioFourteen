namespace ScreenshotStudio.Studio.Pose;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System.ComponentModel;
using System.Windows;

public partial class PoseGuiView : View
{
	private ActorWindow? window;

	public PoseGuiView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.Loaded += this.OnLoaded;
	}

	// TODO: this might be better as a setting, or a value per actor
	[AutoNotify] public bool FlipSides { get; set; }

	[AutoNotify] public bool IsValid => this.window != null && this.window.HasValidTarget;
	[AutoNotify] public bool HasTail => this.IsMiqote || this.IsAuRa || this.IsHrothgar || this.IsIVCS;
	[AutoNotify] public unsafe bool IsCustomFace => this.IsMiqote || this.IsHrothgar;
	[AutoNotify] public unsafe bool IsMiqote => !this.IsValid ? false : this.Actor->GetCustomizeValue(CustomizeIndex.Race) == (byte)Race.RaceRows.Miqote;
	[AutoNotify] public unsafe bool IsViera => !this.IsValid ? false : this.Actor->GetCustomizeValue(CustomizeIndex.Race) == (byte)Race.RaceRows.Viera;
	[AutoNotify] public unsafe bool IsAuRa => !this.IsValid ? false : this.Actor->GetCustomizeValue(CustomizeIndex.Race) == (byte)Race.RaceRows.AuRa;
	[AutoNotify] public unsafe bool IsElezen => !this.IsValid ? false : this.Actor->GetCustomizeValue(CustomizeIndex.Race) == (byte)Race.RaceRows.Elezen;
	[AutoNotify] public unsafe bool IsHrothgar => !this.IsValid ? false : this.Actor->GetCustomizeValue(CustomizeIndex.Race) == (byte)Race.RaceRows.Hrothgar;
	[AutoNotify] public unsafe bool HasTailOrEars => this.IsViera || this.HasTail;
	[AutoNotify] public unsafe bool IsEars01 => this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) <= 1;
	[AutoNotify] public unsafe bool IsEars02 => this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) == 2;
	[AutoNotify] public unsafe bool IsEars03 => this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) == 3;
	[AutoNotify] public unsafe bool IsEars04 => this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) == 4;
	[AutoNotify] public unsafe bool IsIVCS { get; private set; }

	[AutoNotify]
	public unsafe bool IsVieraEarsFlop
	{
		get
		{
			if (!this.IsValid)
				return false;

			if (this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.Gender) == (byte)Genders.Feminine
				&& this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) == 3)
				return true;

			if (this.IsViera && this.Actor->GetCustomizeValue(CustomizeIndex.Gender) == (byte)Genders.Masculine
				&& this.Actor->GetCustomizeValue(CustomizeIndex.RaceFeatureType) == 2)
				return true;

			return false;
		}
	}

	protected unsafe Actor* Actor => this.window == null ? default : this.window.Actor;
	protected unsafe ref DrawDataContainer DrawData => ref this.Actor->DrawData;

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.window = this.FindParent<ActorWindow>();
	}
}