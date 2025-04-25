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
	float4 ClippingPlanes;
    float4x4 ViewProjection;
	float4x4 ObjectTransform;
};

struct Pixel
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
	float4 ScreenPosition:SCREENPOS;
};

Constants constants : register(c0);

Texture2D mask_texture : register(t0);
SamplerState mask_sampler : register(s0);

Texture2D depth_texture : register(t1);
SamplerState depth_sampler : register(s1);

float GetDepth(Pixel pixel)
{
	float3 pos = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	return pos.z;
}

float2 GetScreenPosition(Pixel pixel)
{
	float3 pos = pixel.ScreenPosition.xyz / pixel.ScreenPosition.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float GetClippingAlpha(Pixel pixel, float depthClipAlpha = 0)
{
	float2 screenPos = GetScreenPosition(pixel);
	float mask = mask_texture.Sample(mask_sampler, screenPos).r;
	float depth = depth_texture.Sample(depth_sampler, screenPos).r;
	float thisDepth = GetDepth(pixel);

	if (thisDepth < depth)
		mask = mask * depthClipAlpha;

	return mask;
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