using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PauseBackgroundBlur : CustomPass
{
    [SerializeField] private Material blurMaterial;
    [SerializeField, Range(1, 8)] private int iterations = 4;
    [SerializeField, Range(1.0f, 16.0f)] private float radius = 2.0f;

    private RTHandle blurTexture;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        blurTexture = RTHandles.Alloc(
            Vector2.one * 0.5f,
            colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
            useDynamicScale: true,
            name: "Pause Background Blur"
        );
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (blurMaterial == null)
            return;
        
        blurMaterial.SetFloat("_Raidus", radius);
        HDUtils.BlitCameraTexture(
            ctx.cmd,
            ctx.cameraColorBuffer,
            blurTexture,
            blurMaterial,
            0
        );
        for (int i = 1; i < iterations; i++)
        {
            blurMaterial.SetFloat("_Radius", radius * (i +1));
            HDUtils.BlitCameraTexture(
                ctx.cmd,
                blurTexture,
                ctx.cameraColorBuffer,
                blurMaterial,
                0
            );
            HDUtils.BlitCameraTexture(
                ctx.cmd,
                ctx.cameraColorBuffer,
                blurTexture,
                blurMaterial,
                0
            );
        }

        HDUtils.BlitCameraTexture(
            ctx.cmd,
            blurTexture,
            ctx.cameraColorBuffer
        );
    }

    protected override void Cleanup()
    {
        blurTexture?.Release();
    }
}
