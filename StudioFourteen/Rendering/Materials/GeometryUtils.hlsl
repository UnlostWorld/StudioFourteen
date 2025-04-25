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

#include "Common.hlsl"

struct Constants
{
    float4x4 ViewProjection;
	float4x4 ObjectTransform;
};

Constants constants : register(c0);

Texture2D maskDepth_texture : register(t0);
SamplerState maskDepth_sampler : register(s0);

float2 GetScreenPosition(Pixel pixel)
{
	float3 pos = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float4 GetMaskDepth(Pixel pixel)
{
	float2 screenPos = GetScreenPosition(pixel);
	return maskDepth_texture.Sample(maskDepth_sampler, screenPos);
}

Pixel vert(in Vertex vertex)
{
	float4 position = vertex.Position;
	position = mul(position, constants.ObjectTransform);
	position = mul(position, constants.ViewProjection);

	Pixel result;
	result.Position = position;
	result.Color = vertex.Color;
	result.TexCoord = vertex.TexCoord;
	result.ScreenPosition = position;
	return result;
}