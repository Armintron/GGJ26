using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ToxicGasRenderer), PostProcessEvent.BeforeStack, "Custom/ToxicGas")]
public sealed class ToxicGas : PostProcessEffectSettings
{
    // Default 0 = Mask OFF (Gas Active). 
    // We keep this 0 so the gas is visible by default when you enter the zone.
    [Range(0f, 1f), Tooltip("0 = Gas (Mask Off), 1 = Clear (Mask On)")]
    public FloatParameter maskProgress = new FloatParameter { value = 0f };

    [Header("Gas Settings")]
    // CHANGE: Default is now 0. This ensures that "Outside the zone" = No Gas.
    [Range(0f, 5f)]
    public FloatParameter maxGasDensity = new FloatParameter { value = 0f };

    [Range(0f, 20f)]
    public FloatParameter depthFalloff = new FloatParameter { value = 8f };

    public ColorParameter gasColor = new ColorParameter { value = new Color(0.2f, 0.6f, 0.1f, 1f) };

    [Header("Animation")]
    public FloatParameter speed = new FloatParameter { value = 0.5f };
    public FloatParameter cloudScale = new FloatParameter { value = 5f };

    // CHANGE: Default is now 0 so distortion fades out at distance.
    public FloatParameter maxDistortion = new FloatParameter { value = 0f };

    [Header("Vignette Settings")]
    // We keep this High by default so the Mask Vignette works even outside the gas zone
    [Range(0f, 1f)]
    public FloatParameter maxVignetteStrength = new FloatParameter { value = 0.85f };
    public ColorParameter vignetteColor = new ColorParameter { value = Color.black };

    [Header("Unlit Visibility")]
    [Range(0f, 1.2f)]
    public FloatParameter unlitThreshold = new FloatParameter { value = 0.8f };
    [Range(0f, 1f)]
    public FloatParameter unlitStrength = new FloatParameter { value = 1.0f };
}

public sealed class ToxicGasRenderer : PostProcessEffectRenderer<ToxicGas>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/ToxicGasPPS"));

        // Calculate values based on Mask Progress
        float currentGas = Mathf.Lerp(settings.maxGasDensity, 0f, settings.maskProgress);
        float currentDistortion = Mathf.Lerp(settings.maxDistortion, 0f, settings.maskProgress);
        float currentVignette = Mathf.Lerp(0f, settings.maxVignetteStrength, settings.maskProgress);

        sheet.properties.SetFloat("_Density", currentGas);
        sheet.properties.SetFloat("_Distortion", currentDistortion);
        sheet.properties.SetFloat("_VignetteStrength", currentVignette);

        // Static Props
        sheet.properties.SetColor("_GasColor", settings.gasColor);
        sheet.properties.SetFloat("_DepthFalloff", settings.depthFalloff);
        sheet.properties.SetFloat("_Speed", settings.speed);
        sheet.properties.SetFloat("_Scale", settings.cloudScale);
        sheet.properties.SetColor("_VignetteColor", settings.vignetteColor);
        sheet.properties.SetFloat("_UnlitThreshold", settings.unlitThreshold);
        sheet.properties.SetFloat("_UnlitStrength", settings.unlitStrength);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}