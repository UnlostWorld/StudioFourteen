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

using StudioFourteen.Posing;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;

public class PrimitiveGroup : IPrimitive
{
	public readonly List<IPrimitive> Children = new();
	public Transform Transform = Transform.Identity;
	public bool KeepScreenSize = false;

	public PrimitiveGroup? Parent { get; set; }

	public virtual void Enable(Canvas canvas)
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Parent = this;
			primitive.Enable(canvas);
		}
	}

	public virtual void Disable(Canvas canvas)
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Disable(canvas);
		}
	}

	public virtual void Update(Matrix4x4 view, Matrix4x4 projection)
	{
		foreach (IPrimitive primitive in this.Children)
		{
			primitive.Update(view, projection);
		}
	}

	public Transform GetTransform()
	{
		if (this.Parent != null)
			return this.Parent.GetTransform() * this.Transform;

		return this.Transform;
	}

	public bool GetKeepScreenSize()
	{
		if (this.Parent != null)
			return this.Parent.GetKeepScreenSize() || this.KeepScreenSize;

		return this.KeepScreenSize;
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
