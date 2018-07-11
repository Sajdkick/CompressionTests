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

    private static extern int Test(float[] a_pafSourceRGBA,
                uint a_uiSourceWidth,
                uint a_uiSourceHeight,
                Format a_format,
                ErrorMetric a_eErrMetric,
                float a_fEffort,
                uint a_uiJobs,
                uint a_uimaxJobs,
                out System.IntPtr a_ppaucEncodingBits,
                out uint a_puiEncodingBitsBytes,
                out uint a_puiExtendedWidth,
                out uint a_puiExtendedHeight,
                out uint a_piEncodingTime_ms, bool a_bVerboseOutput = false);
    //private static extern bool TestIfItWorks2(byte[] bytes, out System.IntPtr array, out int arraySize);

    void Awake()
    {

        try
        {
            //byte[] bytes = ;
            System.IntPtr output = System.IntPtr.Zero;
            uint encodingBitsBytes = 0;
            uint extendedWidth = 0;
            uint extendedHeight = 0;
            uint encodingTime = 0;

            float[] pixels = new float[512 * 512 * 4];
            Color[] colors = texture.GetPixels();

            for(int i = 0; i < colors.Length; i++)
            {
                pixels[i + 0] = colors[i].r;
                pixels[i + 1] = colors[i].g;
                pixels[i + 2] = colors[i].b;
                pixels[i + 3] = colors[i].a;
            }

            Test(pixels, 512, 512, Format.RGBA8, ErrorMetric.RGBA, 0.4f, 1, 1, out output, out encodingBitsBytes, out extendedWidth, out extendedHeight, out encodingTime);// System.BitConverter.ToSingle(bytes, 0).ToString();
            text.text = encodingTime.ToString();

            //Texture2D compressedTexture = new Texture2D((int)extendedWidth, (int)extendedHeight, TextureFormat.ETC2_RGBA8, false);
            ////compressedTexture.LoadRawTextureData()
            //display.GetComponent<Renderer>().material.mainTexture = compressedTexture;

        }
        catch (System.Exception e)
        {
            text.text = e.ToString();
        }
        //try
        //{
        //    // Calls the FooPluginFunction inside the plugin
        //    // And prints 5 to the console
        //    byte[] bytes = Resources.Load<TextAsset>("bytes").bytes;//texture.GetRawTextureData();
        //    System.IntPtr arrayValue = System.IntPtr.Zero;
        //    int size = 0;

        //    text.text = "Bytes before: " + bytes.Length + "\n";

        //    if (TestIfItWorks2(bytes, out arrayValue, out size))
        //    {
        //        byte[] compressedBytes = new byte[size];
        //        int byteSize = Marshal.SizeOf(typeof(byte));
        //        for (int i = 0; i < size; i += 4)
        //        {
        //            int intValue = arrayValue.ToInt32();
        //            byte[] intBytes = System.BitConverter.GetBytes(intValue);
        //            if (System.BitConverter.IsLittleEndian)
        //                System.Array.Reverse(intBytes);

        //            compressedBytes[i + 0] = intBytes[0];
        //            compressedBytes[i + 1] = intBytes[1];
        //            compressedBytes[i + 2] = intBytes[2];
        //            compressedBytes[i + 3] = intBytes[3];

        //            arrayValue = new System.IntPtr(arrayValue.ToInt32() + byteSize * 4);
        //        }

        //        //System.IO.File.WriteAllBytes("test.dds", compressedBytes);
        //        text.text += "Bytes after: " + compressedBytes.Length + "\n";

        //        //Texture2D compressedTexture = new Texture2D(512, 512);
        //        //compressedTexture.LoadRawTextureData(compressedBytes);
        //        //display.GetComponent<Renderer>().material.mainTexture = compressedTexture;
        //    }

        //}
        //catch (System.Exception e)
        //{
        //    text.text = e.Message;
        //}

    }
}
