// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using XivToolsWpf.DependencyProperties;

public partial class BoneView : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private BoneWindow? owner;
	private bool isPropagatingState = false;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

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

	public bool IsHighlighted { get; set; } = false;
	public bool IsParentSelected { get; set; } = false;

	public string CurrentName
	{
		get
		{
			string name = this.BoneName;

			////if flipped
			////   name = this.FlippedBoneName;

			name = LegacyBoneNameConverter.GetModernName(name) ?? name;
			return name;
		}
	}

	public unsafe void OnSkeletonChanged()
	{
		if (this.owner == null)
			return;

		this.Dispatcher.Invoke(() =>
		{
			BoneCollection? bones = BoneCollection.Search(this.owner.Skeleton, this.CurrentName);

			this.IsEnabled = bones != null;
			this.TooltipInternalNameText.Text = bones?.DisplayName;
		});
	}

	public unsafe void OnSelectionChanged(BoneCollection? selection)
	{
		bool isSelected = false;
		if (selection != null)
		{
			foreach (Bone bone in selection)
			{
				if (bone.Name == this.CurrentName)
				{
					isSelected = true;
				}
			}
		}

		bool isParentSelected = isSelected;

		if (!isParentSelected && selection != null && this.owner != null)
		{
			BoneCollection? bones = BoneCollection.Search(this.owner.Skeleton, this.CurrentName);

			BoneCollection? parents = bones?.GetParents();
			while (parents != null)
			{
				if (selection.Contains(parents))
				{
					isParentSelected = true;
					break;
				}

				parents = parents?.GetParents();
			}
		}

		this.Dispatcher.Invoke(() =>
		{
			this.isPropagatingState = true;

			this.Radio.IsChecked = isSelected;

			this.IsParentSelected = isParentSelected;
			this.PropertyChanged?.Invoke(this, new(nameof(BoneView.IsParentSelected)));

			this.isPropagatingState = false;
		});
	}

	private void OnChecked(object sender, RoutedEventArgs e)
	{
		if (this.isPropagatingState)
			return;

		bool ctrlDown = Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down;
		this.owner?.Select(this.CurrentName, ctrlDown);
	}

	private void OnUnchecked(object sender, RoutedEventArgs e)
	{
		if (this.isPropagatingState)
			return;

		this.owner?.UnSelect(this.CurrentName);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.TooltipDisplayNameText.Text = this.BoneName;

		this.owner = this.FindParent<BoneWindow>();

		if (this.owner == null)
			return;

		this.owner?.BoneViews.Add(this);
		this.OnSkeletonChanged();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.owner?.BoneViews.Remove(this);
	}
}
