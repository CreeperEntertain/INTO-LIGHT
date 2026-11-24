using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public class PauseBackgroundBlur : CustomPass
{
    [SerializeField] private Material blurMaterial;
    [SerializeField, Range(0.0f, 16.0f)] private float radius = 2.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float minimumBrightness = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float maximumBrightness = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float blend = 1.0f;

    private RTHandle blurTexture;

    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int MinBrightnessId = Shader.PropertyToID("_MinBrightness");
    private static readonly int MaxBrightnessId = Shader.PropertyToID("_MaxBrightness");
    private static readonly int BlendId = Shader.PropertyToID("_Blend");

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        blurTexture = RTHandles.Alloc(
            Vector2.one,
            colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat,
            useDynamicScale: true,
            name: "Pause Background Blur"
        );
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (blurMaterial == null || blurTexture == null)
            return;

        blurMaterial.SetFloat(RadiusId, radius);
        blurMaterial.SetFloat(MinBrightnessId, minimumBrightness);
        blurMaterial.SetFloat(MaxBrightnessId, maximumBrightness);
        blurMaterial.SetFloat(BlendId, blend);

        HDUtils.BlitCameraTexture(
            ctx.cmd,
            ctx.cameraColorBuffer,
            blurTexture,
            blurMaterial,
            0
        );
        HDUtils.BlitCameraTexture(
            ctx.cmd,
            blurTexture,
            ctx.cameraColorBuffer
        );
    }

    protected override void Cleanup()
    {
        blurTexture?.Release();
        blurTexture = null;
    }
}