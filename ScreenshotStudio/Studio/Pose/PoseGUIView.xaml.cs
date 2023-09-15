// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public partial class PoseGuiView : UserControl, IAutoNotify
{
	private BoneWindow? window;

	public PoseGuiView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	// TODO: this might be better as a setting, or a value per actor
	[AutoNotify] public bool FlipSides { get; set; }

	[AutoNotify] public bool IsValid => this.window != null && this.window.HasValidTarget;
	[AutoNotify] public bool HasTail => this.IsMiqote || this.IsAuRa || this.IsHrothgar || this.IsIVCS;
	[AutoNotify] public unsafe bool IsCustomFace => this.IsMiqote || this.IsHrothgar;
	[AutoNotify] public unsafe bool IsMiqote => !this.IsValid ? false : this.Customize.Race == Race.RaceRows.Miqote;
	[AutoNotify] public unsafe bool IsViera => !this.IsValid ? false : this.Customize.Race == Race.RaceRows.Viera;
	[AutoNotify] public unsafe bool IsAuRa => !this.IsValid ? false : this.Customize.Race == Race.RaceRows.AuRa;
	[AutoNotify] public unsafe bool IsElezen => !this.IsValid ? false : this.Customize.Race == Race.RaceRows.Elezen;
	[AutoNotify] public unsafe bool IsHrothgar => !this.IsValid ? false : this.Customize.Race == Race.RaceRows.Hrothgar;
	[AutoNotify] public unsafe bool HasTailOrEars => this.IsViera || this.HasTail;
	[AutoNotify] public unsafe bool IsEars01 => this.IsViera && this.Customize.TailEarsType <= 1;
	[AutoNotify] public unsafe bool IsEars02 => this.IsViera && this.Customize.TailEarsType == 2;
	[AutoNotify] public unsafe bool IsEars03 => this.IsViera && this.Customize.TailEarsType == 3;
	[AutoNotify] public unsafe bool IsEars04 => this.IsViera && this.Customize.TailEarsType == 4;
	[AutoNotify] public unsafe bool IsIVCS { get; private set; }

	[AutoNotify]
	public unsafe bool IsVieraEarsFlop
	{
		get
		{
			if (!this.IsValid)
				return false;

			if (this.IsViera && this.Actor->DrawData.Customize.Gender == Genders.Feminine
				&& this.Actor->DrawData.Customize.TailEarsType == 3)
				return true;

			if (this.IsViera && this.Actor->DrawData.Customize.Gender == Genders.Masculine
				&& this.Actor->DrawData.Customize.TailEarsType == 2)
				return true;

			return false;
		}
	}

	protected unsafe Actor* Actor => this.window == null ? default : this.window.Actor;
	protected unsafe ref Customize Customize => ref this.Actor->DrawData.Customize;

	public unsafe void OnSkeletonChanged(Skeleton* skeleton)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.IsIVCS = BoneCollection.Search(skeleton, "iv_ko_c_l") != null;
		});
	}

	public void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	public bool ShouldTickAutoProperties()
	{
		return this.IsValid;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.window = this.FindParent<BoneWindow>();
		AutoPropertyNotifyService.Register(this);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		AutoPropertyNotifyService.Remove(this);
	}
}