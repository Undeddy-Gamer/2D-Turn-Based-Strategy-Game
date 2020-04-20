// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "pulse"
{
	Properties
	{
		_Color0("Color 0", Color) = (0,0.990566,0.1386579,0)
		_Duration("Duration", Float) = 3
		_Color1("Color 1", Color) = (0,0.1185064,1,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half filler;
		};

		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _Duration;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult1 = lerp( _Color0 , _Color1 , ( _SinTime.w * _Duration ));
			o.Emission = lerpResult1.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
1240;749;1324;679;1208.226;211.1498;1.431428;True;True
Node;AmplifyShaderEditor.SinTimeNode;13;-654.2631,262.6529;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-488.2177,412.9532;Float;False;Property;_Duration;Duration;1;0;Create;True;0;0;False;0;3;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-317.8777,328.4988;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-531.1608,100.9017;Float;False;Property;_Color1;Color 1;2;0;Create;True;0;0;False;0;0,0.1185064,1,0;0,1,0.1089518,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-534.0238,-90.90971;Float;False;Property;_Color0;Color 0;0;0;Create;True;0;0;False;0;0,0.990566,0.1386579,0;0,0.990566,0.1386579,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1;-219.1092,35.056;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;12;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;pulse;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;13;4
WireConnection;6;1;10;0
WireConnection;1;0;4;0
WireConnection;1;1;5;0
WireConnection;1;2;6;0
WireConnection;12;2;1;0
ASEEND*/
//CHKSM=CAE41FCC27F9692A0C2D5BF4E5601F537ABF74CF