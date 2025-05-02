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

namespace StudioFourteen.Settings;

using StudioFourteen.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Persistence
{
	private static readonly Dictionary<string, Persistence> PersistenceObjectCache = new();
	private readonly Dictionary<string, object?> valueCache = [];
	private readonly string persistenceId;

	private Persistence(string persistenceId)
	{
		this.persistenceId = persistenceId;
	}

	public delegate void PersistenceDelegate(Persistence persistence);

	public event PersistenceDelegate? PersistenceChanged;

	public static Persistence GetPersistence(string persistenceId)
	{
		if (PersistenceObjectCache.TryGetValue(persistenceId, out var persistence))
			return persistence;

		Persistence newPersistence = new(persistenceId);
		PersistenceObjectCache[persistenceId] = newPersistence;
		return newPersistence;
	}

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default)
	{
		try
		{
			lock (this.valueCache)
			{
				if (this.valueCache.TryGetValue(id, out object? value))
				{
					return (T?)value;
				}
			}

			string persistenceId = this.persistenceId + "_" + id;

			if (!ServiceManager.Instance.Settings.Current.Persistence.TryGetValue(persistenceId, out string? json) || json == null)
				return defaultValue;

			if (!json.StartsWith('"') || !json.EndsWith('"'))
				json = '"' + json + '"';

			lock (this.valueCache)
			{
				T? value = Serializer.Deserialize<T>(json);
				this.valueCache.Add(id, value);
				return value;
			}
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error in persistence");
			return defaultValue;
		}
	}

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.SetPersistence(id, value);
	}

	public void SetPersistence(string id, object? value)
	{
		try
		{
			lock (this.valueCache)
			{
				this.valueCache[id] = value;
			}

			this.PersistenceChanged?.Invoke(this);

			string persistenceId = this.persistenceId + "_" + id;

			if (value != null)
			{
				ServiceManager.Instance.Settings.Current.Persistence[persistenceId] = Serializer.Serialize(value);
			}
			else
			{
				ServiceManager.Instance.Settings.Current.Persistence.Remove(persistenceId);
			}

			ServiceManager.Instance.Settings.Save();
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error in persistence");
		}
	}
}