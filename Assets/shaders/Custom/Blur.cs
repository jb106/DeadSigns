using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlurRenderer), PostProcessEvent.AfterStack, "Custom/Blur")]
public sealed class Blur : PostProcessEffectSettings
{
    public TextureParameter blurTexture = new TextureParameter { };
    [Range(0f, 1f), Tooltip("Blur Amount")]
    public FloatParameter blend = new FloatParameter { value = 0f };

}

public sealed class BlurRenderer : PostProcessEffectRenderer<Blur>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Blur"));
        sheet.properties.SetTexture("_BlurTex", settings.blurTexture);
        sheet.properties.SetFloat("_Blend", settings.blend);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}