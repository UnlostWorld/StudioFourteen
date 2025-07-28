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
	float4 OutlineColor;
	float Thickness;
	float EndCaps;
	float FadeOutDepth;
	float MinAlpha;
};

static float zOffset = 0.0f;
static const float PI = 3.1415926f;
static const float fRatio = 2.0f;
static float fShadowSize = 0.75f;
static float fThickness = 0.01f;

Fragment vert(in Vertex vertex)
{
	return DefaultVert(vertex);
}

void addHalfCircle(inout TriangleStream<Fragment> triangleStream, int nCountTriangles, float4 linePointToConnect, float fPointWComponent, float fAngle, float4 color)
{
	float thickness = fThickness * Thickness * ViewportScale;

    Fragment output = (Fragment)0;
	output.Color = color;
    for (int nI = 0; nI < nCountTriangles; ++nI)
    {
        output.Position.x = cos(fAngle + (PI / nCountTriangles * nI)) * thickness / fRatio;
        output.Position.y = sin(fAngle + (PI / nCountTriangles * nI)) * thickness;
        output.Position.z = zOffset;
        output.Position.w = 0.0f;
        output.Position += linePointToConnect;
        output.Position *= fPointWComponent;
		output.ScreenPosition = output.Position;
        output.TexCoord = float2(-1, 0);
        triangleStream.Append(output);

        output.Position = linePointToConnect * fPointWComponent;
		output.ScreenPosition = output.Position;
        output.TexCoord = float2(-1, 1);
        triangleStream.Append(output);

        output.Position.x = cos(fAngle + (PI / nCountTriangles * (nI + 1))) * thickness / fRatio;
        output.Position.y = sin(fAngle + (PI / nCountTriangles * (nI + 1))) * thickness;
        output.Position.z = zOffset;
        output.Position.w = 0.0f;
        output.Position += linePointToConnect;
        output.Position *= fPointWComponent;
		output.ScreenPosition = output.Position;
        output.TexCoord = float2(-1, 0);
        triangleStream.Append(output);

        triangleStream.RestartStrip();
    }
}

[maxvertexcount(42)]
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
    float3 coordinateSystem = float3(1.0f, 0.0f, 0.0f);

    positionDifference.z = 0.0f;
    coordinateSystem.z = 0.0f;

    float fAngle = acos(dot(positionDifference.xy, coordinateSystem.xy) / (length(positionDifference.xy) * length(coordinateSystem.xy)));

    if (cross(positionDifference, coordinateSystem).z < 0.0f)
    {
        fAngle = 2.0f * PI - fAngle;
    }

    fAngle *= -1.0f;
    fAngle -= PI * 0.5f;

	float thickness = fThickness * Thickness * ViewportScale;
    float3 extend = 0;

    //first half circle of the line
	if (EndCaps)
	{
   	 	addHalfCircle(triangleStream, nCountTriangles, positionPoint0Transformed, fPoint0w, fAngle, input[0].Color);
    	addHalfCircle(triangleStream, nCountTriangles, positionPoint1Transformed, fPoint1w, fAngle + PI, input[1].Color);
	}
    else
    {
        extend = positionDifference;
	    extend = normalize(extend);
	    extend *= 0.003f;
    }

    //connection between the two circles
    //triangle1
	output.Color = input[0].Color;
	output.Color.a = 1;
	output.TexCoord = float2(0,-1);
    output.Position.x = cos(fAngle) * thickness / fRatio;
    output.Position.y = sin(fAngle) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
	output.Position.xyz += extend;
    output.Position *= fPoint0w; //undo calculate out the W parameter, because of usage of perspective rendering
	output.ScreenPosition = output.Position;
	triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 1;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
	output.Position.xyz += extend;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
	output.Position.xyz -= extend;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

    //triangle2
	output.Color = input[1].Color;
	output.TexCoord = float2(0,-1);
	output.Color.a = 1;
    output.Position.x = cos(fAngle) * thickness / fRatio;
    output.Position.y = sin(fAngle) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint0Transformed;
	output.Position.xyz += extend;
    output.Position *= fPoint0w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(0,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle) * thickness / fRatio;
    output.Position.y = sin(fAngle) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
	output.Position.xyz -= extend;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);

	output.TexCoord = float2(1,-1);
	output.Color.a = 0;
    output.Position.x = cos(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness / fRatio;
    output.Position.y = sin(fAngle + (PI / nCountTriangles * (nCountTriangles))) * thickness;
    output.Position.z = zOffset;
    output.Position.w = 0.0f;
    output.Position += positionPoint1Transformed;
	output.Position.xyz += extend;
    output.Position *= fPoint1w;
	output.ScreenPosition = output.Position;
    triangleStream.Append(output);
}

float4 pixel(Fragment frag) : SV_TARGET
{
	float l = frag.Color.a;
	float4 color = Color;

	// End Cap
	if (frag.TexCoord.x < 0)
	{
		if (frag.TexCoord.y < (fShadowSize / Thickness))
		{
			color.rgb = OutlineColor.rgb;
		}
	}

	// Line Segment
	else if (frag.TexCoord.y < 0)
	{
		if (frag.TexCoord.x > 0.5)
		{
			if (frag.TexCoord.x > (1 - ((fShadowSize / Thickness) / 2)))
			{
				color.rgb = OutlineColor.rgb;
			}
		}
		else
		{
			if (frag.TexCoord.x < ((fShadowSize / Thickness) / 2))
			{
				color.rgb = OutlineColor.rgb;
			}
		}
	}

	// Fade out if the line goes behind the root of this object.
	if (FadeOutDepth > 0)
	{
		float4 position = float4(0,0,0,1);
		position = mul(position, Transform);
		position = mul(position, ViewMatrix);
		position = mul(position, ProjectionMatrix);

		float d = 1 - smoothstep(position.w, position.w + FadeOutDepth, frag.Position.w);
		color.a *= d;
	}

	if (MinAlpha >= 1)
	{
		color.a *= GetUiClippingAlpha(frag);
	}
	else
	{
		color.a *= GetClippingAlpha(frag, MinAlpha);
	}

	if (color.a <= 0)
		discard;

	return color;
}