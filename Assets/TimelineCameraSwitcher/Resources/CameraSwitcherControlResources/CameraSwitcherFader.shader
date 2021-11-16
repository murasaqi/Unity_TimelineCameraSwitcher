Shader "Unlit/CameraSwitcherFader"
{
    Properties
    {
        _TextureA ("_TextureA", 2D) = "white" {}
        _TextureB ("_TextureA", 2D) = "white" {}
        _CrossFade("CrossFade", Range(0,1)) = 0
        _Wiggler("Wiggler", Vector) = (0,0,0,0)
        _WigglerRange("_WigglerRange",Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD4;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD4;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _TextureA;
            float4 _TextureA_ST;

            sampler2D _TextureB;
            float4 _TextureB_ST;

            float _CrossFade;
            float4 _Wiggler;
            float4 _WigglerRange;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                
                
                o.uv = TRANSFORM_TEX(v.uv, _TextureA);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                float scaleA = 1.-_WigglerRange.x;
                float2 pivot_uv = float2(0.5, 0.5); 
                float2 r = (i.uv - pivot_uv) * scaleA;
                float2 uv_a = r+pivot_uv;


                float scaleB = 1.-_WigglerRange.y;
                float2 r_b = (i.uv - pivot_uv) * scaleB;
                float2 uv_b = r_b+pivot_uv;
                float4  colA = tex2D(_TextureA, uv_a+_Wiggler.xy);
                float4  colB = tex2D(_TextureB, uv_b+_Wiggler.zw);
                // sample the texture
                fixed4 col = lerp(colA,colB,_CrossFade);

                
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
