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

            sampler2D _PosTexture;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _PosTexture_TexelSize;
            float4 _Cell;
            int _FrameCount;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center_pos = i.uv * _MainTex_TexelSize.zw;
             
                float latest = 0;
                float2 pos_of_latest = float2(0, 0);
                for (int x=-1; x<=1; x++) {
                    for (int y=-1; y<=1; y++) {
                        float3 info = tex2D(_MainTex, i.uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
                        float2 pixel_pos = i.uv * _MainTex_TexelSize.zw + float2(x, y); 
                        float origin_frame = info.b;
                        float2 charge_pos = info.rg * _MainTex_TexelSize.zw;
                        float2 r_vec_cur = pixel_pos - charge_pos;
                        float2 r_vec_center = center_pos - charge_pos;
                        float dist_kernel_center = length(r_vec_center);
                        float dist_circle_center = _FrameCount - origin_frame;
                        if (dist_kernel_center - dist_circle_center < 1) {
                            if (latest < origin_frame) {
                                latest = origin_frame;
                                pos_of_latest = charge_pos * _MainTex_TexelSize.xy;
                            }
                        }
                    }
                }

                float4 col = float4(pos_of_latest.x, pos_of_latest.y, latest, 1);
                // col.r = pos_of_latest.x;
                // col.g = pos_of_latest.y;
                // col.b = latest;
                
                if (int(center_pos.x) == int(_Cell.x) && int(center_pos.y) == int(_Cell.y)) {
                    float2 charge_pos_unit = tex2D(_PosTexture, float2(0, 0)).rg * _MainTex_TexelSize.xy;
                    col = float4(charge_pos_unit.x, charge_pos_unit.y, _FrameCount+1, 1);
                }
                return col;
                // if (center_pos.x >= _Cell.x-1 && center_pos.x <= _Cell.x+1 && center_pos.y >= _Cell.y-1 && center_pos.y <= _Cell.y+1) {
                //     latest = 0;
                //     pos_of_latest = float2(0, 0);
                //     float2 charge_pos = tex2D(_PosTexture, float2(0, 0)).rg;
                //     for (int x=-1; x<=1; x++) {
                //         for (int y=-1; y<=1; y++) {
                //             float2 pixel_pos = (i.uv + float2(x, y)) * _MainTex_TexelSize.zw; 
                //             // int index = _FrameCount - int(info.b);
                //             // float2 index_uv = index * _PosTexture_TexelSize.xy;
                //             int origin_frame = int(_FrameCount);
                //             float2 r_vec_cur = pixel_pos - charge_pos;
                //             float dist_cur = length(r_vec_cur);
                //             float r_vec_center = center_pos - charge_pos;
                //             float dist_center = length(r_vec_center);
                //             if ((dist_cur + 1) < dist_center) {
                //                 if (latest < origin_frame) {
                //                     latest = origin_frame;
                //                     pos_of_latest = charge_pos;
                //                 }
                //             }
                //         }
                //     }
                // }
                // col.r = pos_of_latest.x;
                // col.g = pos_of_latest.y;
                // col.b = latest;
                // // col.g = col.b = col.r;
                // // col.r = tex2D(_PosTexture, float2(0, 0)).r / 100.0;
                // // col.g = tex2D(_PosTexture, float2(0, 0)).g / 100.0;
                // // col.r = tex2D(_PosTexture, float2(_PosTexture_TexelSize.x, _PosTexture_TexelSize.y)).r;
                // // col.g = tex2D(_PosTexture, float2(_PosTexture_TexelSize.x, _PosTexture_TexelSize.y)).g;
                // col.a = 1;
                // return col;
            }
            ENDCG
        }
    }
}
