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

namespace StudioFourteen.Rendering.Draw.Handles;

using Serilog;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WpfUtils.Windows;

public partial class HandleTipWindow : MultithreadedWindow
{
	protected readonly ILogger Log;

	private readonly TextBlock textBlock;
	private Handle? handle;

	public HandleTipWindow()
	{
		this.Log = Logging.ForContext(this.GetType());

		this.WindowStartupLocation = WindowStartupLocation.Manual;
		this.Resources = StudioFourteen.Resources.Load();
		this.AllowsTransparency = true;
		this.Background = new SolidColorBrush(Colors.Transparent);
		this.ResizeMode = ResizeMode.NoResize;
		this.UseLayoutRounding = true;
		this.WindowStyle = WindowStyle.None;
		this.SizeToContent = SizeToContent.Manual;
		this.Width = 150;
		this.Height = 50;

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);

		this.Loaded += this.OnLoaded;

		this.textBlock = new();
		this.textBlock.Text = "Hello World";
		this.textBlock.HorizontalAlignment = HorizontalAlignment.Center;
		this.textBlock.VerticalAlignment = VerticalAlignment.Center;
		this.textBlock.Foreground = new SolidColorBrush(Colors.White);
		this.textBlock.FontWeight = FontWeights.Bold;

		DropShadowEffect shadow = new();
		shadow.ShadowDepth = 0;
		this.textBlock.Effect = shadow;

		this.Content = this.textBlock;
	}

	public ServiceManager Services => ServiceManager.Instance;

	public void Show(Handle handle)
	{
		this.handle = handle;
		this.Dispatcher.Invoke(this.Show);
	}

	public void Hide(Handle? handle)
	{
		this.handle = null;
		this.Dispatcher.Invoke(this.Hide);
	}

	public void Update(bool visible, string content, Vector3 worldPosition)
	{
		if (this.handle == null)
			return;

		this.Dispatcher.Invoke(() =>
		{
			this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			this.textBlock.Text = content;

			Vector3 pos = this.Services.Camera.WorldToCamera(worldPosition);
			this.Services.Windows.SetPosition(this, new System.Windows.Point(pos.X, pos.Y), false);
			this.Services.Windows.SendToBack(this);
		});
	}

	protected void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Services.Windows.OnWindowOpening(this);
		this.Services.Windows.Embed(this);
	}
}