namespace ScreenshotStudio.Library.Controls;

using Dalamud.Plugin.Services;
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

	public unsafe Actor* Actor { get; private set; }
	[AutoNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";
	[AutoNotify] public bool CanApply => this.Item != null && this.HasValidTarget && this.Item.EquipSlot != null;
	[AutoNotify] public unsafe bool CanRevert => this.Services.ActorAppearanceBackup.CanRestore(this.Actor);
	[AutoNotify] public bool IsLive { get; set; }

	[AutoNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			if (this.Actor == null)
				return false;

			if (this.Actor->GameObject.RenderFlags != (int)RenderMode.Draw)
				return false;

			return true;
		}
	}

	public Item? Item
	{
		get => (Item?)this.GetValue(ItemProperty);
		set => this.SetValue(ItemProperty, value);
	}

	protected unsafe void OnFrameworkUpdate(IFramework framework)
	{
		this.Actor = ActorWindow.GetTarget();
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

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (DalamudServices.Framework != null)
		{
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;
		}
	}

	private unsafe void OnApplyClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		// ...
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.ActorAppearanceBackup.Restore(this.Actor);
		this.IsLive = false;
	}

	private void OnInfoClicked(object sender, RoutedEventArgs e)
	{
		if (this.Item == null)
			return;

		UrlUtility.Open($"https://ffxiv.consolegameswiki.com/mediawiki/index.php?search={this.Item.Name}");
	}
}