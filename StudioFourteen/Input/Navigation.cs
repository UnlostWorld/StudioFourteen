// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Input;

using DependencyPropertyGenerator;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;

public partial class Navigation
{
	public static readonly RoutedEvent EnterEvent = EventManager.RegisterRoutedEvent(
		"Enter",
		RoutingStrategy.Bubble,
		typeof(RoutedEventHandler),
		typeof(Navigation));

	public static readonly RoutedEvent BackEvent = EventManager.RegisterRoutedEvent(
		"Back",
		RoutingStrategy.Bubble,
		typeof(RoutedEventHandler),
		typeof(Navigation));

	protected readonly ILogger Log;

	private readonly DependencyObject scope;
	private readonly InputActionListener upListener;
	private readonly InputActionListener downListener;
	private readonly InputActionListener leftListener;
	private readonly InputActionListener rightListener;
	private readonly InputActionListener enterListener;
	private readonly InputActionListener backListener;

	public Navigation(DependencyObject scope)
	{
		this.scope = scope;
		this.Log = Logging.ForContext(this.GetType());

		this.upListener = new(InputAction.Navigate_Up);
		this.upListener.Activate = () => this.OnNavigate(FocusNavigationDirection.Up).Run();

		this.downListener = new(InputAction.Navigate_Down);
		this.downListener.Activate = () => this.OnNavigate(FocusNavigationDirection.Down).Run();

		this.leftListener = new(InputAction.Navigate_Left);
		this.leftListener.Activate = () => this.OnNavigate(FocusNavigationDirection.Left).Run();

		this.rightListener = new(InputAction.Navigate_Right);
		this.rightListener.Activate = () => this.OnNavigate(FocusNavigationDirection.Right).Run();

		this.enterListener = new(InputAction.Navigate_Enter);
		this.enterListener.Activate = () => this.OnEnter().Run();

		this.backListener = new(InputAction.Navigate_Back);
		this.backListener.Activate = () => this.OnBack().Run();
	}

	public static void AddEnterHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
	{
		if (dependencyObject is not UIElement uiElement)
			return;

		uiElement.AddHandler(EnterEvent, handler);
	}

	public static void RemoveEnterHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
	{
		if (dependencyObject is not UIElement uiElement)
			return;

		uiElement.RemoveHandler(EnterEvent, handler);
	}

	public static void AddBackHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
	{
		if (dependencyObject is not UIElement uiElement)
			return;

		uiElement.AddHandler(BackEvent, handler);
	}

	public static void RemoveBackHandler(DependencyObject dependencyObject, RoutedEventHandler handler)
	{
		if (dependencyObject is not UIElement uiElement)
			return;

		uiElement.RemoveHandler(BackEvent, handler);
	}

	public void Activate()
	{
		this.upListener.Enable();
		this.downListener.Enable();
		this.leftListener.Enable();
		this.rightListener.Enable();
		this.enterListener.Enable();
		this.backListener.Enable();
	}

	public void Deactivate()
	{
		this.upListener.Disable();
		this.downListener.Disable();
		this.leftListener.Disable();
		this.rightListener.Disable();
		this.enterListener.Disable();
		this.backListener.Disable();
	}

	private async Task OnNavigate(FocusNavigationDirection direction)
	{
		await this.scope.MainThread();

		IInputElement focusedControl = FocusManager.GetFocusedElement(this.scope);
		if (focusedControl is UIElement el)
		{
			el.MoveFocus(new TraversalRequest(direction));
		}

		IInputElement newFocusedControl = FocusManager.GetFocusedElement(this.scope);
		Keyboard.Focus(newFocusedControl);
		if (newFocusedControl is UIElement newEl)
		{
			if (newEl is ListBoxItem item)
			{
				item.IsSelected = true;
			}
		}

		Type type = typeof(System.Windows.Input.KeyboardNavigation);
		MethodInfo? showFocusVisual = type.GetMethod("ShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
		showFocusVisual?.Invoke(null, null);
	}

	private async Task OnEnter()
	{
		await this.scope.MainThread();
		IInputElement focusedControl = FocusManager.GetFocusedElement(this.scope);
		if (focusedControl is UIElement el)
		{
			el.RaiseEvent(new RoutedEventArgs(Navigation.EnterEvent));
		}

		MethodInfo? method = null;
		if (focusedControl.GetType().IsAssignableTo(typeof(ButtonBase)))
		{
			method = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		if (method != null)
		{
			method.Invoke(focusedControl, null);
		}
	}

	private async Task OnBack()
	{
		await this.scope.MainThread();
		IInputElement focusedControl = FocusManager.GetFocusedElement(this.scope);
		if (focusedControl is UIElement el)
		{
			el.RaiseEvent(new RoutedEventArgs(Navigation.BackEvent));
		}
	}
}