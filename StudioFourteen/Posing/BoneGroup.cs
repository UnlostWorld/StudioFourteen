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

namespace StudioFourteen.Posing;

using System;
using System.Collections.Generic;
using System.Numerics;

public class SimpleViewLayout
{
	public string? Background { get; set; }
	public Dictionary<string, Vector2> Bones { get; set; } = new();
	public string? BasedOn { get; set; }

	public void MergeBasedOn(SimpleViewLayout? parent)
	{
		if (parent == null)
			return;

		if (this.Background == null)
			this.Background = parent.Background;

		foreach ((string key, Vector2 pos) in parent.Bones)
		{
			if (this.Bones.ContainsKey(key))
				continue;

			this.Bones.Add(key, pos);
		}
	}
}

#pragma warning disable
public class BoneGroup
{
	private readonly SimpleViewLayout? layout;

	public string Name { get; set; } = "Unknown";
	public HashSet<string> Bones { get; set; } = new();
	public string? SimpleLayout { get; set; }

	public SimpleViewLayout? GetSimpleViewLayout()
	{
		if (this.SimpleLayout == null)
			return null;

		if (this.layout == null)
			throw new NotImplementedException();
		////ServiceManager.Instance.Content.SimplePoseLayouts?.TryGetValue(this.SimpleLayout, out this.layout);

		return this.layout;
	}
}