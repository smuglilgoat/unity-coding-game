using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(EdgeEffectRenderer), PostProcessEvent.BeforeStack, "Hidden/Edge Effect")]
public sealed class EdgeEffect : PostProcessEffectSettings
{
    public IntParameter edge_size = new IntParameter { value = 2 };
    public FloatParameter depth_threshold = new FloatParameter { value = 2f };
    public FloatParameter normal_threshold = new FloatParameter { value = 0.4f };
    public FloatParameter depth_normal_threshold = new FloatParameter { value = 0.4f };
    public FloatParameter depth_threshold_scale = new FloatParameter { value = 1f };
}

public sealed class EdgeEffectRenderer : PostProcessEffectRenderer<EdgeEffect>
{

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Edge Effect"));
        sheet.properties.SetFloat("_edge_size", settings.edge_size);
        sheet.properties.SetFloat("_depth_threshold", settings.depth_threshold);
        sheet.properties.SetFloat("_normal_threshold", settings.normal_threshold);
        sheet.properties.SetFloat("_depth_normal_threshold", settings.depth_normal_threshold);
        sheet.properties.SetFloat("_depth_threshold_scale", settings.depth_threshold_scale);

        Matrix4x4 clip_to_view = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, true).inverse;
        sheet.properties.SetMatrix("_clip_to_view", clip_to_view);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}