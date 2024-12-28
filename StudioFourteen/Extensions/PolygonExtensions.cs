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

namespace System.Windows.Shapes;

using System.Windows;

public static class PolygonExtensions
{
	// https://stackoverflow.com/a/19298028/9934501
	public static bool IsPointWithin(this Polygon self, Point p)
	{
		int sides = self.Points.Count;
		int j = sides - 1;
		bool pointStatus = false;
		for (int i = 0; i < sides; i++)
		{
			if ((self.Points[i].Y < p.Y && self.Points[j].Y >= p.Y) || (self.Points[j].Y < p.Y && self.Points[i].Y >= p.Y))
			{
				if (self.Points[i].X + ((p.Y - self.Points[i].Y) / (self.Points[j].Y - self.Points[i].Y) * (self.Points[j].X - self.Points[i].X)) < p.X)
				{
					pointStatus = !pointStatus;
				}
			}

			j = i;
		}

		return pointStatus;
	}
}