namespace StudioFourteen.Context;

using FontAwesome.Sharp;
using StudioFourteen.Input;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfUtils.Commands;
using WpfUtils.Extensions;

public interface IContextProvider
{
	Task GetMenu(ref List<MenuEntry> menus, int objectTableIndex);
}

/// <summary>
/// The Context Service is responsible for initiating the right click menu that appears over
/// world objects. (For the context menu that appears in the library, see LibraryContextMenu.cs).
/// </summary>
public class ContextService : ServiceBase
{
	private readonly List<IContextProvider> contextProviders = new();
	private HitInfo? currentHitInfo;

	public delegate void ContextMenuDelegate(Vector2 position, List<MenuEntry> entries);

	public event ContextMenuDelegate? ShowMenu;

	public void AddProvider(IContextProvider provider)
	{
		this.contextProviders.Add(provider);
	}

	public void RemoveProvider(IContextProvider provider)
	{
		this.contextProviders.Remove(provider);
	}

	public override Task Initialize()
	{
		this.Services.Input.MouseButton += this.OnMouseButton;
		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Services.Input.MouseButton -= this.OnMouseButton;
		return base.Shutdown();
	}

	private void OnMouseButton(MouseButton button, InputService.States state, Vector2 position)
	{
		if (button == MouseButton.Right && state == InputService.States.Released)
		{
			this.ShowContext(position).Run();
		}
	}

	private async Task ShowContext(Vector2 screenPosition)
	{
		await this.DoRayCast(screenPosition);
	}

	private async Task DoRayCast(Vector2 screenPos)
	{
		await Threads.FrameworkThread();
		this.currentHitInfo = RayCast.Cast(screenPos);

		List<MenuEntry> entries = new();
		foreach (IContextProvider provider in this.contextProviders)
		{
			await provider.GetMenu(ref entries, this.currentHitInfo.ObjectTableIndex);
		}

		foreach (MenuEntry entry in entries)
		{
			entry.SetCallback(this.OnContextMenuClicked);
		}

		this.ShowMenu?.Invoke(screenPos, entries);
	}

	private void OnContextMenuClicked(MenuEntry entry)
	{
		if (this.currentHitInfo == null)
			return;

		entry.Callback?.Invoke(this.currentHitInfo);
	}
}

public class MenuEntry(IconChar? icon, string? label, bool isEnabled = true, Func<HitInfo, Task>? callback = null)
{
	private Action<MenuEntry>? invokeCallback;

	public Func<HitInfo, Task>? Callback => callback;
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