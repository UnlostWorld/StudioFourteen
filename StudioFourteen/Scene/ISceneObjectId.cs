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

namespace StudioFourteen.Scene;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class ISceneObjectId : IEquatable<ISceneObjectId?>
{
	public static bool operator ==(ISceneObjectId? left, ISceneObjectId? right)
	{
		return EqualityComparer<ISceneObjectId>.Default.Equals(left, right);
	}

	public static bool operator !=(ISceneObjectId? left, ISceneObjectId? right)
	{
		return !(left == right);
	}

	public abstract SceneObjectBase? Create();

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as ISceneObjectId);
	}

	public bool Equals(ISceneObjectId? other)
	{
		return other is not null && this.GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}
}

public abstract class IAsyncSceneObjectId : ISceneObjectId
{
	public abstract Task<SceneObjectBase?> CreateAsync();

	public sealed override SceneObjectBase? Create() => throw new NotSupportedException();
}