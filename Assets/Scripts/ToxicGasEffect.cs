using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ToxicGasSimple : MonoBehaviour
{
    [Header("Control")]
    [Tooltip("If TRUE: Gas goes away, Vignette appears.")]
    public bool wearingMask = false;

    [Tooltip("How fast the transition happens.")]
    public float fadeSpeed = 3.0f;

    [Header("Shader Reference")]
    public Shader gasShader;

    [Header("Gas Settings")]
    [Range(0f, 5f)] public float maxGasDensity = 1.5f;
    [Range(0f, 20f)] public float depthFalloff = 8f;
    public Color gasColor = new Color(0.2f, 0.6f, 0.1f, 1f);

    [Header("Mask Vignette Settings")]
    [Range(0f, 1f)] public float maxVignetteStrength = 0.85f;
    public Color vignetteColor = Color.black;

    [Header("Animation")]
    [Range(0f, 2f)] public float speed = 0.5f;
    [Range(1f, 20f)] public float cloudScale = 5f;
    [Range(0f, 0.1f)] public float distortionStrength = 0.02f;

    private Material _material;
    private Camera _cam;

    // Internal animation values
    private float _currentGasDensity;
    private float _currentVignetteStrength;

    private Material Material
    {
        get
        {
            if (_material == null)
            {
                if (gasShader == null) return null;
                _material = new Material(gasShader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        _cam.depthTextureMode = _cam.depthTextureMode | DepthTextureMode.Depth;

        // Snap to initial state
        _currentGasDensity = wearingMask ? 0f : maxGasDensity;
        _currentVignetteStrength = wearingMask ? maxVignetteStrength : 0f;
    }

    void OnDisable()
    {
        if (_material != null) DestroyImmediate(_material);
    }

    void Update()
    {
        // TARGET VALUES
        // Mask ON = Gas 0, Vignette HIGH
        // Mask OFF = Gas HIGH, Vignette 0
        float targetGas = wearingMask ? 0f : maxGasDensity;
        float targetVignette = wearingMask ? maxVignetteStrength : 0f;

        if (Application.isPlaying)
        {
            float dt = Time.deltaTime * fadeSpeed;
            _currentGasDensity = Mathf.Lerp(_currentGasDensity, targetGas, dt);
            _currentVignetteStrength = Mathf.Lerp(_currentVignetteStrength, targetVignette, dt);
        }
        else
        {
            _currentGasDensity = targetGas;
            _currentVignetteStrength = targetVignette;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (gasShader == null || Material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Pass animated values
        Material.SetFloat("_Density", _currentGasDensity);
        Material.SetFloat("_VignetteStrength", _currentVignetteStrength);

        // Pass static/color values
        Material.SetColor("_VignetteColor", vignetteColor);
        Material.SetFloat("_DepthFalloff", depthFalloff);
        Material.SetColor("_GasColor", gasColor);
        Material.SetFloat("_Speed", speed);
        Material.SetFloat("_Scale", cloudScale);
        Material.SetFloat("_Distortion", distortionStrength);

        Graphics.Blit(source, destination, Material);
    }
}