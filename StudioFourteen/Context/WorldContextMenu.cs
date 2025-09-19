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

namespace StudioFourteen.Context;

using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Serilog;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using StudioFourteen;
using StudioFourteen.Controls;
using StudioFourteen.Extensions;

[DependencyProperty<string>("ObjectName")]
public partial class WorldContextMenu : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();

	private HitInfo? currentHitInfo;

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public bool IsObject => this.ObjectTableIndex != -1;
	public int ObjectTableIndex => this.currentHitInfo?.ObjectTableIndex ?? -1;
	public Vector3 Position => this.currentHitInfo?.Position ?? Vector3.Zero;

	public Task Show(Point screenPosition)
	{
		return this.Show(new Vector2((float)screenPosition.X, (float)screenPosition.Y));
	}

	public async Task Show(Vector2 screenPosition)
	{
		await this.MainThread();
		this.Menus.Clear();

		string? name = null;
		await TickService.GameTick();

		unsafe
		{
			this.currentHitInfo = RayCast.Cast(screenPosition);

			if (this.currentHitInfo.ObjectTableIndex != -1)
			{
				name = this.currentHitInfo.GameObject->NameString;
			}
			else
			{
				name = this.currentHitInfo.Position.ToString("N0").Replace("<", string.Empty).Replace(">", string.Empty);
			}
		}

		await this.MainThread();

		this.PlacementRectangle = new Rect(screenPosition.X, screenPosition.Y + 25, 1, 1);
		this.IsOpen = true;

		this.ObjectName = name;
	}
}