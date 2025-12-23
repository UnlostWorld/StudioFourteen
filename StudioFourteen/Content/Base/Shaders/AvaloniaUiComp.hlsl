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

#include "Geometry.hlsl"

Texture2D buffer_texture : register(t3);
SamplerState buffer_sampler : register(s3);

Fragment vert(in Vertex vertex)
{
	Fragment result;

	float4 position = vertex.Position;
	position = mul(position, Transform);
	result.Position = position;
	result.ScreenPosition = position;

	result.Color = vertex.Color;
	result.TexCoord = vertex.TexCoord;

	return result;
}

float4 pixel(Fragment pixel) : SV_TARGET
{
	float4 color = buffer_texture.Sample(buffer_sampler, pixel.TexCoord);
	//color.a *= GetUiClippingAlpha(pixel);
	return color;
}