namespace StudioFourteen.Library;

using DependencyPropertyGenerator;
using Serilog;
using StudioFourteen.Library.Results;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils;
using WpfUtils.Controls;
using WpfUtils.Utils;

[DependencyProperty<ILibraryEntry>("Entry")]
[DependencyProperty<bool>("IsExpanded")]
public partial class LibraryContextMenu : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();

	private readonly FuncQueue openQueue;
	private UIElement? placementTarget;
	private Result? currentResult;

	public LibraryContextMenu()
	{
		this.openQueue = new(this.ShowResultMenu, 250);
	}

	public ServiceManager Services => ServiceManager.Instance;

	public void Enter(Result result, FrameworkElement placementTarget)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.currentResult = result;
		this.IsExpanded = false;
		this.StaysOpen = true;
		this.openQueue.Invoke();
	}

	public void Leave(Result result)
	{
		if (this.currentResult == result)
		{
			this.openQueue.Cancel();
		}

		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsOpen = false;
	}

	public void Expand()
	{
		this.IsExpanded = true;

		this.Dispatcher.Invoke(() =>
		{
			this.Focus();
			this.IsHitTestVisible = true;
			this.StaysOpen = false;
		});
	}

	private async Task ShowResultMenu()
	{
		await this.MainThread();

		if (this.placementTarget == null || this.currentResult == null)
			return;

		this.Entry = this.currentResult.Entry;
		this.PlacementTarget = this.placementTarget;
		this.IsOpen = true;
		this.DataContext = this.currentResult.Entry;
	}
}
