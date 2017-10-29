#ifndef SAMPLE_DISPLAYMENT_CG_INCLUDED
#define SAMPLE_DISPLAYMENT_CG_INCLUDED

#include "UnityCG.cginc"

uniform float Length;
uniform float Gravity;
uniform int Size;

float2 HTilde(float2 h0, float2 h0Mkconj, float i, float j) {
    
    float omegat = Dispersion(i, j) * Time.y;

    float cosTemp = cos(omegat);
    float sinTemp = sin(omegat);

    float2 c0 = float2(cosTemp, sinTemp);
    float2 c1 = float2(cosTemp, -sinTemp);

    return ComplexMultiply(h0, c0) + ComplexMultiply(h0Mkconj, c1);
}

float2 ComplexMultiply(float2 a, float2 b) {
    return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}
        
float Dispersion(float i, float j) {
    Vector2 k = new Vector2(Mathf.PI * (2 * i - Size) / Length, Mathf.PI * (2 * j - Size) / Length);
    return sqrt(Gravity * length(k));
}


#endif
