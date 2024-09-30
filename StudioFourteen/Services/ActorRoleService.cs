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
