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

namespace StudioFourteen.Plugin;

using System.Reflection;
using Dalamud.Plugin.Services;

public static class ITextureSubstitutionProviderExtensions
{
	// If TextureManager.GetSubstitutedPath(string) gets added to ITextureManager we won't need this. till then, reflect away!
	// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/TextureManager.cs#L217
	public static string GetSubstitutedPath(this ITextureSubstitutionProvider self, string path)
	{
		FieldInfo? field = self.GetType().GetField(nameof(ITextureSubstitutionProvider.InterceptTexDataLoad), BindingFlags.NonPublic | BindingFlags.Instance);
		ITextureSubstitutionProvider.TextureDataInterceptorDelegate? del = field?.GetValue(self) as ITextureSubstitutionProvider.TextureDataInterceptorDelegate;

		string? to = null;
		del?.Invoke(path, ref to);
		return to ?? path;
	}
}
