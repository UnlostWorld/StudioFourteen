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

namespace StudioFourteen.Services.Library;

using System;
using System.Threading.Tasks;

public abstract class LibraryPreviewBase
{
	private bool isStarting = false;
	private bool isStopping = false;
	private LibraryEntryBase? entry;

	public bool HasStopped { get; private set; }
	public bool HasStarted { get; private set; }

	public bool IsEntry(LibraryEntryBase entry)
	{
		return this.entry == entry;
	}

	public void StartPreview(LibraryEntryBase entry, LibraryPreviewBase? other)
	{
		if (this.HasStarted)
			return;

		this.entry = entry;
		this.StartPreviewAsync(other).RunAsynchronously();
	}

	public async Task StartPreviewAsync(LibraryPreviewBase? other)
	{
		while (this.isStopping)
			await Task.Delay(33);

		if (other != null)
		{
			if (!other.HasStopped && other.GetType() != this.GetType())
			{
				await other.StopPreviewAsync();
			}

			while (other.isStopping)
			{
				await Task.Delay(33);
			}
		}

		this.isStarting = true;

		await this.Start(other);
		this.isStarting = false;
		this.HasStarted = true;
	}

	public void StopPreview()
	{
		this.StopPreviewAsync().RunAsynchronously();
	}

	public async Task StopPreviewAsync()
	{
		this.isStopping = true;

		while (this.isStarting)
			await Task.Delay(33);

		try
		{
			await this.Stop();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error stopping library preview");
		}
		finally
		{
			this.isStopping = false;
			this.HasStopped = true;
			this.HasStarted = false;
		}
	}

	protected abstract Task Start(LibraryPreviewBase? other);
	protected abstract Task Stop();
}