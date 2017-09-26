
#ifndef WATER_CG_INCLUDED
#define WATER_CG_INCLUDED

#include "UnityCG.cginc"

uniform fixed _GerstnerIntensity;

fixed3 GerstnerOffset4 (fixed2 xzVtx, fixed4 steepness, fixed4 amp, fixed4 freq, fixed4 speed, fixed4 dirAB, fixed4 dirCD) 
{
	fixed3 offsets;

	fixed4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
	fixed4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;

	fixed4 dotABCD = freq.xyzw * fixed4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
	fixed4 TIME = _Time.yyyy * speed;

	fixed4 COS = cos (dotABCD + TIME);
	fixed4 SIN = sin (dotABCD + TIME);

	offsets.x = dot(COS, fixed4(AB.xz, CD.xz));
	offsets.z = dot(COS, fixed4(AB.yw, CD.yw));
	offsets.y = dot(SIN, amp);

	return offsets;			
}	


fixed3 GerstnerNormal4 (fixed2 xzVtx, fixed4 amp, fixed4 freq, fixed4 speed, fixed4 dirAB, fixed4 dirCD) 
{
	fixed3 nrml = fixed3(0,2,0);

	fixed4 AB = freq.xxyy * amp.xxyy * dirAB.xyzw;
	fixed4 CD = freq.zzww * amp.zzww * dirCD.xyzw;

	fixed4 dotABCD = freq.xyzw * fixed4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
	fixed4 TIME = _Time.yyyy * speed;

	fixed4 COS = cos (dotABCD + TIME);

	nrml.x -= dot(COS, fixed4(AB.xz, CD.xz));
	nrml.z -= dot(COS, fixed4(AB.yw, CD.yw));

	nrml.xz *= _GerstnerIntensity;
	nrml = normalize (nrml);
	return nrml;			
}	

void Gerstner (	out fixed3 offs, out fixed3 nrml,
	fixed3 vtx, fixed3 tileableVtx, 
	fixed4 amplitude, fixed4 frequency, fixed4 steepness, 
	fixed4 speed, fixed4 directionAB, fixed4 directionCD ) 
{
	#ifdef WATER_VERTEX_DISPLACEMENT_ON
	offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
	nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD);		
	#else
	offs = fixed3(0,0,0);
	nrml = fixed3(0,1,0);
	#endif							
}


#endif
