using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{
    private static ScreenshotHandler instance;
    public Camera myCamera;
    private bool takeScreenshotOnNextFrame;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            TakeScreenshot(1024 , 1024);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            MenuSkinController.instance.GetSkin().GetComponent<Animator>().SetTrigger("Wave");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            MenuSkinController.instance.GetSkin().GetComponent<Animator>().SetTrigger("Talking");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            MenuSkinController.instance.GetSkin().GetComponent<Animator>().SetTrigger("Victory");
        }

    }

    private void OnPostRender()
    {
        if(takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/_Textures/SkinTextures/" + MenuSkinController.instance.GetSkin().name + "Icon4" +
                ".png", byteArray);
            Debug.Log("Saved " + MenuSkinController.instance.GetSkin().name);

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;
        }
    }

    void TakeScreenshot(int width, int height)
    {
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 256);
        takeScreenshotOnNextFrame = true;
    }

    public static void TakeScreenshot_Static(int width, int height)
    {
        instance.TakeScreenshot(width, height);
    }


}
