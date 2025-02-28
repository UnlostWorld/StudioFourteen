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
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Files;
using StudioFourteen.Utilities;
using WpfUtils.Extensions;

public class RedrawService : ServiceBase
{
	private const float FadeOutTimeMs = 150;
	private const float FadeInTimeMs = 250;

	private readonly Dictionary<int, RedrawPhases> redraws = new();

	public enum RedrawPhases
	{
		Pending,
		FadeOut,
		Redraw,
		FadeIn,
		Done,
	}

	public void Redraw(int objectTableIndex)
	{
		this.redraws[objectTableIndex] = RedrawPhases.Pending;
	}

	public bool IsPendingRedraw(int objectTableIndex)
	{
		return this.redraws.ContainsKey(objectTableIndex);
	}

	public async Task WaitForRedraw(int objectTableIndex)
	{
		while (this.redraws.ContainsKey(objectTableIndex))
		{
			await Task.Delay(10);
		}
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		foreach ((int objectTableIndex, RedrawPhases phase) in this.redraws)
		{
			if (phase == RedrawPhases.Pending)
			{
				this.DoRedraw(objectTableIndex).Run();
			}
		}

		this.redraws.Clear();
	}

	private async Task DoRedraw(int objectTableIndex)
	{
		this.redraws[objectTableIndex] = RedrawPhases.FadeOut;

		// Backup pose
		PoseFile file = new();
		await file.Save(objectTableIndex, false, null, true);

		Stopwatch sw = new();
		sw.Start();

		while(sw.ElapsedMilliseconds < FadeOutTimeMs)
		{
			await Threads.NextFrame();
			float p = 1 - (sw.ElapsedMilliseconds / FadeOutTimeMs);

			unsafe
			{
				Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);
				pCharacter->Alpha = p;
			}
		}

		// Clear bone references
		this.Services.Pose.FlushBoneReferences(objectTableIndex);

		this.redraws[objectTableIndex] = RedrawPhases.Redraw;

		// Perform redraw
		await Threads.FrameworkThread();
		unsafe
		{
			Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);
			pCharacter->DisableDraw();
			pCharacter->EnableDraw();
		}

		bool isReady = false;
		while (!isReady)
		{
			await Threads.NextFrame();

			unsafe
			{
				Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);
				if (!pCharacter->CanDraw())
					continue;
			}

			isReady = true;
		}

		await this.WaitForRedraw(objectTableIndex);

		// Restore pose
		await file.Apply(objectTableIndex, false);

		this.redraws[objectTableIndex] = RedrawPhases.FadeIn;

		sw.Restart();
		while(sw.ElapsedMilliseconds < FadeInTimeMs)
		{
			await Threads.NextFrame();
			float p = sw.ElapsedMilliseconds / FadeInTimeMs;

			unsafe
			{
				Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);
				pCharacter->Alpha = p;
			}
		}

		this.redraws[objectTableIndex] = RedrawPhases.Done;
	}
}