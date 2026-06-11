using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.UI
{
    class BlurTexture : MonoBehaviour
    {
        [Header("Blur")]
        [SerializeField] Material horizontalMaterial;
        [SerializeField] Material verticalMaterial;
        [SerializeField] int Radial = 3;

        RenderTexture screenShot;

        int width;
        int height;

        public void Blur(Action<Sprite> onFinish)
        {
            width = Screen.width;
            height = Screen.height;

            Debug.Log("starting to render");
            Camera c = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            c.enabled = true;
            screenShot = new RenderTexture(width, height, 24);
            c.targetTexture = screenShot;

            c.Render();
            StartCoroutine(WaitOnPostRender(onFinish));
        }

        /// <summary>
        /// Assigns the background and sets up the orbital camera.
        /// </summary>
        /// <param name="_item">Item for inspection</param>
        /// <param name="_asset">Asset containging actions.</param>
        /// <returns></returns>
        IEnumerator WaitOnPostRender(Action<Sprite> onFinish)
        {
            // Create the picture
            yield return new WaitForEndOfFrame();

            RenderTexture renH = RenderTexture.GetTemporary(width, height);

            horizontalMaterial.SetFloat("_BlurSize", Radial);
            verticalMaterial.SetFloat("_BlurSize", Radial);

            Graphics.Blit(screenShot, renH, horizontalMaterial);
            Graphics.Blit(renH, screenShot, verticalMaterial);
            RenderTexture.ReleaseTemporary(renH);

            Debug.Log("Shaded");
            CreateSprite(onFinish);
        }

        async void CreateSprite(Action<Sprite> onFinish)
        {
            NativeArray<byte> tempvar = new NativeArray<byte>(screenShot.width * screenShot.height * 16, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var req = AsyncGPUReadback.RequestIntoNativeArrayAsync(ref tempvar, screenShot);
            await req;

            Texture2D blurTexture = new Texture2D(screenShot.width, screenShot.height);
            blurTexture.LoadRawTextureData(tempvar);
            blurTexture.Apply();

            Sprite finalBlur = Sprite.Create(blurTexture, new(0, 0, blurTexture.width, blurTexture.height), new(0, 0));
            onFinish?.Invoke(finalBlur);
        }
    }
}
