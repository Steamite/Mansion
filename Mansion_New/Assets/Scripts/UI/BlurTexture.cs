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

        public async Awaitable<Sprite> Blur()
        {
            width = Screen.width;
            height = Screen.height;

            Debug.Log("starting to render");
            Camera c = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            c.enabled = true;
            screenShot = new RenderTexture(width, height, 24);
            c.targetTexture = screenShot;

            c.Render();
            return await WaitOnPostRender();
        }

        /// <summary>
        /// Assigns the background and sets up the orbital camera.
        /// </summary>
        /// <param name="_item">Item for inspection</param>
        /// <param name="_asset">Asset containging actions.</param>
        /// <returns></returns>
        async Awaitable<Sprite> WaitOnPostRender()
        {
            // Create the picture
            await Awaitable.EndOfFrameAsync();
            RenderTexture renH = RenderTexture.GetTemporary(width, height);

            horizontalMaterial.SetFloat("_BlurSize", Radial);
            verticalMaterial.SetFloat("_BlurSize", Radial);

            Graphics.Blit(screenShot, renH, horizontalMaterial);
            Graphics.Blit(renH, screenShot, verticalMaterial);
            RenderTexture.ReleaseTemporary(renH);

            Debug.Log("Shaded");
            return await CreateSprite();
        }

        async Awaitable<Sprite> CreateSprite()
        {
            NativeArray<byte> bytArr = 
                new NativeArray<byte>(
                    screenShot.width * screenShot.height * 16, 
                    Allocator.Persistent, 
                    NativeArrayOptions.UninitializedMemory);
            Texture2D blurTexture;
            try
            {

                await AsyncGPUReadback.RequestIntoNativeArrayAsync(
                    ref bytArr,
                    screenShot);

                blurTexture = new Texture2D(
                    screenShot.width,
                    screenShot.height);

                blurTexture.LoadRawTextureData(bytArr);
                blurTexture.Apply();
                Debug.Log("Texture created");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
            finally
            {
                bytArr.Dispose();
            }

            return Sprite.Create(
                blurTexture,
                new(0, 0, blurTexture.width, blurTexture.height),
                new(0, 0));
        }
    }
}
