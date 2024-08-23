namespace ScreenshotStudio.Services;

using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterNicknameService : ServiceBase
{
	private readonly Dictionary<int, string> nicknames = new();

	public string? GetNicknameOrDefault(int objectTableIndex)
	{
		string? nickname = this.GetNickname(objectTableIndex);
		if (nickname == null)
			return $"Actor {objectTableIndex}";

		return nickname;
	}

	public string? GetNickname(int objectTableIndex)
	{
		string? name;
		this.nicknames.TryGetValue(objectTableIndex, out name);
		return name;
	}

	public void SetNickname(int objectTableIndex, string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			this.nicknames.Remove(objectTableIndex);
			return;
		}

		if (!this.nicknames.ContainsKey(objectTableIndex))
			this.nicknames.Add(objectTableIndex, name);

		this.nicknames[objectTableIndex] = name;
	}

	public override Task Start()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		return base.Start();
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		this.nicknames.Remove(objectTableIndex);
	}
}
