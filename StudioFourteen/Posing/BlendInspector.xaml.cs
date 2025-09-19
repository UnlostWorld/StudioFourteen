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

namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Scene;
using StudioFourteen.Selection;
using System.Threading.Tasks;
using System.Windows;
using StudioFourteen;

[DependencyProperty<SceneObjectBase>("Selection")]
[DependencyProperty<BlendSelection>("BlendSelection")]
public partial class BlendInspector : View
{
	private bool extendClamps = false;

	public PosePanel? Panel => this.FindParent<PosePanel>();

	public double BlendMinimum
	{
		get
		{
			if (this.BlendSelection == null)
				return 0;

			if (this.BlendSelection.HasLeft)
				return this.extendClamps ? -500 : -100;

			return 0;
		}
	}

	public double BlendMaximum
	{
		get
		{
			if (this.BlendSelection == null)
				return 0;

			return this.extendClamps ? 500 : 100;
		}
	}

	public bool ExtendClamps
	{
		get => this.extendClamps;
		set
		{
			this.extendClamps = value;
			this.NotifyPropertyChanged();
			this.NotifyPropertyChanged(nameof(BlendInspector.BlendMinimum));
			this.NotifyPropertyChanged(nameof(BlendInspector.BlendMaximum));
		}
	}

	public double BlendValue
	{
		get => (this.BlendSelection?.Value * 100) ?? 0;
		set
		{
			this.BlendSelection?.SetValue(value / 100);
			this.NotifyPropertyChanged();
		}
	}

	partial void OnSelectionChanged()
	{
		this.BlendSelection = this.Selection as BlendSelection;

		this.NotifyPropertyChanged(nameof(BlendInspector.BlendValue));
		this.NotifyPropertyChanged(nameof(BlendInspector.BlendMinimum));
		this.NotifyPropertyChanged(nameof(BlendInspector.BlendMaximum));
		this.NotifyPropertyChanged(nameof(BlendInspector.ExtendClamps));

		Task.Run(async () =>
		{
			await Task.Delay(50);
			await this.MainThread();
			this.NotifyPropertyChanged(nameof(BlendInspector.BlendMinimum));
			this.NotifyPropertyChanged(nameof(BlendInspector.BlendMaximum));
		});
	}
}