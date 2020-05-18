using UnityEngine;

[ExecuteInEditMode]
public class ColorGrading : MonoBehaviour
{
    public Color Color = Color.white;
    [Range(-180, 180)]
    public int Hue = 0;
    [Range(0, 1)]
    public float Contrast = 0f;
    [Range(-1, 1)]
    public float Brightness = 0f;
    [Range(-1, 1)]
    public float Saturation = 0f;
    [Range(-1, 1)]
    public float Exposure = 0f;
    [Range(-1, 1)]
    public float Gamma = 0f;
    [Range(0, 1)]
    public float Sharpness = 0f;
    [Range(0, 1)]
    public float Blur = 0f;
    public Texture2D BlurMask;
    [Range(0, 1)]
    public float Vignette = 0f;

    public Material material;
    static readonly int color = Shader.PropertyToID("_Color");
    static readonly int hueCos = Shader.PropertyToID("_HueCos");
    static readonly int hueSin = Shader.PropertyToID("_HueSin");
    static readonly int hueVector = Shader.PropertyToID("_HueVector");
    static readonly int contrast = Shader.PropertyToID("_Contrast");
    static readonly int brightness = Shader.PropertyToID("_Brightness");
    static readonly int saturation = Shader.PropertyToID("_Saturation");
    static readonly int exposure = Shader.PropertyToID("_Exposure");
    static readonly int gamma = Shader.PropertyToID("_Gamma");
    static readonly int centralFactor = Shader.PropertyToID("_CentralFactor");
    static readonly int sideFactor = Shader.PropertyToID("_SideFactor");
    static readonly int blur = Shader.PropertyToID("_Blur");
    static readonly int blurMask = Shader.PropertyToID("_MaskTex");
    static readonly int vignette = Shader.PropertyToID("_Vignette");

    static readonly string blurKeyword = "BLUR";
    static readonly string shaprenKeyword = "SHARPEN";

    private void Start()
    {
        if (BlurMask == null)
        {
            Shader.SetGlobalTexture(blurMask, Texture2D.whiteTexture);
        }
        else
        {
            Shader.SetGlobalTexture(blurMask, BlurMask);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetColor(color, Color);
        material.SetFloat(hueCos, Mathf.Cos(Mathf.Deg2Rad * Hue));
        material.SetFloat(hueSin, Mathf.Sin(Mathf.Deg2Rad * Hue));
        material.SetVector(hueVector, new Vector3(0.57735f, 0.57735f, 0.57735f));
        material.SetFloat(contrast, Contrast + 1f);
        material.SetFloat(brightness, Brightness * 0.5f + 0.5f);
        material.SetFloat(saturation, Saturation + 1f);
        material.SetFloat(exposure, Exposure);
        material.SetFloat(gamma, Gamma);
        material.SetFloat(vignette, Vignette * 2.5f);

        if (Blur > 0)
        {
            material.EnableKeyword(blurKeyword);
            material.SetFloat(blur, Blur);
        }
        else
        {
            material.DisableKeyword(blurKeyword);
        }

        if (Sharpness > 0)
        {
            material.EnableKeyword(shaprenKeyword);
            material.SetFloat(centralFactor, 1.0f + (3.2f * Sharpness));
            material.SetFloat(sideFactor, 0.8f * Sharpness);
        }
        else
        {
            material.DisableKeyword(shaprenKeyword);
        }

        Graphics.Blit(source, destination, material, 0);
    }
}
