namespace StudioFourteen.Studio;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System.Threading.Tasks;
using System.Windows;

public partial class BackgroundWindow : PanelWindow
{
	public static BackgroundWindow? Instance;

	public BackgroundWindow()
	{
		Instance = this;
		this.ContentArea.DataContext = this;
	}

	[AutoNotify]
	public bool IsFullyLoaded
	{
		get
		{
			return this.Services.CurrentState == ServiceManagerBase.States.Started;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();
		XivWindow.Activate();
	}

	private void UpdatePosition()
	{
		if (XivWindow.Process == null)
			return;

		this.Width = XivWindow.Size.Width;
		this.Height = XivWindow.Size.Height - XivWindow.TitleBarHeight;

		XivWindow.SetPosition(this, new(0, 0));
	}

	private void OnStudioClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.Services.Studio.CloseStudio();
		}
		else
		{
			this.Services.Studio.OpenStudio();
		}
	}

	private void OnShutdownClicked(object sender, RoutedEventArgs e)
	{
		Task.Run(this.Services.Stop);
	}
}
