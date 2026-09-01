// ADR-0003: occlusion-independent attack telegraph channel.
//
// A tiny, hand-written unlit shader used by AttackTelegraphIndicator and (ADR-0007)
// AttackTelegraphLane (both Boxhead.Core). The billboard indicator's load-bearing state is
// "ZTest Always" + "ZWrite Off": it draws regardless of what is already in the depth buffer,
// which is how it stays visible through walls/props without a URP Decal Renderer Feature or a
// depth prepass (both explicitly rejected in ADR-0003 on mobile cost grounds —
// Mobile_Renderer.asset has no decal support and adding one would impose a permanent
// full-screen depth prepass on every scene).
//
// ADR-0007 promotes ZTest to a material property so a ground-plane lane can opt into normal
// depth testing (LEqual, via mat_TelegraphLane.mat) instead — a floor marking should be
// occluded by nothing (M2 caps interior obstructions too small to hide a 19 m band) but should
// still be stood ON by the player/boss rather than drawing through them, which ZTest Always
// would do. Default is 8 (Always), so mat_TelegraphOverlay.mat and the shipped billboard path
// are completely unaffected — see ADR-0007 §Validation 10.
//
// "Queue"="Transparent+100" additionally makes sure it draws after ordinary transparent
// geometry, so overlapping transparent props do not draw on top of it by queue order alone.
Shader "BoxForged/TelegraphOverlayUnlit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "TelegraphUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest [_ZTest]
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
