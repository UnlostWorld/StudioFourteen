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

cbuffer MaterialInstanceData : register(MaterialDataRegister)
{
	float2 WindowSize;
	float CornerRadius;
	float Margin;
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
	float4 color = 0;
	float bgIntensity = 1;

	float2 pixelPos = fragment.TexCoord * WindowSize;

	// Margin
	if (Margin > 0)
	{
		if (pixelPos.x < Margin
			|| pixelPos.y < Margin
			|| pixelPos.x > WindowSize.x - Margin
			|| pixelPos.y > WindowSize.y - Margin)
		{
			bgIntensity = 0;
		}
	}

	// Round corners
	if (bgIntensity > 0 && CornerRadius > 0)
	{
		float CornerMargin = CornerRadius + Margin;

		if(pixelPos.x < CornerMargin
			&& pixelPos.y < CornerMargin
			&& length(pixelPos - float2(CornerMargin, CornerMargin)) > CornerRadius)
		{
			bgIntensity = 0;
		}

		if(pixelPos.x > WindowSize.x - CornerMargin
			&& pixelPos.y < CornerMargin
			&& length(pixelPos - float2(WindowSize.x - CornerMargin, CornerMargin)) > CornerRadius)
		{
			bgIntensity = 0;
		}

		if(pixelPos.x < CornerMargin
			&& pixelPos.y > WindowSize.y - CornerMargin
			&& length(pixelPos - float2(CornerMargin, WindowSize.y - CornerMargin)) > CornerRadius)
		{
			bgIntensity = 0;
		}

		if(pixelPos.x > WindowSize.x - CornerMargin
			&& pixelPos.y > WindowSize.y - CornerMargin
			&& length(pixelPos - float2(WindowSize.x - CornerMargin, WindowSize.y - CornerMargin)) > CornerRadius)
		{
			bgIntensity = 0;
		}
	}

	// Blur the back buffer
	if (bgIntensity > 0)
	{
		float screenXSize = 1.0 / ScreenSize.x;
		float screenYSize = 1.0 / ScreenSize.y;
		float2 screenPos = GetScreenPosition(fragment);
		color = back_texture.Sample(back_sampler, screenPos);
		int count = 1;		// how many samples did we make
		int radius = 12;	// Blur radius
		int step = 2;		// skip n pixels when sampling for blur.

		for (float x = -radius; x <= radius; x += step)
		{
			for (float y = -radius; y <= radius; y += step)
			{
				color += back_texture.Sample(back_sampler, screenPos + float2(x * screenXSize, y * screenYSize));
				count++;
			}
		}

		color /= count;

		// Ignore the back buffer alpha
		color.a = 1;
	}

	// Actual Window
	{
		float4 uiColor = buffer_texture.Sample(buffer_sampler, fragment.TexCoord);
		color.rgb = lerp(color.rgb, uiColor.rgb, uiColor.a);

		// Let the window overlap the rounded corners
		// for AA purposes.
		if (color.a == 0)
		{
			color.a = uiColor.a;
		}
	}

	//color.a *= GetUiClippingAlpha(fragment);
	return color;
}