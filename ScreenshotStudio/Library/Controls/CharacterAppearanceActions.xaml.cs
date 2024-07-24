namespace ScreenshotStudio.Library.Controls;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Windows;

public partial class CharacterAppearanceActions : View
{
	public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
		nameof(CharacterAppearanceActions.Appearance),
		typeof(ICharacterAppearance),
		typeof(CharacterAppearanceActions),
		new(null, OnAppearanceChanged));

	public CharacterAppearanceActions()
	{
		this.ContentArea.DataContext = this;
	}

	public unsafe Character* Character { get; private set; }
	[AutoNotify] public unsafe string? CharacterName => this.HasValidTarget ? this.Character->GetNameAsString() : "Nobody";
	[AutoNotify] public bool CanApply => this.Appearance != null && this.HasValidTarget;
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

	public ICharacterAppearance? Appearance
	{
		get => (ICharacterAppearance?)this.GetValue(AppearanceProperty);
		set => this.SetValue(AppearanceProperty, value);
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		this.Character = CharacterWindow.GetTarget();
	}

	private static unsafe void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CharacterAppearanceActions actionsView)
		{
			if (e.OldValue is ICharacterAppearance old)
			{
				old.ExecuteRequested -= actionsView.OnExecuteRequested;
			}

			if (e.NewValue is ICharacterAppearance newAppearance)
			{
				newAppearance.ExecuteRequested += actionsView.OnExecuteRequested;
			}

			if (actionsView.IsLive && actionsView.Appearance != null)
			{
				ICharacterAppearance appearance = actionsView.Appearance;
				Threads.RunOnFrameworkThread(() =>
				{
					appearance.Apply(CharacterWindow.GetTarget());
				});
			}
		}
	}

	private void OnExecuteRequested()
	{
		this.OnApplyClicked();
	}

	private unsafe void OnApplyClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Appearance == null)
			return;

		ICharacterAppearance appearance = this.Appearance;
		Threads.RunOnFrameworkThread(() =>
		{
			appearance.Apply(CharacterWindow.GetTarget());
		});
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearanceBackup.Restore(this.Character);
		this.IsLive = false;
	}
}