namespace ScreenshotStudio.Services;

public class PoseService : ServiceBase
{
	private string? selectedBone;

	public delegate void SelectionChangedDelegate(object? newSelection);

	public event SelectionChangedDelegate? SelectionChanged;

	public string? SelectedBone
	{
		get => this.selectedBone;
		set
		{
			this.selectedBone = value;
			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();
		}
	}
}
