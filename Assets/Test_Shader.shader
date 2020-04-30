Shader "Unlit/Test_Shader"
{
    Properties
    {
        [Header(IntRange)]
        [Gamma]_MyFloat("This  my float", Float) = 2.46
        //_MyFloat2("This is my float2", Float) = 2
        //_MyFloat3("This is my float3", Float) = 2

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1 //"One"

        [Space(50)]
        [Header(Other Props)]
        _CustomColor("Colored outline", Color) = (1, 1, 1, 1)
        [HDR] _CustomGamma("Colored Gamma", Color) = (1, 1, 1, 1)
        [HDR] _Vector("That Vec", Vector) = (1, 3, 2, 4)
        [IntRange]_Range2("Range2", Range(-255, 255)) = 0
        _Range("AS", Range(654, 1024)) = 750
        _RangeFloat("TRUE F", Range(0, 1)) = 0.08
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
                float4 vertex : SV_POSITION;
            };

            float4 _CustomColor;
            float4 _Vector;
            float _RangeFloat;
            float _MyFloat;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _CustomColor * (_MyFloat+0.5);
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
