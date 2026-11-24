using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public class PauseBackgroundBlur : CustomPass
{
    [SerializeField] private Material blurMaterial;
    [SerializeField, Range(0.1f, 16.0f)]
    private float radius = 2.0f;

    private RTHandle blurTexture;

    private static readonly int RadiusId = Shader.PropertyToID("_Radius");

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        blurTexture = RTHandles.Alloc(
            Vector2.one * 0.5f,
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