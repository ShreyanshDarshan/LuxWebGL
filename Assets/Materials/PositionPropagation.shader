Shader "Hidden/PositionPropagation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            int _FrameCount;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center_pos = i.uv * _MainTex_TexelSize.zw;
             
                float latest = 0;
                float2 acc_of_latest = float2(0, 0);
                for (int x=-1; x<=1; x++) {
                    for (int y=-1; y<=1; y++) {
                        float3 info = tex2D(_MainTex, i.uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
                        float2 pixel_pos = i.uv * _MainTex_TexelSize.zw + float2(x, y); 
                        float origin_frame = info.b;
                        // float2 charge_pos = info.rg * _MainTex_TexelSize.zw;
                        float2 charge_pos = tex2D(_PosTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rg;
                        float2 r_vec_cur = pixel_pos - charge_pos;
                        float2 r_vec_center = center_pos - charge_pos;
                        float dist_kernel_center = length(r_vec_center);
                        float dist_circle_center = _FrameCount - origin_frame;
                        if (dist_kernel_center - dist_circle_center < 1) {
                            if (latest < origin_frame) {
                                latest = origin_frame;
                                float2 acc = tex2D(_AccTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rg;
                                float2 acc_perp = acc - dot(acc, normalize(r_vec_cur)) * normalize(r_vec_cur);
                                acc_of_latest = acc_perp / length(r_vec_cur) * 1000.0;
                            }
                        }
                    }
                }

                // float acc_final = length(acc_of_latest.y);
                // float2 test_acc = tex2D(_AccTexture, float2(0, 0)).rg / 2.0;
                float4 col = float4(abs(acc_of_latest.x), abs(acc_of_latest.y), latest, 1);
                
                if (int(center_pos.x) == int(_Cell.x) && int(center_pos.y) == int(_Cell.y)) {
                    float2 charge_pos_unit = tex2D(_PosTexture, float2(0, 0)).rg * _MainTex_TexelSize.xy;
                    col = float4(1, 0, _FrameCount+1, 1);
                }

                return col;
            }
            ENDCG
        }
    }
}
