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

namespace System.Reflection;

using System;

public static class ReflectionExtensions
{
	public static T Property<T>(this object? self, string propertyName)
	{
		object? obj = self.Property(propertyName);
		if (obj is T tObj)
			return tObj;

		throw new Exception("Property returned incorrect type");
	}

	public static object? Property(this object? self, string name)
	{
		if (self == null)
			throw new Exception("Object was null");

		Type? type = self.GetType();
		PropertyInfo? info = null;

		while (type != null && info == null)
		{
			info = type.GetProperty(
				name,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (info == null)
			{
				type = type.BaseType;
			}
		}

		if (info == null)
			throw new Exception($"Failed to locate property {name} on Type: {self.GetType()}");

		return info.GetValue(self);
	}

	public static T Field<T>(this object? self, string fieldName)
	{
		object? obj = self.Field(fieldName);
		if (obj is T tObj)
			return tObj;

		throw new Exception("Field returned incorrect type");
	}

	public static object? Field(this object? self, string name)
	{
		if (self == null)
			throw new Exception("Object was null");

		Type? type = self.GetType();
		FieldInfo? info = null;

		while (type != null && info == null)
		{
			info = type.GetField(
				name,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			if (info == null)
			{
				type = type.BaseType;
			}
		}

		if (info == null)
			throw new Exception($"Failed to locate field {name} on Type: {self.GetType()}");

		return info.GetValue(self);
	}
}