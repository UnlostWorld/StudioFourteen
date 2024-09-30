namespace StudioFourteen.Appearance.Customize;

using StudioFourteen.GameData.Excel;
using System.ComponentModel;
using System.Windows.Controls;
using DependencyPropertyGenerator;

[DependencyProperty<byte>("Value", DefaultValue = 0, DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<CharaMakeType.Menu>("Menu")]
public partial class CustomizeNumberOption : UserControl, INotifyPropertyChanged
{
	private bool manualEntry;

	public CustomizeNumberOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool ManualEntry
	{
		get => this.manualEntry;
		set
		{
			this.manualEntry = value;
			this.PropertyChanged?.Invoke(this, new(nameof(CustomizeNumberOption.ManualEntry)));
		}
	}
}
