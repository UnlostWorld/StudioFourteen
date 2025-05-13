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

// Thanks Thomas!
// https://stackoverflow.com/questions/42510542/render-thick-lines-with-instanced-rendering-in-directx-11

#include "Geometry.hlsl"

cbuffer MaterialInstanceData : register(MaterialDataRegister)
{
	float4 Color;
};

static const float PI = 3.1415926f;
static const float fRatio = 2.0f;
static float fThickness = 0.0075f;
static float fShadowSize = 0.75f;

[maxvertexcount(12)]
void geometry(line Fragment input[2], inout TriangleStream<Fragment> triangleStream)
{
    Fragment output= (Fragment)0;

    int nCountTriangles = 6;

    float4 positionPoint0Transformed = input[0].Position;
    float4 positionPoint1Transformed = input[1].Position;

    float fPoint0w = positionPoint0Transformed.w;
    float fPoint1w = positionPoint1Transformed.w;

    //calculate out the W parameter, because of usage of perspective rendering
    positionPoint0Transformed.xyz = positionPoint0Transformed.xyz / positionPoint0Transformed.w;
    positionPoint0Transformed.w = 1.0f;
    positionPoint1Transformed.xyz = positionPoint1Transformed.xyz / positionPoint1Transformed.w;
    positionPoint1Transformed.w = 1.0f;

    //calculate the angle between the 2 points on the screen
    float3 positionDifference = positionPoint0Transformed.xyz - positionPoint1Transformed.xyz;
    float3 coordinateSysten = float3(1.0f, 0.0f, 0.0f);

    positionDifference.z = 0.0f;
    coordinateSysten.z = 0.0f;

    float fAngle = acos(dot(positionDifference.xy, coordinateSysten.xy) / (length(positionDifference.xy) * length(coordinateSysten.xy)));

    if (cross(positionDifference, coordinateSysten).z < 0.0f)
    {
        fAngle = 2.0f * PI - fAngle;
    }

    fAngle *= -1.0f;
    fAngle -= PI * 0.5f;

    //connection between the two circles
    //triangle1
	output.Color = input[0].Color;
	output.Color.a = 1;
	output.TexCoord = float2(0,-1);
    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w; //undo calculate out the W parameter, because of usage of perspective rendering
	output.ScreenPosition = output.Position;
	triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 1;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    //triangle2
	output.Color = input[1].Color;
	output.TexCoord = float2(0,-1);
	output.Color.a = 1;
    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(0,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle) * fThickness / fRatio;
    output.Position.y = sin(fAngle) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * fThickness;
    output.Position.z = 0.0f;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float l = frag.Color.a;
	frag.Color.a = 1;
	float4 color = frag.Color * ObjectColor;

	// End Cap
	if (frag.TexCoord.x < 0)
	{
		if (frag.TexCoord.y < fShadowSize)
		{
			color.rgb = 0;
		}
	}

	// Line Segment
	else if (frag.TexCoord.y < 0)
	{
		if (frag.TexCoord.x > 0.5)
		{
			if (frag.TexCoord.x > (1 - (fShadowSize / 2)))
			{
				color.rgb = 0;
			}
		}
		else
		{
			if (frag.TexCoord.x < (fShadowSize / 2))
			{
				color.rgb = 0;
			}
		}
	}

	color.rgb *= smoothstep(-0.25, 1, l);

	color.a *= GetUiClippingAlpha(frag);
	return color;
}