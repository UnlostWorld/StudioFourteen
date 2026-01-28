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
using System.Reflection;
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
		this.Icon = inspect?.IconPath;

		PropertyInfo[] props = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
		foreach (PropertyInfo property in props)
		{
			if (!Attribute.IsDefined(property, typeof(InspectAttribute)))
				continue;

			this.Properties.Add(new(property));
		}
	}

	[ObservableProperty]
	public partial string? Name { get; private set; }

	[ObservableProperty]
	public partial string? Icon { get; private set; }

	[ObservableProperty]
	public partial AvaloniaList<InspectorProperty> Properties { get; set; } = new();
}

public partial class InspectorProperty(PropertyInfo property)
{
	public string Name => property.Name;
}
