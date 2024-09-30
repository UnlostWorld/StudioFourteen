// WinTabControl
// https://github.com/DennisWacom/WintabControl/blob/master/WintabControl/MainWindow.xaml.cs

namespace ScreenshotStudio.Tablet;

using Apos.WintabDN;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

public class TabletService : ServiceBase
{
	private readonly double maxPressure = CWintabInfo.GetMaxPressure();

	private CWintabContext? context = null;
	private CWintabData? data = null;

	public double PenPressure { get; private set; }

	public override async Task Start()
	{
		await base.Start();

		if (!CWintabInfo.IsWintabAvailable())
			return;

		this.Log.Information("WinTab is available");
		this.Log.Information("Stylus name (pen):" + CWintabInfo.GetStylusName(EWTICursorNameIndex.CSR_NAME_PRESSURE_STYLUS));

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

			if (XivWindow.Hwnd == null)
				return;

			HWND hwnd = new HWND((nint)XivWindow.Hwnd);
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
		while(this.data != null && this.IsAlive)
		{
			try
			{
				uint packetCount = 0;
				WintabPacket[] packets = this.data.GetDataPackets(100, true, ref packetCount);

				for (int i = 0; i < packetCount; i++)
				{
					this.PenPressure = packets[i].pkNormalPressure / this.maxPressure;
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
