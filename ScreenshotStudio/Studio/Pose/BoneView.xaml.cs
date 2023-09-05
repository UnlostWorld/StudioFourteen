// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf.DependencyProperties;

public partial class BoneView : UserControl
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public string? InternalBoneName => "TODO";
	public string? LocalizedBoneName => "HUH";

	public bool IsSelected { get; set; }
	public bool IsHighlighted { get; set; }
	public bool IsParentSelected { get; set; }

	public string Label
	{
		get => LabelDp.Get(this);
		set => LabelDp.Set(this, value);
	}

	public string BoneName
	{
		get => NameDp.Get(this);
		set => NameDp.Set(this, value);
	}

	public string FlippedBoneName
	{
		get => FlippedNameDp.Get(this);
		set => FlippedNameDp.Set(this, value);
	}

	public void Select(bool select, bool add)
	{
	}

	private void OnChecked(object sender, RoutedEventArgs e)
	{
		bool ctrlDown = Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down;
		this.Select(true, ctrlDown);
	}

	private void OnUnchecked(object sender, RoutedEventArgs e)
	{
		this.Select(false, false);
	}
}
