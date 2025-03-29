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

namespace StudioFourteen.Interop;

using Dalamud.Hooking;
using System;

public abstract class HookBase<TDelegate>
	where TDelegate : Delegate
{
	private Hook<TDelegate>? hook;

	public TDelegate Original
	{
		get
		{
			if (this.hook == null)
				throw new Exception("Attempt to get hook original before the hook as been enabled");

			return this.hook.Original;
		}
	}

	public virtual void Enable(TDelegate detour)
	{
		if (this.hook == null)
			this.hook = this.Create(detour);

		if (this.hook == null)
			return;

		this.hook.Enable();
	}

	public virtual void Disable()
	{
		if (this.hook == null)
			return;

		if (this.hook.IsEnabled)
		{
			this.hook.Disable();
		}

		if (!this.hook.IsDisposed)
		{
			this.hook.Dispose();
		}
	}

	public void Destroy()
	{
		this.Disable();
		this.hook = null;
	}

	protected abstract Hook<TDelegate>? Create(TDelegate detour);
}
