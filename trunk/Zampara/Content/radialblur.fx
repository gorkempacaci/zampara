sampler TextureSampler : register(s0);

float2 Center = { 0.5, 0.5 };
float BlurStart = 1.0f;
float BlurWidth = -0.1;

float4 PS_RadialBlur(float2 TexC: TEXCOORD0, uniform int nsamples) : COLOR
{
    TexC -= Center;
    float4 c = 0;
    // this loop will be unrolled by compiler and the constants precalculated:
    for(int i=0; i<nsamples; i++) {
    	float scale = BlurStart + BlurWidth*(i/(float) (nsamples-1));
    	c += tex2D(TextureSampler, TexC.xy * scale + Center );
   	}
   	c /= nsamples;
    return c;
} 

technique RadialBlur
{
    pass p0
    {
		PixelShader  = compile ps_2_0 PS_RadialBlur(16);
    }
}