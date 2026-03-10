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
using System.Security.Cryptography;

public abstract class ContentReference(string path) : IDisposable
{
	public readonly string Path = path;

	public delegate void ReloadDelegate();

	public event ReloadDelegate? Reloaded;

	public DateTime LastLoadTimeUtc { get; set; }

	public virtual void Reload()
	{
		this.Reloaded?.Invoke();
	}

	public virtual void Dispose()
	{
	}
}

public abstract class ContentReference<T>(string path)
	: ContentReference(path), IContent<T>
{
	private byte[]? lastHash;
	private T? instance;
	private T? lastInstance;
	public bool IsLoaded => this.instance != null;

	public sealed override void Reload()
	{
		// Calculate file hash to verify its actually changed.
		using Stream stream = Studio.Content.GetContent(this);
		using (SHA256 sha256Hash = SHA256.Create())
		{
			byte[] hashBytes = sha256Hash.ComputeHash(stream);
			if (this.lastHash != null && hashBytes.SequenceEqual(this.lastHash))
			{
				return;
			}

			this.lastHash = hashBytes;
		}

		Studio.Log.Information($"Reloading file: {this.Path}");

		if (this.instance is IDisposable disposable)
		{
			disposable.Dispose();
		}
		else
		{
			this.lastInstance = this.instance;
		}

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
					Studio.Log.Warning(ex, $"Error reloading content: \"{this.Path}\"");
				}
				else
				{
					throw new Exception($"Failed to load content: \"{this.Path}\"", ex);
				}
			}
		}

		return this.instance;
	}

	public override void Dispose()
	{
		base.Dispose();

		if (this.instance is IDisposable disposable)
		{
			disposable.Dispose();
		}

		this.instance = default;

		if (this.lastInstance is IDisposable lastDisposable)
		{
			lastDisposable.Dispose();
		}

		this.lastInstance = default;
	}

	protected abstract T Load(Stream stream);
}
