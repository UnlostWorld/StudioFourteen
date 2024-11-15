namespace Lumina.Text.ReadOnly;

public static class ReadOnlySeStringExtensions
{
	public static string? GetString(this ReadOnlySeString self)
	{
		string text = self.ExtractText();
		if (string.IsNullOrEmpty(text))
			return null;

		return text;
	}
}
