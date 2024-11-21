// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Files;

using StudioFourteen.Services;
using StudioFourteen.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using StudioFourteen.Images;
using SixLabors.ImageSharp.Formats.Png;

public class FileThumbnailService : ServiceBase
{
	private readonly ConcurrentQueue<ThumbnailRequest> requests = new();

	public override Task Start()
	{
		Thread generatorThread = new(new ThreadStart(this.ThumbnailGeneratorThread));
		generatorThread.Start();

		return base.Start();
	}

	public void GetThumbnail(FileInfo fileInfo, Action<string> callback)
	{
		string name = this.HashName(fileInfo);
		string dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/StudioFourteen/Thumbnails/";

		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		string path = $"{dir}{name}.png";

		if (File.Exists(path))
		{
			callback.Invoke(path);
		}
		else
		{
			this.requests.Enqueue(new(fileInfo, callback, path));
		}
	}

	private string HashName(FileInfo fileInfo) => HashUtility.GetHashString(fileInfo.FullName);

	private void ThumbnailGeneratorThread()
	{
		while(this.IsAlive)
		{
			if (this.requests.Count <= 0)
			{
				Thread.Sleep(100);
				continue;
			}

			if (!this.requests.TryDequeue(out ThumbnailRequest request))
				continue;

			try
			{
				this.ProcessRequest(request);
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, "Failed to generate thumbnail");
			}
		}
	}

	private void ProcessRequest(ThumbnailRequest request)
	{
		FileTypeInfoBase? typeInfo = this.Services.Files.GetTypeInfo(request.FileInfo);
		if (typeInfo == null)
			throw new Exception($"No File Type Info for file: {request.FileInfo}");

		FileBase? file = typeInfo.Load(request.FileInfo);
		if (file == null)
			throw new Exception($"Failed to load file: {request.FileInfo}");

		if (file.Base64Image == null)
			return;

		byte[] binaryData = Convert.FromBase64String(file.Base64Image);

		PngEncoder encoder = new()
		{
			ColorType = PngColorType.RgbWithAlpha,
			TransparentColorMode = PngTransparentColorMode.Preserve,
			CompressionLevel = PngCompressionLevel.BestSpeed,
		};

		using (Image image = Image.Load<Rgba32>(binaryData))
		{
			ResizeOptions op = new();
			op.Mode = ResizeMode.Max;
			op.Size = new(128, 128);
			op.PremultiplyAlpha = false;
			image.Mutate(x => x.Resize(op));
			image.Mutate(x => x.ApplyRoundedCorners(8));

			image.SaveAsPng(request.ThumbnailPath, encoder);
		}

		request.Callback.Invoke(request.ThumbnailPath);
	}

	public struct ThumbnailRequest(FileInfo fileInfo, Action<string> callback, string path)
	{
		public readonly FileInfo FileInfo = fileInfo;
		public readonly Action<string> Callback = callback;
		public readonly string ThumbnailPath = path;
	}
}
