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

// Thanks @Javier Salcedo!
// https://dev.to/javiersalcedopuyo/simple-infinite-grid-shader-5fah

#include "Common.hlsl"

cbuffer GeometryPassData : register(PassDataRegister)
{
    float4x4 ViewMatrix;
	float4x4 ProjectionMatrix;
	float4 CameraPosition;
	float UseGameDepth;
	float UseGameUiMask;
	float Unused1;
	float Unused2;
};

cbuffer RendererInstanceData : register(RendererDataRegister)
{
	float4 Transform;
};

cbuffer MaterialInstanceData : register(MaterialDataRegister)
{
	float4 Color;
	float4 XColor;
	float4 ZColor;
	float GridSize;
	float LineThickness;
	float Height;
};

struct Fragment
{
    float4 Position:SV_POSITION;
    float4 Color:COLOR;
	float2 TexCoord:TEXCOORD;
	float4 Position2:POSITION;
	float4 WorldPosition:WORLDPOS;
};

static const float quadScale = 50.0f;

Texture2D mask_texture : register(t0);
SamplerState mask_sampler : register(s0);

Texture2D depth_texture : register(t1);
SamplerState depth_sampler : register(s1);

float GetDepth(Fragment pixel)
{
	float3 pos = pixel.Position2.xyz / pixel.Position2.w;
	return pos.z;
}

float2 GetScreenPosition(Fragment pixel)
{
	float3 pos = pixel.Position2.xyz / pixel.Position2.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float GetClippingAlpha(Fragment pixel, float depthClipAlpha = 0)
{
	float2 screenPos = GetScreenPosition(pixel);
	float mask = mask_texture.Sample(mask_sampler, screenPos).r;

	float depth = depth_texture.Sample(depth_sampler, screenPos).r;

	if (depth == 0)
		return 1;

	float thisDepth = GetDepth(pixel);

	if (thisDepth < depth)
		mask = mask * depthClipAlpha;

	return mask;
}

Fragment vert(in Vertex vertex)
{
	Fragment result;

	float4 position = vertex.Position;
	position.xyz *= quadScale;
	position.xz += CameraPosition.xz;
	position.y += Height;
	result.WorldPosition = position;

	result.Position = mul(mul(position, ViewMatrix), ProjectionMatrix);
	result.Position2 = result.Position;
	result.Color = vertex.Color;
	result.TexCoord = vertex.TexCoord;
	return result;
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float largeGridSize = GridSize * 10;
	float2 cell_coords = mod((frag.WorldPosition.xz * 1) + (largeGridSize * 0.5f), largeGridSize);
	float2 distance_to_cell = abs(cell_coords - (largeGridSize * 0.5f));

	float2 subcell_coords = mod((frag.WorldPosition.xz * 1) + (GridSize * 0.5f), GridSize);
	float2 distance_to_subcell = abs(subcell_coords - (GridSize * 0.5f));

	// Fade out towards the ends
	float r = smoothstep(0, quadScale, frag.Position.w);
	r = 1 - r;

	float4 color = Color;
	color.a = 0;
	if (any(distance_to_subcell < (LineThickness / 100) * (frag.Position.w * 2)))
	{
		color.a = 0.25;
	}

	if(distance_to_cell.y < (LineThickness / 100) * (frag.Position.w * 2))
	{
		color = XColor;
		color.a = 1;
	}
	if(distance_to_cell.x < (LineThickness / 100) * (frag.Position.w * 2))
	{
		color = ZColor;
		color.a = 1;
	}

	color.a *= Color.a;
	color.a *= GetClippingAlpha(frag, 0.0f);
	color.a *= r;
	return color;
}