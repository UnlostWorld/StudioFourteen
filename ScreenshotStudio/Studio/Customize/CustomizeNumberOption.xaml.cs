namespace ScreenshotStudio.Studio.Customize;

using ScreenshotStudio.GameData.Excel;
using System.ComponentModel;
using System.Windows.Controls;
using WpfUtils.DependencyProperties;

public partial class CustomizeNumberOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeNumberOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeNumberOption>(nameof(Menu), OnMenuChanged);
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

	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public CharaMakeType.Menu? Menu
	{
		get => MenuDp.Get(this);
		set => MenuDp.Set(this, value);
	}

	public static void OnValueChanged(CustomizeNumberOption sender, byte newValue)
	{
	}

	public static void OnMenuChanged(CustomizeNumberOption sender, CharaMakeType.Menu? newValue)
	{
		// A bit of a hack, but as the selected value has not changed, while the list of options has,
		// refresh the selector so it shows the correct selected item.
		// Doing this will set the value to 0 on load, so ... don't.
		////sender.ValueList.SelectedItem = sender.Value;
	}
}
