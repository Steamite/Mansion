using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Blur
{
    public class NewBlur : MonoBehaviour
    {
        public Material blurMaterial; // Assign a Gaussian Blur shader material here
        public int blurIterations = 3; // More iterations = smoother blur

        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        public void CaptureAndBlurScreen()
        {
            StartCoroutine(CaptureScreenshot());
        }

        private IEnumerator CaptureScreenshot()
        {
            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;

            // Capture the screen into a temporary RenderTexture
            RenderTexture rt = new RenderTexture(width, height, 0);
            mainCamera.targetTexture = rt;
            mainCamera.Render();
            mainCamera.targetTexture = null;

            // Apply blur effect
            RenderTexture blurredTexture = ApplyGaussianBlur(rt);

            // Convert RenderTexture to Texture2D
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = blurredTexture;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;

            // Cleanup
            Destroy(rt);
            Destroy(blurredTexture);

            // Convert to sprite
            Sprite blurredSprite = Sprite.Create(screenshot, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));

            // Example: Assign to UI Image
            GetComponent<Image>().sprite = blurredSprite;
        }

        private RenderTexture ApplyGaussianBlur(RenderTexture source)
        {
            RenderTexture temp1 = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width, source.height, 0);

            Graphics.Blit(source, temp1); // Copy the original image

            for (int i = 0; i < blurIterations; i++)
            {
                Graphics.Blit(temp1, temp2, blurMaterial, 0); // Horizontal blur
                Graphics.Blit(temp2, temp1, blurMaterial, 1); // Vertical blur
            }

            RenderTexture.ReleaseTemporary(temp2);
            return temp1;
        }

    }
}
