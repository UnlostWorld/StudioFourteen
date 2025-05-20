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

namespace StudioFourteen.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

public class AdornerContentPresenter : Adorner
{
	private readonly VisualCollection visuals;
	private readonly ContentPresenter contentPresenter;

	public AdornerContentPresenter(UIElement adornedElement)
	  : base(adornedElement)
	{
		this.visuals = new VisualCollection(this);
		this.contentPresenter = new ContentPresenter();
		this.visuals.Add(this.contentPresenter);
	}

	public AdornerContentPresenter(UIElement adornedElement, Visual content)
	  : this(adornedElement)
	{
		this.Content = content;
	}

	public object Content
	{
		get => this.contentPresenter.Content;
		set => this.contentPresenter.Content = value;
	}

	protected override int VisualChildrenCount => this.visuals.Count;

	protected override Size MeasureOverride(Size constraint)
	{
		this.contentPresenter.Measure(constraint);
		return this.contentPresenter.DesiredSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		this.contentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
		return this.contentPresenter.RenderSize;
	}

	protected override Visual GetVisualChild(int index) => this.visuals[index];
}