namespace StudioFourteen.Context;

using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using Serilog;
using StudioFourteen.Input;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using WpfUtils;
using WpfUtils.Commands;
using WpfUtils.Controls;
using WpfUtils.Extensions;

[DependencyProperty<string>("ObjectName")]
public partial class WorldContextMenu : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();
	private static readonly List<IProvider> ContextProviders = new();
	private HitInfo? currentHitInfo;

	public WorldContextMenu()
	{
		this.Services.Input.MouseButton += this.OnMouseButton;
	}

	public interface IProvider
	{
		Task GetMenu(WorldContextMenu menu);
	}

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public bool IsObject => this.ObjectTableIndex != -1;
	public int ObjectTableIndex => this.currentHitInfo?.ObjectTableIndex ?? -1;
	public Vector3 Position => this.currentHitInfo?.Position ?? Vector3.Zero;

	public static void AddProvider(IProvider provider)
	{
		ContextProviders.Add(provider);
	}

	public static void RemoveProvider(IProvider provider)
	{
		ContextProviders.Remove(provider);
	}

	public void Add(IconChar? icon, string? label, bool isEnabled = true, Func<WorldContextMenu, Task>? callback = null)
	{
		MenuEntry entry = new(icon, label, isEnabled, callback);
		entry.SetCallback(this.OnContextMenuClicked);

		this.Dispatcher.Invoke(() =>
		{
			this.Menus.Add(entry);
		});
	}

	private void OnMouseButton(MouseButton button, InputService.States state, Vector2 position)
	{
		if (button == MouseButton.Right && state == InputService.States.Released)
		{
			this.Show(position).Run();
		}
	}

	private async Task Show(Vector2 screenPosition)
	{
		await this.MainThread();
		this.Menus.Clear();

		string? name = null;
		await Threads.FrameworkThread();

		unsafe
		{
			this.currentHitInfo = RayCast.Cast(screenPosition);

			if (this.currentHitInfo.ObjectTableIndex != -1)
			{
				name = this.currentHitInfo.GameObject->GetDisplayName();
			}
			else
			{
				name = this.currentHitInfo.Position.ToString();
			}
		}

		foreach (IProvider provider in ContextProviders)
		{
			await provider.GetMenu(this);
		}

		await this.MainThread();

		this.PlacementRectangle = new Rect(screenPosition.X, screenPosition.Y + 50, 1, 1);
		this.IsOpen = true;

		this.ObjectName = name;
	}

	private void OnContextMenuClicked(MenuEntry entry)
	{
		if (this.currentHitInfo == null)
			return;

		this.IsOpen = false;
		entry.Callback?.Invoke(this);
	}
}

public class MenuEntry(IconChar? icon, string? label, bool isEnabled = true, Func<WorldContextMenu, Task>? callback = null)
{
	private Action<MenuEntry>? invokeCallback;

	public Func<WorldContextMenu, Task>? Callback => callback;
	public IconChar? Icon => icon;
	public bool IsEnabled => isEnabled;
	public ICommand? OnClicked => new SimpleCommand(this.Invoke);
	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public string? Label => label;

	public void SetCallback(Action<MenuEntry> callback)
	{
		this.invokeCallback = callback;

		foreach (MenuEntry entry in this.Children)
		{
			entry.SetCallback(callback);
		}
	}

	private void Invoke()
	{
		this.invokeCallback?.Invoke(this);
	}
}
