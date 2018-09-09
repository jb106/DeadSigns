using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(HungerRenderer), PostProcessEvent.AfterStack, "Custom/Hunger")]
public sealed class Hunger : PostProcessEffectSettings
{
    public TextureParameter hungerDiffuse = new TextureParameter { };
    public TextureParameter hungerNormal = new TextureParameter { };

    [Range(0f, 1f), Tooltip("Hunger Amount")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };

    [Range(0f, 1f), Tooltip("Distortion")]
    public FloatParameter distortion = new FloatParameter { value = 0f };
}

public sealed class HungerRenderer : PostProcessEffectRenderer<Hunger>

{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Hunger"));
        sheet.properties.SetFloat("_Blend", settings.blend);
        sheet.properties.SetTexture("_Diffuse", settings.hungerDiffuse);
        sheet.properties.SetTexture("_Normal", settings.hungerNormal);
        sheet.properties.SetFloat("_Distortion", settings.distortion);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}