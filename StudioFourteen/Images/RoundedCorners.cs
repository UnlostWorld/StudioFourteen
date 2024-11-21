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

namespace StudioFourteen.Images;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public static class RoundedCorners
{
	// This method can be seen as an inline implementation of an `IImageProcessor`:
	// (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
	public static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext context, float cornerRadius)
	{
		Size size = context.GetCurrentSize();
		IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

		context.SetGraphicsOptions(new GraphicsOptions()
		{
			Antialias = true,

			// Enforces that any part of this shape that has color is punched out of the background
			AlphaCompositionMode = PixelAlphaCompositionMode.DestOut,
		});

		// Mutating in here as we already have a cloned original
		// use any color (not Transparent), so the corners will be clipped
		foreach (IPath path in corners)
		{
			context = context.Fill(Color.Pink, path);
		}

		return context;
	}

	private static PathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
	{
		// First create a square
		var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

		// Then cut out of the square a circle so we are left with a corner
		IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

		// Corner is now a corner shape positions top left
		// let's make 3 more positioned correctly, we can do that by translating the original around the center of the image.
		float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
		float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

		// Move it across the width of the image - the width of the shape
		IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
		IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
		IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

		return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
	}
}
