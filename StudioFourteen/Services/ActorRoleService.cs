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

namespace StudioFourteen.Services;

using System.Collections.Generic;
using System.Threading.Tasks;

public class ActorRoleService : ServiceBase
{
	private readonly Dictionary<int, string> roles = new();

	public string? GetRoleOrDefault(int objectTableIndex)
	{
		string? nickname = this.GetRole(objectTableIndex);
		if (nickname == null)
			return $"Actor {objectTableIndex}";

		return nickname;
	}

	public string? GetRole(int objectTableIndex)
	{
		string? name;
		this.roles.TryGetValue(objectTableIndex, out name);
		return name;
	}

	public void SetRole(int objectTableIndex, string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			this.roles.Remove(objectTableIndex);
			return;
		}

		if (!this.roles.ContainsKey(objectTableIndex))
			this.roles.Add(objectTableIndex, name);

		this.roles[objectTableIndex] = name;
	}

	public override Task Start()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		return base.Start();
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		this.roles.Remove(objectTableIndex);
	}
}
