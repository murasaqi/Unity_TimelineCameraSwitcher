Shader "Unlit/CameraSwitcherFader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TextureA ("_TextureA", 2D) = "white" {}
        _TextureB ("_TextureB", 2D) = "white" {}
        _CrossFade("CrossFade", Range(0,1)) = 0
        _WigglerValueA("WigglerA", Vector) = (0,0,0,0)
        _ClipSizeA("_WigglerRangeA",Vector) = (0,0,0,0)
        _MultiplyColorA("MultiplyColorA",Color) = (1,1,1,1)
        
        _WigglerValueB("WigglerB", Vector) = (0,0,0,0)
        _ClipSizeB("_WigglerRangeB",Vector) = (0,0,0,0)
        _MultiplyColorB("MultiplyColorB",Color) = (1,1,1,1)
        _BlendA("Blend A", int) = 0
        _BlendB("Blend B", int) = 0
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

            sampler2D_float _TextureA;
            float4 _TextureA_ST;

            sampler2D_float _TextureB;
            float4 _TextureB_ST;

            float _CrossFade;
            float2 _WigglerValueA;
            float2 _ClipSizeA;

            float2 _WigglerValueB;
            float2 _ClipSizeB;
            float4 _MultiplyColorA;
            float4 _MultiplyColorB;
            int _BlendA;
            int _BlendB;
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


                float scaleA = 1.-max(_ClipSizeA.x,_ClipSizeA.y);
                float2 pivot_uv = float2(0.5, 0.5); 
                float2 r = (i.uv - pivot_uv) * scaleA;
                float2 uv_a = r+pivot_uv;


                float scaleB = 1.-max(_ClipSizeB.x,_ClipSizeB.y);
                float2 r_b = (i.uv - pivot_uv) * scaleB;
                float2 uv_b = r_b+pivot_uv;
                float4  colA = tex2D(_TextureA, uv_a+_WigglerValueA);
                float4  colB = tex2D(_TextureB, uv_b+_WigglerValueB);
                // sample the texture
               
                if(_BlendA == 1)
                {
                    colA *= _MultiplyColorA;
                }else if(_BlendA == 2)
                {
                     colA += _MultiplyColorA;
                }else if(_BlendA == 3)
                {
                     colA -=_MultiplyColorA;
                }
                else if(_BlendA == 4)
                {
                    colA =_MultiplyColorA;
                }

                 if(_BlendB == 1)
                {
                    colB *=_MultiplyColorB;
                }else if(_BlendB == 2)
                {
                    colB += _MultiplyColorB;
                }else if(_BlendB == 3)
                {
                    colB -=_MultiplyColorB;
                }
                else if(_BlendB == 4)
                {
                    colB =_MultiplyColorB;
                }

                 float4 col = lerp(colA,colB,_CrossFade);

                
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
