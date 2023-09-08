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

	private BoneWindow? owner;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

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

	public unsafe void OnSkeletonChanged()
	{
		if (this.owner == null)
			return;

		this.Dispatcher.Invoke(() =>
		{
			string name = LegacyBoneNameConverter.GetModernName(this.BoneName) ?? this.BoneName;
			BoneReferences? bones = BoneReferences.Search(this.owner.Skeleton, name);

			this.IsEnabled = bones != null;
			this.TooltipInternalNameText.Text = bones?.Name;
		});
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

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.TooltipDisplayNameText.Text = this.BoneName;

		this.owner = this.FindParent<BoneWindow>();
		this.owner?.BoneViews.Add(this);

		this.OnSkeletonChanged();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.owner?.BoneViews.Remove(this);
	}
}
