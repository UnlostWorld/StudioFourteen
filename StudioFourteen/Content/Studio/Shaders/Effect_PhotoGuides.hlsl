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

#include "Effect.hlsl"

cbuffer MaterialInstanceData : register(b2)
{
	float LeftRight;
	float TopBottom;
	uint GuidesMode;
	float Unused3;
};

static const float lineWidthPixels = 4;

float4 pixel(Pixel pixel) : SV_TARGET
{
	float4 color = buffer_texture.Sample(buffer_sampler, pixel.TexCoord);
	float mask = mask_texture.Sample(mask_sampler, pixel.TexCoord).r;

	if(mask < 0.3)
		mask = 0;

	float intensity = 0;

	if (pixel.TexCoord.x < LeftRight
		|| pixel.TexCoord.x > 1 - LeftRight
		|| pixel.TexCoord.y < TopBottom
		|| pixel.TexCoord.y > 1 - TopBottom)
	{
		float bw = (color.r + color.g + color.b) / 3;
		color.rgb = bw * 0.25;
	}
	else
	{
		// Thirds
		if (GuidesMode == 1)
		{
			float lineWidthX = (lineWidthPixels / ScreenSize.x) / 2;
			float lineWidthY = (lineWidthPixels / ScreenSize.y) / 2;

			float left = lerp(LeftRight, 1 - LeftRight, 0.333);
			float right = lerp(LeftRight, 1 - LeftRight, 0.666);
			float top = invLerp(TopBottom, 1 - TopBottom, 0.333);
			float bottom = invLerp(TopBottom, 1 - TopBottom, 0.666);

			if ((pixel.TexCoord.x < left + lineWidthX && pixel.TexCoord.x > left - lineWidthX)
				|| (pixel.TexCoord.x < right + lineWidthX && pixel.TexCoord.x > right - lineWidthX)
				|| (pixel.TexCoord.y < top + lineWidthY && pixel.TexCoord.y > top - lineWidthY)
				|| (pixel.TexCoord.y < bottom + lineWidthY && pixel.TexCoord.y > bottom - lineWidthY))
			{
				float bw = (color.r + color.g + color.b) / 3;
				color.rgb = bw * 0.5;
			}
		}

		// Crosshair
		else if (GuidesMode == 2)
		{
			float lineWidthX = (lineWidthPixels / ScreenSize.x) / 2;
			float lineWidthY = (lineWidthPixels / ScreenSize.y) / 2;

			float vertical = lerp(LeftRight, 1 - LeftRight, 0.5);
			float horizontal = invLerp(TopBottom, 1 - TopBottom, 0.5);

			if ((pixel.TexCoord.x < vertical + lineWidthX && pixel.TexCoord.x > vertical - lineWidthX)
				|| (pixel.TexCoord.y < horizontal + lineWidthY && pixel.TexCoord.y > horizontal - lineWidthY))
			{
				float bw = (color.r + color.g + color.b) / 3;
				color.rgb = bw * 0.5;
			}
		}
	}

	color.a = mask;
	return color;
}