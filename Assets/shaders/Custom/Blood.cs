using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BloodRenderer), PostProcessEvent.AfterStack, "Custom/Blood")]
public sealed class Blood : PostProcessEffectSettings
{
    public TextureParameter bloodDiffuse = new TextureParameter { };
    public TextureParameter bloodNormal = new TextureParameter { };

    [Range(0f, 1f), Tooltip("Blood Amount")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };

    [Range(0f, 1f), Tooltip("Distortion")]
    public FloatParameter distortion = new FloatParameter { value = 0f };
}

public sealed class BloodRenderer : PostProcessEffectRenderer<Blood>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Blood"));
        sheet.properties.SetFloat("_Blend", settings.blend);
        sheet.properties.SetTexture("_Diffuse", settings.bloodDiffuse);
        sheet.properties.SetTexture("_Normal", settings.bloodNormal);
        sheet.properties.SetFloat("_Distortion", settings.distortion);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}