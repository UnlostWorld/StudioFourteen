namespace ScreenshotStudio.Library.Controls;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using System.Windows;

public partial class ItemActions : View
{
	public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
		nameof(ItemActions.Item),
		typeof(Item),
		typeof(ItemActions),
		new(null, OnItemChanged));

	public ItemActions()
	{
		this.ContentArea.DataContext = this;
	}

	public unsafe Character* Character { get; private set; }
	[AutoNotify] public unsafe string? CharacterName => this.HasValidTarget ? this.Character->GetNameAsString() : "Nobody";
	[AutoNotify] public bool CanApply => this.Item != null && this.HasValidTarget && this.Item.EquipSlot != null;
	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearanceBackup.CanRestore(this.Character);
	[AutoNotify] public bool IsLive { get; set; }

	[AutoNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			if (this.Character == null)
				return false;

			if (this.Character->GameObject.RenderFlags != (int)RenderMode.Draw)
				return false;

			return true;
		}
	}

	public Item? Item
	{
		get => (Item?)this.GetValue(ItemProperty);
		set => this.SetValue(ItemProperty, value);
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		this.Character = CharacterWindow.GetTarget();
	}

	private static unsafe void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ItemActions actionsView)
		{
			if (e.OldValue is Item oldValue)
			{
				oldValue.ExecuteRequested -= actionsView.OnExecuteRequested;
			}

			if (e.NewValue is Item newValue)
			{
				newValue.ExecuteRequested += actionsView.OnExecuteRequested;
			}
		}
	}

	private void OnExecuteRequested()
	{
		this.OnApplyClicked();
	}

	private unsafe void OnApplyClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		// ...
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearanceBackup.Restore(this.Character);
		this.IsLive = false;
	}

	private void OnInfoClicked(object sender, RoutedEventArgs e)
	{
		if (this.Item == null)
			return;

		UrlUtility.Open($"https://ffxiv.consolegameswiki.com/mediawiki/index.php?search={this.Item.Name}");
	}
}