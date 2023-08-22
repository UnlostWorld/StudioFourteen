// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Plugin;

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
