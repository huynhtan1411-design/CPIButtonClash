#ifndef DYNAMIC_RADIAL_MASKS_TORUS_4_ADVANCED_ADDITIVE_ID1_GLOBAL
#define DYNAMIC_RADIAL_MASKS_TORUS_4_ADVANCED_ADDITIVE_ID1_GLOBAL


uniform float4 DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global_DATA1[4];	
uniform float4 DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global_DATA2[4];	

#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global(float3 positionWS, float noise
)
{
    float retValue = 0; 

	int i = 0;
[unroll]	for(i = 0; i < 4; i++)
	{
		retValue += ShaderExtensions_DynamicRadialMasks_Torus_Advanced(positionWS, 
																					DYNAMIC_RADIAL_MASKS_READ_POSITION(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_RADIUS(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_INTENSITY(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_EDGE_SIZE(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_SMOOTH(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_NOISE_STRENGTH(DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global, i) * noise);
	}		

    return retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global_float(float3 positionWS, float noise, out float retValue)
{
		retValue = DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global(positionWS, noise); 	
}

void DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global_half(half3 positionWS, half noise, out half retValue)
{
		retValue = DynamicRadialMasks_Torus_4_Advanced_Additive_ID1_Global(positionWS, noise); 
}

#endif
