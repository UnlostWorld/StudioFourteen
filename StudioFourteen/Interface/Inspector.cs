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

namespace StudioFourteen.Interface;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using StudioFourteen.Services.Avalonia;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Tick;

public partial class Inspector : WindowReference
{
	[ObservableProperty] private AvaloniaList<InspectorGroup> groups = new();
	[ObservableProperty] private SceneObjectBase? target;

	public Inspector()
		: base("UI/Inspector.ui")
	{
		Studio.Scene.ObjectSelected += this.OnObjectSelected;
	}

	private void OnObjectSelected(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Target = obj;
			this.Groups.Clear();

			Type? type = obj.GetType();
			while (type != null)
			{
				InspectAttribute? inspect = type.GetCustomAttribute<InspectAttribute>(false);
				if (inspect != null)
				{
					this.Groups.Add(new(obj, type));
				}

				type = type.BaseType;
			}
		});
	}
}

public partial class InspectorGroup
		 : ObservableObject
{
	public InspectorGroup(SceneObjectBase target, Type type)
	{
		InspectAttribute? inspect = type.GetCustomAttribute<InspectAttribute>();

		this.Name = type.Name;
		this.Icon = inspect?.Path;

		PropertyInfo[] props = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
		foreach (PropertyInfo property in props)
		{
			if (!Attribute.IsDefined(property, typeof(InspectAttribute)))
				continue;

			this.Properties.Add(new(target, property));
		}
	}

	[ObservableProperty]
	public partial string? Name { get; private set; }

	[ObservableProperty]
	public partial string? Icon { get; private set; }

	[ObservableProperty]
	public partial AvaloniaList<InspectorProperty> Properties { get; set; } = new();
}

public partial class InspectorProperty : ObservableObject
{
	private readonly PropertyInfo property;
	private readonly INotifyPropertyChanged target;
	private readonly AvaloniaContentReference<Visual>? inspectorReference;

	public InspectorProperty(INotifyPropertyChanged target, PropertyInfo property)
	{
		this.target = target;
		this.target.PropertyChanged += this.OnTargetPropertyChanged;

		this.property = property;

		InspectAttribute? attribute = property.GetCustomAttribute<InspectAttribute>();
		if (attribute == null)
			return;

		if (string.IsNullOrEmpty(attribute.Path))
			return;

		this.inspectorReference = new(attribute.Path);
		this.inspectorReference.Reloaded += this.OnInspectorReloaded;

		this.Inspector = this.inspectorReference.Get();
		this.Inspector.DataContext = this;
	}

	public string Name => this.property.Name;

	[ObservableProperty]
	public partial Visual? Inspector { get; private set; }

	public object? Value
	{
		get => this.property.GetValue(this.target);
		set => this.property.SetValue(this.target, value);
	}

	private void OnInspectorReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Inspector = this.inspectorReference?.Get();
			this.Inspector?.DataContext = this;
		});
	}

	private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == this.property.Name)
		{
			this.OnPropertyChanged(nameof(InspectorProperty.Value));
		}
	}
}
