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

Texture2D back_texture : register(t2);
SamplerState back_sampler : register(s2);

Texture2D buffer_texture : register(t3);
SamplerState buffer_sampler : register(s3);

cbuffer UiPassData : register(PassDataRegister)
{
    float2 ScreenSize;
	float2 RenderScale;
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
};

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

float2 GetScreenPosition(Fragment frag)
{
	float3 pos = frag.ScreenPosition.xyz / frag.ScreenPosition.w;
	return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float4 pixel(Fragment fragment) : SV_TARGET
{
	float4 color = 1;
	float xSize = 1.0 / ScreenSize.x;
	float ySize = 1.0 / ScreenSize.y;

	float2 screenPos = GetScreenPosition(fragment);

	int count = 1;
	color = back_texture.Sample(back_sampler, screenPos);

	int radius = 12;

	for (float x = -radius; x <= radius; x++)
	{
		for (float y = -radius; y <= radius; y++)
		{
			color += back_texture.Sample(back_sampler, screenPos + float2(x * xSize, y * ySize));
			count++;
		}
	}

	color /= count;
	color.a = 1;

	// TODO: rounded corners?

	float4 uiColor = buffer_texture.Sample(buffer_sampler, fragment.TexCoord);
	color.rgb = lerp(color.rgb, uiColor.rgb, uiColor.a);
	//color.a *= GetUiClippingAlpha(fragment);
	return color;
}