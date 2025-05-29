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

cbuffer GeometryPassData : register(PassDataRegister)
{
    float4x4 ViewMatrix;
	float4x4 ProjectionMatrix;
	float4 CameraPosition;
};

cbuffer RendererInstanceData : register(RendererDataRegister)
{
	float4x4 Transform;
};

struct Fragment
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
	float4 ScreenPosition:POSITION;
	float4 WorldPosition:WORLDPOS;
	float4 ObjectPosition:OBJPOS;
};

Texture2D mask_texture : register(t0);
SamplerState mask_sampler : register(s0);

Texture2D depth_texture : register(t1);
SamplerState depth_sampler : register(s1);

float GetDepth(Fragment frag)
{
	float3 pos = frag.ScreenPosition.xyz / frag.ScreenPosition.w;
	return pos.z;
}

float2 GetScreenPosition(Fragment frag)
{
	float3 pos = frag.ScreenPosition.xyz / frag.ScreenPosition.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float GetUiClippingAlpha(Fragment frag)
{
	float2 screenPos = GetScreenPosition(frag);
	return mask_texture.Sample(mask_sampler, screenPos).r;
}

float GetClippingAlpha(Fragment frag, float depthClipAlpha = 0)
{
	float2 screenPos = GetScreenPosition(frag);
	float mask = mask_texture.Sample(mask_sampler, screenPos).r;
	float depth = depth_texture.Sample(depth_sampler, screenPos).r;
	float thisDepth = GetDepth(frag);

	if (thisDepth < depth)
		mask = mask * depthClipAlpha;

	return mask;
}

Fragment DefaultVert(in Vertex vertex)
{
	Fragment result;

	float4 position = vertex.Position;
	position = mul(position, Transform);
	position = mul(position, ViewMatrix);
	position = mul(position, ProjectionMatrix);
	result.Position = position;
	result.ScreenPosition = position;

	result.Color = vertex.Color;
	result.TexCoord = vertex.TexCoord;

	return result;
}