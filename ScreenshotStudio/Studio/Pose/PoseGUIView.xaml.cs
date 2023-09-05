// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using System.Windows.Controls;

public partial class PoseGuiView : UserControl
{
	public PoseGuiView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	// TODO: this might be better as a setting, or a value per actor
	public bool FlipSides { get; set; }

	public bool HasTail => true;

	public bool IsCustomFace => false; // this.Actor == null ? false : this.IsMiqote || this.IsHrothgar;
	public bool IsMiqote => false; // this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Miqote;
	public bool IsViera => false; // this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Viera;
	public bool IsElezen => false; // this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Elezen;
	public bool IsHrothgar => false; // this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Hrothgar;
	public bool HasTailOrEars => this.IsViera || this.HasTail;
	public bool IsEars01 => false; // this.IsViera && this.Actor?.Customize?.TailEarsType <= 1;
	public bool IsEars02 => false; // this.IsViera && this.Actor?.Customize?.TailEarsType == 2;
	public bool IsEars03 => false; // this.IsViera && this.Actor?.Customize?.TailEarsType == 3;
	public bool IsEars04 => false; // this.IsViera && this.Actor?.Customize?.TailEarsType == 4;
	public bool IsIVCS => false; // this.Actor?.ModelObject?.Skeleton?.GetBone("iv_ko_c_l") != null;

	public bool IsVieraEarsFlop
	{
		get
		{
			/*if (this.IsViera && this.Actor?.Customize?.Gender == ActorCustomizeMemory.Genders.Feminine && this.Actor?.Customize?.TailEarsType == 3)
				return true;

			if (this.IsViera && this.Actor?.Customize?.Gender == ActorCustomizeMemory.Genders.Masculine && this.Actor?.Customize?.TailEarsType == 2)
				return true;

			return false;*/
			return false;
		}
	}
}
