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

namespace StudioFourteen.Overlays.Primitives;

using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;

public class PrimitiveGroup : IPrimitive
{
	public readonly List<IPrimitive> Children = new();

	public void Enable(Canvas canvas)
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Enable(canvas);
		}
	}

	public void Disable(Canvas canvas)
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Disable(canvas);
		}
	}

	public void Update()
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Update();
		}
	}

	protected T AddChild<T>()
		where T : IPrimitive, new()
	{
		T primitive = new T();
		this.Children.Add(primitive);
		return primitive;
	}

	protected void AddChild(IPrimitive primitive)
	{
		this.Children.Add(primitive);
	}
}
