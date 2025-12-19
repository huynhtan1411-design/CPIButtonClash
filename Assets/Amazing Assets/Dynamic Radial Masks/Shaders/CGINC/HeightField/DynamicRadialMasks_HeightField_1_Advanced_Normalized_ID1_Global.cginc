#ifndef DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_1_ADVANCED_NORMALIZED_ID1_GLOBAL
#define DYNAMIC_RADIAL_MASKS_HEIGHTFIELD_1_ADVANCED_NORMALIZED_ID1_GLOBAL


uniform float4 DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global_DATA1[1];	
uniform float4 DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global_DATA2[1];	

#include "../../Core/Core.cginc"



////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                                Main Method                                 //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
float DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(float3 positionWS, float noise
)
{
    float retValue = 1; 

	int i = 0;

	{
		float mask = ShaderExtensions_DynamicRadialMasks_HeightField_Advanced(positionWS, 
																					DYNAMIC_RADIAL_MASKS_READ_POSITION(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_RADIUS(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_INTENSITY(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_EDGE_SIZE(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_SMOOTH(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i), 
																					DYNAMIC_RADIAL_MASKS_READ_NOISE_STRENGTH(DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global, i) * noise);

		retValue *= 1 - saturate(mask);	
	}		

    return 1 - retValue;
}

////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//                               Helper Methods                               //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////
void DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global_float(float3 positionWS, float noise, out float retValue)
{
		retValue = DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(positionWS, noise); 	
}

void DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global_half(half3 positionWS, half noise, out half retValue)
{
		retValue = DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(positionWS, noise); 	
}

#endif
