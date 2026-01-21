Shader "Custom/VtPackSpriteSeq"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "black" {}
        _VtPack ("VT Pack", 2D) = "black" {}
        _VtIndex ("VT Index", 2D) = "black" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(GPUSpriteProperties0)
                UNITY_DEFINE_INSTANCED_PROP(float, _GPU_Frame_PixelSegmentation)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _GPU_Frame_ColorSegmentation)
            UNITY_INSTANCING_BUFFER_END(GPUSpriteProperties0)

            sampler2D _MainTex;
            sampler2D _VtPack;
            sampler2D _VtIndex;

            CBUFFER_START(UnityPerMaterial)
            float4 _VtIndex_TexelSize;   // 1/width , 1/height
            float4 _VtPack_TexelSize;   // 1/width , 1/height
            float4 _Color;
            CBUFFER_END

            struct FragmentToVertex
            {
                float4 vertex   : POSITION;
                uint vertexId : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexToFragment
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            VertexToFragment vert (FragmentToVertex v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                float frame = UNITY_ACCESS_INSTANCED_PROP(GPUSpriteProperties0, _GPU_Frame_PixelSegmentation);
                float currentFrame = floor(frame)+0.5;

                float2 idxUV = float2(v.vertexId+0.5, currentFrame) * _VtIndex_TexelSize.xy;
                uint vtxID = (uint)tex2Dlod(_VtIndex, float4(idxUV, 0, 0)).r;

                float2 vtVtxUV = float2(vtxID+0.5, currentFrame) * _VtPack_TexelSize.xy;
                float4 vdata = tex2Dlod(_VtPack, float4(vtVtxUV, 0, 0));

                float2 localXY = vdata.ba;
                float3 localPos =float3(localXY, 0);

                VertexToFragment o;
                o.vertex = UnityObjectToClipPos(localPos);
                o.texcoord  = vdata.rg;
                o.color = UNITY_ACCESS_INSTANCED_PROP(GPUSpriteProperties0, _GPU_Frame_ColorSegmentation);
                return o;
            }

            fixed4 frag (VertexToFragment i) : SV_Target
            {
            //    return fixed4(i.texcoord.x,i.texcoord.y,0,1);
                float4 color = tex2D(_MainTex, i.texcoord)*_Color*i.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}