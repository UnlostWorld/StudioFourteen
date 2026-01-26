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

namespace StudioFourteen.Services.Portraits;

using System;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using global::Avalonia.Media.Imaging;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Device = SharpDX.Direct3D11.Device;

public partial class Portrait : IDisposable
{
	private readonly int objectIndex = 1;
	private readonly Action<string> callback;

	private int framesToRender = 10;

	private unsafe CharaView* pView;
	private unsafe Texture* pTexture;
	private Texture2D? characterTexture;
	private SaveTexturePass? pass;

	public Portrait(int objectIndex, Action<string> callback)
	{
		this.callback = callback;
		this.objectIndex = objectIndex;
	}

	public bool IsDone { get; private set; }

	public unsafe void Dispose()
	{
		this.pView = null;
		this.pTexture = null;
		this.characterTexture = null;
		this.pass?.Dispose();
	}

	internal unsafe void OnGameTick()
	{
		if (Studio.IsDisposed)
			return;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return;

		Device? device = Studio.Rendering.OverlayRenderer.Device;
		if (device == null)
			return;

		if (this.pView == null)
		{
			this.pView = CharaView.Create();

			if (this.objectIndex != -1)
			{
				Character* pCharacter = (Character*)Studio.Scene.GetXivObject(this.objectIndex);
				this.pView->ModelData.CopyFromCharacter(pCharacter);
			}
			else
			{
				// Set customization options to anything we want. =]
				this.pView->ModelData.CustomizeData.Race = 0;
				this.pView->ModelData.CustomizeData.Sex = 1;
			}

			// Use object Id 1 as its guaranteed to be the current characters minion/mount/whatever,
			//  which wont ever have its own chara view, so we can safely use it for our purposes.
			this.pView->Initialize(null, 1, 0);
			this.pTexture = pRenderTargetManager->Base.GetCharaViewTexture(1);

			if (this.pTexture == null || (nint)this.pTexture->D3D11Texture2D == 0)
			{
				Studio.Log.Error(new Exception("Failed to create chara view texture"), "Error generating portrait");
				this.IsDone = true;
			}

			this.characterTexture = new((nint)pTexture->D3D11Texture2D);

			this.pass = new(this.characterTexture);
			Studio.Rendering.OverlayRenderer.AddAfterEffectsPass(this.pass);
		}

		this.pView->SetCameraDistance(-1.9f);

		// TODO: Change based on character height
		this.pView->SetCameraXAndY(0.0f, -29.0f);

		this.pView->Render(1);
		this.framesToRender--;

		if (this.framesToRender > 0)
			return;

		if (this.characterTexture == null)
			return;

		Image? image = this.pass?.Image;
		if (image == null)
			return;

		if (this.pass == null)
			return;

		Studio.Rendering.OverlayRenderer.RemoveAfterEffectsPass(this.pass);
		this.pass.Dispose();
		this.pass = null;

		// TODO: Use a hash of the appearance?
		string name = $"{this.objectIndex}";
		string dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/StudioFourteen/Portraits/";
		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		string path = $"{dir}{name}.png";

		PngEncoder encoder = new()
		{
			ColorType = PngColorType.RgbWithAlpha,
			TransparentColorMode = PngTransparentColorMode.Preserve,
			CompressionLevel = PngCompressionLevel.BestSpeed,
		};

		image.SaveAsPng(path, encoder);
		image.Dispose();

		this.IsDone = true;
		this.callback.Invoke(path);

		Studio.Log.Information("generated portrait");
	}
}