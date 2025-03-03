using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class GaussianBlur
{
    const int RADIAL = 5;
    static ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16 };

    static int width;
    static int height;

    public static Sprite Blur()
    {
        width = Screen.width;
        height = Screen.height;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply(); // Apply texture changes before reading pixels


        /*
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply(); // Apply texture changes before reading pixels*/

        Color32[] source = tex.GetPixels32();

        // Initialize color channels
        int[] _red = new int[source.Length];
        int[] _green = new int[source.Length];
        int[] _blue = new int[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            _red[i] = source[i].r;
            _green[i] = source[i].g;
            _blue[i] = source[i].b;
        }/*
        Parallel.For(0, source.Length, _pOptions, i =>
        {
            _alpha[i] = source[i].a;
            _red[i] = source[i].r;
            _green[i] = source[i].g;
            _blue[i] = source[i].b;
        });*/

        // Process blurring
        int[] newRed = new int[source.Length];
        int[] newGreen = new int[source.Length];
        int[] newBlue = new int[source.Length];

        gaussBlur_4(_red, newRed, RADIAL);
        gaussBlur_4(_green, newGreen, RADIAL);
        gaussBlur_4(_blue, newBlue, RADIAL);
        /*
        Parallel.Invoke(
            () => gaussBlur_4(_alpha, newAlpha, RADIAL),
            () => gaussBlur_4(_red, newRed, RADIAL),
            () => gaussBlur_4(_green, newGreen, RADIAL),
            () => gaussBlur_4(_blue, newBlue, RADIAL)
        );*/

        Color32[] colors = new Color32[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            colors[i] = new Color32(
                (byte)Mathf.Clamp(newRed[i] - 10, 0, 255),
                (byte)Mathf.Clamp(newGreen[i] - 10, 0, 255),
                (byte)Mathf.Clamp(newBlue[i] - 10, 0, 255),
                255
            );
        }


        /*Parallel.For(0, colors.Length, _pOptions, i =>
        {
            colors[i] = new Color32(
                (byte)Mathf.Clamp(newRed[i]-10, 0, 255),
                (byte)Mathf.Clamp(newGreen[i]-10, 0, 255),
                (byte)Mathf.Clamp(newBlue[i]-10, 0, 255),
                (byte)Mathf.Clamp(newAlpha[i], 0, 255)
            );
        });*/

        Texture2D blurredTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        blurredTex.SetPixels32(colors);
        blurredTex.Apply();

        return Sprite.Create(blurredTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    static async void gaussBlur_4(int[] source, int[] dest, int r)
    {
        var bxs = boxesForGauss(r, 3);
        await Task.Run(() => boxBlur_4(source, dest, width, height, (bxs[0] - 1) / 2));
        await Task.Run(() => boxBlur_4(dest, source, width, height, (bxs[1] - 1) / 2));
        await Task.Run(() => boxBlur_4(source, dest, width, height, (bxs[2] - 1) / 2));
    }

    static int[] boxesForGauss(int sigma, int n)
    {
        var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
        int wl = (int)Math.Floor(wIdeal);
        if (wl % 2 == 0) wl--;
        int wu = wl + 2;

        double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
        var m = Math.Round(mIdeal);

        List<int> sizes = new List<int>();
        for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
        return sizes.ToArray();
    }

    static void boxBlur_4(int[] scl, int[] tcl, int w, int h, int r)
    {
        Array.Copy(scl, tcl, scl.Length);
        boxBlurH_4(tcl, scl, w, h, r);
        boxBlurT_4(scl, tcl, w, h, r);
    }

    static void boxBlurH_4(int[] scl, int[] tcl, int w, int h, int r)
    {
        double iarr = 1.0 / (r + r + 1);
        for (var i = 0; i < h; i++)
        {
            int ti = i * w, li = ti, ri = ti + r;
            int fv = scl[ti], lv = scl[ti + w - 1], val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += scl[ti + j];

            for (var j = 0; j <= r; j++)
            {
                val += scl[ri++] - fv;
                tcl[ti++] = (int)Math.Round(val * iarr);
            }
            for (var j = r + 1; j < w - r; j++)
            {
                val += scl[ri++] - scl[li++];
                tcl[ti++] = (int)Math.Round(val * iarr);
            }
            for (var j = w - r; j < w; j++)
            {
                val += lv - scl[li++];
                tcl[ti++] = (int)Math.Round(val * iarr);
            }
        }
    }

    static void boxBlurT_4(int[] scl, int[] tcl, int w, int h, int r)
    {
        double iarr = 1.0 / (r + r + 1);
        for (var i = 0; i < w; i++)
        {
            int ti = i, li = ti, ri = ti + r * w;
            int fv = scl[ti], lv = scl[ti + w * (h - 1)], val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += scl[ti + j * w];

            for (var j = 0; j <= r; j++)
            {
                val += scl[ri] - fv;
                tcl[ti] = (int)Math.Round(val * iarr);
                ri += w; ti += w;
            }
            for (var j = r + 1; j < h - r; j++)
            {
                val += scl[ri] - scl[li];
                tcl[ti] = (int)Math.Round(val * iarr);
                li += w; ri += w; ti += w;
            }
            for (var j = h - r; j < h; j++)
            {
                val += lv - scl[li];
                tcl[ti] = (int)Math.Round(val * iarr);
                li += w; ti += w;
            }
        }
    }
}
