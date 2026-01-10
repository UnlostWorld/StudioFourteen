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

namespace StudioFourteen.Services.Content;

using System;
using System.IO;

public abstract class ContentReference(string path)
{
	public readonly string Path = path;

	public delegate void ReloadDelegate();

	public event ReloadDelegate? OnReloaded;

	public DateTime LastLoadTimeUtc { get; set; }

	public virtual void Reload()
	{
		this.OnReloaded?.Invoke();
	}
}

public abstract class ContentReference<T>(string path)
	: ContentReference(path), IContent<T>
{
	private T? instance;
	private T? lastInstance;
	public bool IsLoaded => this.instance != null;

	public sealed override void Reload()
	{
		this.lastInstance = this.instance;
		this.instance = default;
		base.Reload();
	}

	public T Get()
	{
		if (this.instance == null)
		{
			try
			{
				using Stream stream = Studio.Content.GetContent(this);
				this.instance = this.Load(stream);
			}
			catch (Exception ex)
			{
				if (this.lastInstance != null)
				{
					this.instance = this.lastInstance;
					Studio.Log.Warning(ex, "Error reloading content");
				}
				else
				{
					throw;
				}
			}
		}

		return this.instance;
	}

	protected abstract T Load(Stream stream);
}
