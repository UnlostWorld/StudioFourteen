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

#include "Blit.hlsl"

cbuffer MaterialInstanceData : register(b2)
{
	float LeftRight;
	float TopBottom;
	float Unused1;
	float Unused2;
};

float4 pixel(Pixel pixel) : SV_TARGET
{
	float4 color = buffer_texture.Sample(buffer_sampler, pixel.TexCoord);
	float mask = 1 - color.a;
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

	color.a = mask;
	return color;
}