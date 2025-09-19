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

namespace StudioFourteen.Tablet;

using Apos.WintabDN;
using StudioFourteen.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

[Service]
public class TabletService : ServiceBase
{
	private CWintabContext? context = null;
	private CWintabData? data = null;

	public double PenPressure { get; private set; }
	private double MaxPressure { get; set; }

	public override async Task Start()
	{
		await base.Start();

		try
		{
			if (!CWintabInfo.IsWintabAvailable())
				return;
		}
		catch (Exception)
		{
			return;
		}

		this.Log.Information("WinTab is available");
		this.Log.Information("Stylus name (pen):" + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PRESSURE_STYLUS));

		this.MaxPressure = CWintabInfo.GetMaxPressure();

		this.OpenSystemContext();
	}

	public override Task Stop()
	{
		this.CloseCurrentContext();
		return base.Stop();
	}

	private void OpenSystemContext()
	{
		try
		{
			this.context = CWintabInfo.GetDefaultSystemContext();

			if (this.context == null)
				return;

			this.context.Name = "System Context";

			if (this.Services.Windows.XivWindowHwnd == null)
				return;

			HWND hwnd = new HWND((nint)this.Services.Windows.XivWindowHwnd);
			HCTX result = this.context.Open(hwnd, true);

			if (result == IntPtr.Zero)
			{
				this.Log.Error("Failed to open wintab context");
				return;
			}

			this.data = new CWintabData(this.context);

			Thread packetThread = new(new ThreadStart(this.PacketThread));
			packetThread.Start();
		}
		catch (Exception ex)
		{
			System.Windows.Forms.MessageBox.Show(ex.Message);
		}
	}

	private void CloseCurrentContext()
	{
		try
		{
			if (this.context != null)
			{
				this.context.Close();
				this.context = null;
				this.data = null;
			}
		}
		catch (Exception ex)
		{
			System.Windows.Forms.MessageBox.Show(ex.ToString());
		}
	}

	private void PacketThread()
	{
		while (this.data != null && this.IsAlive)
		{
			try
			{
				uint packetCount = 0;
				WintabPacket[] packets = this.data.GetDataPackets(100, true, ref packetCount);

				for (int i = 0; i < packetCount; i++)
				{
					this.PenPressure = packets[i].pkNormalPressure / this.MaxPressure;
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error processing wintab packet");
			}

			Thread.Sleep(10);
		}
	}
}
