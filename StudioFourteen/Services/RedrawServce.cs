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
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Files;
using StudioFourteen.Utilities;
using WpfUtils.Extensions;

public class RedrawService : ServiceBase
{
	private readonly Dictionary<int, Request> redraws = new();

	public Request Redraw(int objectTableIndex, bool animate = true)
	{
		lock(this.redraws)
		{
			Request request;
			if (this.redraws.TryGetValue(objectTableIndex, out Request? otherRequest))
			{
				if (!otherRequest.IsDone && otherRequest.IsRunning)
				{
					if (!animate)
						otherRequest.Animate = false;

					return otherRequest;
				}
				else
				{
					request = otherRequest;
					request.Reset();
					request.Animate = animate;
				}
			}
			else
			{
				request = new(objectTableIndex);
				request.Animate = animate;
				this.redraws.Add(objectTableIndex, request);
			}

			return request;
		}
	}

	public async Task RedrawAsync(int objectTableIndex, bool animate = true)
	{
		Request request = this.Redraw(objectTableIndex, animate);
		while (!request.IsDone)
		{
			await Task.Delay(10);
		}
	}

	public bool IsRedrawing(int objectTableIndex)
	{
		if (this.redraws.TryGetValue(objectTableIndex, out Request? otherRequest))
			return !otherRequest.IsDone;

		return false;
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		lock(this.redraws)
		{
			foreach((int objectTableIndex, Request request) in this.redraws)
			{
				if (!request.IsRunning && !request.IsDone)
				{
					request.Begin();
				}
			}
		}
	}

	public class Request(int objectTableIndex)
	{
		private const float FadeOutTimeMs = 150;
		private const float FadeInTimeMs = 250;

		public bool IsRunning { get; private set; }
		public bool IsDone { get; private set; }
		public bool Animate { get; set; }

		public async Task WaitFor()
		{
			while (!this.IsDone)
			{
				await Task.Delay(10);
			}
		}

		public void Begin()
		{
			this.IsRunning = true;
			this.Run().Run();
		}

		public void Reset()
		{
			if (this.IsRunning)
				throw new System.Exception("Attempt to reset a running redraw request");

			this.IsDone = false;
			this.Animate = true;
		}

		private async Task Run()
		{
			this.IsRunning = true;
			this.IsDone = false;

			// Backup pose
			PoseFile file = new();
			await file.Save(objectTableIndex, false, null, true);

			// Backup position
			Vector3 position;
			Quaternion rotation;
			Vector3 scale;
			unsafe
			{
				Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
				position = pCharacter->DrawObject->Position;
				rotation = pCharacter->DrawObject->Rotation;
				scale = pCharacter->DrawObject->Scale;
			}

			Stopwatch sw = new();
			sw.Start();

			while(sw.ElapsedMilliseconds < FadeOutTimeMs && this.Animate)
			{
				await Threads.NextFrame();
				float p = 1 - (sw.ElapsedMilliseconds / FadeOutTimeMs);

				unsafe
				{
					Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
					pCharacter->Alpha = p;
				}
			}

			unsafe
			{
				Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
				pCharacter->Alpha = 0;
			}

			// Clear bone references
			ServiceManager.Instance.Pose.FlushBoneReferences(objectTableIndex);

			// Perform redraw
			await Threads.FrameworkThread();
			unsafe
			{
				Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
				pCharacter->DisableDraw();
				pCharacter->EnableDraw();
			}

			bool isReady = false;
			while (!isReady)
			{
				await Threads.NextFrame();

				unsafe
				{
					Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
					if (!pCharacter->CanDraw())
						continue;
				}

				isReady = true;
			}

			// Restore pose
			await file.Apply(objectTableIndex, false);

			// Restore position
			await Threads.FrameworkThread();
			unsafe
			{
				Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
				pCharacter->DrawObject->Position = position;
				pCharacter->DrawObject->Rotation = rotation;
				pCharacter->DrawObject->Scale = scale;
			}

			sw.Restart();

			// Can't skip the fade in since the games built-in fade will happen
			// no matter what.
			while(sw.ElapsedMilliseconds < FadeInTimeMs)
			{
				await Threads.NextFrame();
				float p = sw.ElapsedMilliseconds / FadeInTimeMs;

				unsafe
				{
					Character* pCharacter = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
					pCharacter->Alpha = p;
				}
			}

			await Threads.NextFrame();

			// TODO: Umbrellas take 500ms to fade in, so detect if this target is holding an umbrella and wait a bit longer.
			this.IsDone = true;
			this.IsRunning = false;
		}
	}
}