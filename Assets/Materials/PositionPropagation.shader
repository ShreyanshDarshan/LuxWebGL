Shader "Hidden/PositionPropagation"
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
            float4 _GridSize;
            float _Charge;
            int _FrameCount;

            int3 get_pos(float2 pixel) {
                int3 pos;
                pos.x = (int)(pixel.x * _MainTex_TexelSize.z);
                pos.y = (int)(pixel.y * _MainTex_TexelSize.w) / (int)(_GridSize.z);
                pos.z = (int)(uint(pixel.y * _MainTex_TexelSize.w) % uint(_GridSize.z));
                return pos;
            }

            float2 get_pixel(int3 pos) {
                float2 pixel;
                pixel.x = (float)(pos.x) * _MainTex_TexelSize.x;
                // pixel.y = (pos.y * _GridSize.z + pos.z) * _MainTex_TexelSize.y;
                pixel.y = (float)(pos.y * _GridSize.z + pos.z) * _MainTex_TexelSize.y;
                return pixel;
            }

            fixed frag (v2f i) : SV_Target
            {
                float3 center_pos = get_pos(i.uv);
                // if (center_pos.z < 10) {
                // return center_pos.z / 100.0;
                // } else {
                    // return 0.0;
                // }

                float latest = 0;
                // float3 acc_of_latest = float3(0, 0, 0);

                // for (int offset_id=0; offset_id<27; offset_id++) {
                for (int x=-1; x<=1; x++) {
                    for (int y=-1; y<=1; y++) {
                        for (int z=-1; z<=1; z++) {
                            int3 offset = float3(x, y, z);
                            float2 pixel_offset = get_pixel(offset);
                            float info = tex2D(_MainTex, i.uv + pixel_offset).r;
                            float3 pixel_pos = get_pos(i.uv) + offset; 
                            float origin_frame = info;
                            float3 charge_pos = tex2D(_PosTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rgb;
                            float3 r_vec_center = center_pos - charge_pos;
                            float dist_kernel_center = length(r_vec_center);
                            float dist_circle_center = _FrameCount - origin_frame;
                            if (dist_kernel_center - dist_circle_center < 1) {
                                if (latest < origin_frame) {
                                    latest = origin_frame;
                                    // float3 acc = tex2D(_AccTexture, float2(0, 1) * (_FrameCount - origin_frame) * _PosTexture_TexelSize.xy).rgb;
                                    // float3 acc_perp = acc - dot(acc, normalize(r_vec_cur)) * normalize(r_vec_cur);
                                    // acc_of_latest = _Charge * acc_perp / length(r_vec_cur);
                                }
                            }
                        }
                    }
                }

                // float acc_final = length(acc_of_latest.y);
                // float2 test_acc = tex2D(_AccTexture, float2(0, 0)).rg / 2.0;
                // float4 col = float4((acc_of_latest.x), (acc_of_latest.y), latest, 1);
                float col = latest;
                // float col = 1;
                // float col = tex2D(_MainTex, get_pixel(get_pos(i.uv)));
                // float col = tex2D(_MainTex, i.uv + float2(1, -1) * _MainTex_TexelSize.xy);
                
                if (int(center_pos.x) == int(_Cell.x) && int(center_pos.y) == int(_Cell.y) && int(center_pos.z) == int(_Cell.z)) {
                    // float2 charge_pos = tex2D(_PosTexture, float2(0, 0)).rg;
                    // float2 r_vec_cur = center_pos - charge_pos;
                    // float2 acc = tex2D(_AccTexture, float2(0, 0)).rg;
                    // float2 acc_perp = acc - dot(acc, normalize(r_vec_cur)) * normalize(r_vec_cur);
                    // float2 acc_final = acc_perp / length(r_vec_cur) * 1000.0;
                    // col = float4(0, 0, _FrameCount+1, 1);
                    col = (_FrameCount+1);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
