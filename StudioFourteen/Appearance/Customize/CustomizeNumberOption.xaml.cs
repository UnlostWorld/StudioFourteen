namespace StudioFourteen.Appearance.Customize;

using System.ComponentModel;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using Lumina.Excel.Sheets;

[DependencyProperty<byte>("Value", DefaultValue = 0, DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<CharaMakeType>("MakeType")]
[DependencyProperty<CharaMakeType.CharaMakeStructStruct>("MakeStruct")]
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
