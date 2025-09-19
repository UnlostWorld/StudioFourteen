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

namespace StudioFourteen;

using System.Text.RegularExpressions;

public static class SearchUtility
{
	public static string[] ToQuery(string input)
	{
		return input.Split(' ');
	}

	public static bool Matches(object input, string[]? query) => Matches(input.ToString(), query);

	public static bool Matches(string? input, string[]? query)
	{
		if (input == null)
			return false;

		if (query == null)
			return true;

		input = input.ToLower();
		input = Regex.Replace(input, @"[^\w\d\s]", string.Empty);

		bool matchesSearch = true;
		foreach (string str in query)
		{
			string strB = str.ToLower();

			// ignore 'the'
			if (strB == "the")
				continue;

			// ignore all symbols
			strB = Regex.Replace(strB, @"[^\w\d\s]", string.Empty);

			// Parse integers as numbers instead of strings
			if (int.TryParse(str, out int v))
			{
				matchesSearch &= input.Contains(v.ToString());
			}
			else
			{
				matchesSearch &= input.Contains(strB);
			}
		}

		if (!matchesSearch)
		{
			return false;
		}

		return true;
	}
}
