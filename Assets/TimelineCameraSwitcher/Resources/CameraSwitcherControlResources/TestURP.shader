Shader "Unlit/TestURP"
{
    Properties
    {
        _TextureA ("_TextureA", 2D) = "white" {}
        _TextureB ("_TextureA", 2D) = "white" {}
        _CrossFade("CrossFade", Range(0,1)) = 0
        [ShowAsVector2] _WigglerA("WigglerA", Vector) = (0,0,0,0)
        
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _TextureA;
            float4 _TextureA_ST;

            sampler2D _TextureB;
            float4 _TextureB_ST;

            float _CrossFade;
            float4 _WigglerA;
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
                // sample the texture
                fixed4 col = lerp(tex2D(_TextureA, i.uv),tex2D(_TextureB,i.uv),_CrossFade);

                
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
