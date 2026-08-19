// ADR-0003: occlusion-independent attack telegraph channel.
//
// A tiny, hand-written unlit shader used only by AttackTelegraphIndicator (Boxhead.Core).
// The single load-bearing state here is "ZTest Always" + "ZWrite Off": the indicator draws
// regardless of what is already in the depth buffer, which is how it stays visible through
// walls/props without a URP Decal Renderer Feature or a depth prepass (both explicitly
// rejected in ADR-0003 on mobile cost grounds — Mobile_Renderer.asset has no decal support and
// adding one would impose a permanent full-screen depth prepass on every scene).
//
// "Queue"="Transparent+100" additionally makes sure it draws after ordinary transparent
// geometry, so overlapping transparent props do not draw on top of it by queue order alone.
Shader "BoxForged/TelegraphOverlayUnlit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
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
            ZTest Always
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
