Shader "Hidden/FieldCalculate"
{
    Properties
    {
        _MainTex ("FieldTexture", 2D) = "white" {}
        _Cell ("Cell", Vector) = (0.01, 0.01, 0.01, 0.01)
        _PosTexture ("PosTexture", 2D) = "white" {}
        // _FrameCount ("FrameCount", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _PosTexture;
            sampler2D _AccTexture;
            float4 _MainTex_TexelSize;
            float4 _PosTexture_TexelSize;
            float4 _AccTexture_TexelSize;
            float4 _Cell;
            float _Charge;
            int _FrameCount;

            fixed4 frag (v2f i) : SV_Target
            {
                float latest = 0;
                float3 acc_of_latest = float3(0, 0, 0);
                float2 pixel_pos2d = i.uv * _MainTex_TexelSize.zw; 
                float3 pixel_pos = float3(pixel_pos2d.x, pixel_pos2d.y, 0);
                float origin_frame = tex2D(_MainTex, i.uv);
                float3 charge_pos = tex2D(_PosTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rgb;
                float3 r_vec_cur = pixel_pos - charge_pos;
                float3 acc = tex2D(_AccTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rgb;
                float3 acc_perp = acc - dot(acc, normalize(r_vec_cur)) * normalize(r_vec_cur);
                acc_of_latest = _Charge * acc_perp / length(r_vec_cur);

                float4 col = float4(acc_of_latest.x, acc_of_latest.y, acc_of_latest.z, 1);
                
                float3 center_pos = float3((i.uv * _MainTex_TexelSize.zw).x, (i.uv * _MainTex_TexelSize.zw).y, 0);
                if (int(center_pos.x) == int(_Cell.x) && int(center_pos.y) == int(_Cell.y)) {
                    // float2 charge_pos = tex2D(_PosTexture, float2(0, 0)).rg;
                    // float2 r_vec_cur = center_pos - charge_pos;
                    // float2 acc = tex2D(_AccTexture, float2(0, 0)).rg;
                    // float2 acc_perp = acc - dot(acc, normalize(r_vec_cur)) * normalize(r_vec_cur);
                    // float2 acc_final = acc_perp / length(r_vec_cur) * 1000.0;
                    // col = float4(0, 0, _FrameCount+1, 1);
                    col = float4(0, 0, 0, 1);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
