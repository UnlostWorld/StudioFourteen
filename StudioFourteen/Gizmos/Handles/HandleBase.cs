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

namespace StudioFourteen.Gizmos.Handles;

using StudioFourteen.Gizmos;
using System;
using System.Windows;

public abstract class HandleBase : GizmoBase
{
	public bool IsCursorOver { get; private set; }

	public virtual void StartDrag(Point mousePos)
	{
	}

	public virtual void OnDrag(Vector mouseDelta)
	{
	}

	public virtual void EndDrag()
	{
	}

	public virtual void SetIsCursorOver(bool isCursorOver)
	{
		this.IsCursorOver = isCursorOver;
	}

	public override void HitTest(Point mousePos, ref HandleHitResult result)
	{
		int depth = this.HitTest(mousePos);

		if (depth == int.MinValue)
			return;

		if (depth > result.Depth)
		{
			result.Depth = depth;
			result.Handle = this;
		}
	}

	public abstract int HitTest(Point mousePos);

	public virtual bool OnScrollWheel(float delta)
	{
		return false;
	}
}
