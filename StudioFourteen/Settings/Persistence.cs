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

public class Persistence(string persistenceId)
{
	private readonly string persistenceId = persistenceId;
	private readonly Dictionary<string, object?> persistenceCache = [];

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default)
	{
		try
		{
			lock (this.persistenceCache)
			{
				if (this.persistenceCache.TryGetValue(id, out object? value))
				{
					return (T?)value;
				}
			}

			string persistenceId = this.persistenceId + "_" + id;

			if (!ServiceManager.Instance.Settings.Current.Persistence.TryGetValue(persistenceId, out string? json) || json == null)
				return defaultValue;

			if (!json.StartsWith('"') || !json.EndsWith('"'))
				json = '"' + json + '"';

			lock (this.persistenceCache)
			{
				T? value = Serializer.Deserialize<T>(json);
				this.persistenceCache.Add(id, value);
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
			lock (this.persistenceCache)
			{
				this.persistenceCache[id] = value;
			}

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