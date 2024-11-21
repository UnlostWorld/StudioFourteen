// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Library.Filters;

using System;
using System.Collections.Generic;

public class TypeFilter : FilterBase
{
	private readonly HashSet<Type> types = new();

	public TypeFilter(params Type[] loadTypes)
	{
		foreach(Type type in loadTypes)
		{
			this.types.Add(type);
		}
	}

	public IEnumerable<Type> Types => this.types;

	public override bool IsEmpty => this.types.Count == 0;

	public override void Clear()
	{
		this.types.Clear();
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		foreach (Type type in this.types)
		{
			if (entry.IsType(type))
			{
				return true;
			}
		}

		return false;
	}
}
