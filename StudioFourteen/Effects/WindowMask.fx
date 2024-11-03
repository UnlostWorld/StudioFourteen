sampler2D implicitInputSampler : register(S0);
float opacity : register(C0);
float4 regions[32] : register(C1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(implicitInputSampler, uv);
    
    for (int i = 0; i < 32; i++)
    {
        // region:
        //  x = X
        //  y = Y
        //  z = Width
        //  w = Height
        if (uv.x > regions[i].x
            && uv.x < regions[i].x + regions[i].z
            && uv.y > regions[i].y
            && uv.y < regions[i].y + regions[i].w)
        {
            return float4(0, 0, 0, opacity);
        }
    }
    
    return color;
}