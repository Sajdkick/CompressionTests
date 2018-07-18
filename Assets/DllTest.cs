using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DllTest : MonoBehaviour
{

    public Texture2D texture;
    public GameObject display;
    public UnityEngine.UI.Text text;

    enum Format
    {
        UNKNOWN,
			//
			ETC1,
			//
			// ETC2 formats
			RGB8,
			SRGB8,
			RGBA8,
			SRGBA8,
			R11,
			SIGNED_R11,
			RG11,
			SIGNED_RG11,
			RGB8A1,
			SRGB8A1,
			//
			FORMATS,
			//
			DEFAULT = SRGB8
    };

    enum ErrorMetric
    {
        RGBA,
        RGBX,
        REC709,
        NUMERIC,
        NORMALXYZ,
        //
        ERROR_METRICS,
        //
        BT709 = REC709
    };

#if UNITY_IPHONE
   
       // On iOS plugins are statically linked into
       // the executable, so we have to use __Internal as the
       // library name.
       [DllImport ("__Internal")]

#else

    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    //[DllImport("crnlib", EntryPoint = "TestIfItWorks2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    //[return: MarshalAs(UnmanagedType.I1)]
    [DllImport("hello", EntryPoint = "Encode")]

#endif

    private static unsafe extern int Encode(float[] a_pafSourceRGBA,
                uint a_uiSourceWidth,
                uint a_uiSourceHeight,
                Format a_format,
                ErrorMetric a_eErrMetric,
                float a_fEffort,
                uint a_uiJobs,
                uint a_uimaxJobs,
                out char* a_ppaucEncodingBits,
                out uint a_puiEncodingBitsBytes,
                out uint a_puiExtendedWidth,
                out uint a_puiExtendedHeight,
                out uint a_piEncodingTime_ms, bool a_bVerboseOutput = false);

    void Awake()
    {

        try
        {


            Texture2D compressedTexture = CompressTexture(texture);
            display.GetComponent<Renderer>().material.mainTexture = compressedTexture;

        }
        catch (System.Exception e)
        {
            text.text = e.ToString();
        }
    }

    unsafe Texture2D CompressTexture(Texture2D texture)
    {
        //////byte[] bytes = ;
        char* output;
        uint encodingBitsBytes = 0;
        uint extendedWidth = 0;
        uint extendedHeight = 0;
        uint encodingTime = 0;

        float[] pixels = new float[texture.width * texture.height * 4];
        Color[] colors = texture.GetPixels();

        for (int i = 0; i < colors.Length; i++)
        {
            pixels[i * 4 + 0] = colors[i].r;
            pixels[i * 4 + 1] = colors[i].g;
            pixels[i * 4 + 2] = colors[i].b;
            pixels[i * 4 + 3] = colors[i].a;
        }

        Encode(pixels, (uint)texture.width, (uint)texture.height, Format.SRGB8A1, ErrorMetric.RGBA, 0.4f, 1, 1, out output, out encodingBitsBytes, out extendedWidth, out extendedHeight, out encodingTime);// System.BitConverter.ToSingle(bytes, 0).ToString();
        text.text = encodingTime.ToString();

        byte[] compressedBytes = new byte[encodingBitsBytes];

        for (int i = 0; i < encodingBitsBytes / 2; i++)
        {
            char c = output[i];
            byte[] bytes = System.BitConverter.GetBytes(c);
            compressedBytes[i * 2 + 0] = bytes[0];
            compressedBytes[i * 2 + 1] = bytes[1];
        }

        Texture2D compressedTexture = new Texture2D((int)extendedWidth, (int)extendedHeight, TextureFormat.ETC2_RGBA1, false);
        //System.Array.Reverse(compressedBytes);
        compressedTexture.LoadRawTextureData(compressedBytes);
        compressedTexture.Apply();

        //System.IO.File.WriteAllBytes("UnityTest.ktx", compressedBytes);

        return texture;
    }
}
