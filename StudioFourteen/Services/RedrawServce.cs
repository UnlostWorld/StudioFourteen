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
using StudioFourteen.Posing;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Utilities;
using StudioFourteen.Extensions;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class RedrawService : ServiceBase
{
	private readonly Dictionary<Character, Request> redraws = new();

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.StudioTick, this.OnTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.StudioTick, this.OnTick);
		base.Detach();
	}

	public Request Redraw(Character objectTableIndex, bool animate = true)
	{
		lock (this.redraws)
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

	public async Task RedrawAsync(Character character, bool animate = true)
	{
		Request request = this.Redraw(character, animate);
		while (!request.IsDone)
		{
			await Task.Delay(10);
		}
	}

	public bool IsRedrawing(Character character)
	{
		if (this.redraws.TryGetValue(character, out Request? otherRequest))
			return !otherRequest.IsDone;

		return false;
	}

	protected void OnTick()
	{
		lock (this.redraws)
		{
			foreach ((Character character, Request request) in this.redraws)
			{
				if (!request.IsRunning && !request.IsDone)
				{
					request.Begin();
				}
			}
		}
	}

	public class Request(Character character)
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
			this.Run().RunAsynchronously();
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
			PoseFile file = await character.ExportPoseAsync(false, null, true);

			// Backup position
			Vector3 position;
			Quaternion rotation;
			Vector3 scale;
			bool doFadeOut;
			unsafe
			{
				XivCharacter* pCharacter = character.GetXivCharacter();
				position = pCharacter->DrawObject->Position;
				rotation = pCharacter->DrawObject->Rotation;
				scale = pCharacter->DrawObject->Scale;

				doFadeOut = this.Animate && pCharacter->Alpha > 0.01f;
			}

			Stopwatch sw = new();

			if (doFadeOut)
			{
				sw.Start();

				while (sw.ElapsedMilliseconds < FadeOutTimeMs)
				{
					await Threads.NextFrame();
					float p = 1 - (sw.ElapsedMilliseconds / FadeOutTimeMs);

					unsafe
					{
						XivCharacter* pCharacter = character.GetXivCharacter();
						pCharacter->Alpha = p;
					}
				}

				unsafe
				{
					XivCharacter* pCharacter = character.GetXivCharacter();
					pCharacter->Alpha = 0;
				}
			}

			// Clear bone references
			// TODO!
			////ServiceManager.Instance.Pose.FlushBoneReferences(objectTableIndex);

			// Perform redraw
			await TickService.GameTick();
			unsafe
			{
				XivCharacter* pCharacter = character.GetXivCharacter();
				pCharacter->DisableDraw();
				pCharacter->EnableDraw();
			}

			bool isReady = false;
			while (!isReady)
			{
				await Threads.NextFrame();

				if (!character.CanDraw())
					continue;

				isReady = true;
			}

			// Restore pose
			await character.ImportPose(file, UpdateSource.Restore, false);

			// Restore position
			await TickService.GameTick();
			unsafe
			{
				XivCharacter* pCharacter = character.GetXivCharacter();
				pCharacter->DrawObject->Position = position;
				pCharacter->DrawObject->Rotation = rotation;
				pCharacter->DrawObject->Scale = scale;
			}

			sw.Restart();

			// Can't skip the fade in since the games built-in fade will happen
			// no matter what.
			while (sw.ElapsedMilliseconds < FadeInTimeMs)
			{
				await Threads.NextFrame();
				float p = sw.ElapsedMilliseconds / FadeInTimeMs;

				unsafe
				{
					XivCharacter* pCharacter = character.GetXivCharacter();
					pCharacter->Alpha = p;
				}
			}

			await Threads.NextFrame();

			unsafe
			{
				XivCharacter* pCharacter = character.GetXivCharacter();
				pCharacter->Alpha = 1.0f;
			}

			// TODO: Umbrellas take 500ms to fade in, so detect if this target is holding an umbrella and wait a bit longer.
			this.IsDone = true;
			this.IsRunning = false;
		}
	}
}