using System;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// ETC1 texture compressor for Android.
/// </summary>
/// <remarks></remarks>
/// <example>
/// <code>
/// CompressTexture compressor = new CompressTexture(texture);
/// 
/// yield return StartCoroutine(compressor);
/// 
/// if (compressor.compressedTexture != null)
///	{
///		// ...
///	}
/// </code>
/// </example>
/// <example>
/// <code>
/// CompressTexture compressor = new CompressTexture(texture);
/// 
/// StartCoroutine(compressor);
/// 
/// // Do something else.
/// 
/// while (!compressor.isDone)
///		yield return null;
/// 
///	if (compressor.compressedTexture != null)
///	{
///		// ...
///	}
/// </code>
/// </example>
class CompressTexture : Midworld.UnityCoroutine
{
	/// <summary>
	/// Compressed texture.
	/// </summary>
	/// <remarks>
	/// If it is null, compressing is failed.
	/// </remarks>
	public Texture2D compressedTexture = null;

	/// <summary>
	/// Compress the texture to ETC1 format.
	/// </summary>
	/// <param name="texture">Input texture.</param>
	public CompressTexture(Texture2D texture) : this(texture, true) { }

	/// <summary>
	/// Compress the texture to ETC1 format.
	/// </summary>
	/// <param name="texture">Input texture.</param>
	/// <param name="removeReadableMemory">
	/// Remove readable color data from main memory(It may occur a small lag with a big texture).
	/// </param>
	public CompressTexture(Texture2D texture, bool removeReadableMemory)
	{
#if UNITY_EDITOR && UNITY_ANDROID
        CompressDXT(texture);
        //CompressETC(texture, removeReadableMemory);
#elif UNITY_ANDROID

#else
        Debug.LogWarning("CompressTexture only supports Android.");
		
		compressedTexture = null;
		isDone = true;
#endif
    }

    void CompressDXT(Texture2D texture)
    {
        try
        {
            compressedTexture = new Texture2D(texture.width, texture.height,
                TextureFormat.RGBA32, texture.mipmapCount > 1);
            compressedTexture.name = texture.name;

            for (int i = 0; i < texture.mipmapCount; i++)
            {
                compressedTexture.SetPixels32(texture.GetPixels32(i), i);
            }

            compressedTexture.Compress(true);
            compressedTexture.Apply(true, true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            if (compressedTexture != null)
                GameObject.Destroy(compressedTexture);

            compressedTexture = null;
        }

        isDone = true;
    }

    void CompressETC(Texture2D texture, bool removeReadableMemory)
    {
        Texture2D compressed = new Texture2D(texture.width, texture.height, TextureFormat.ETC_RGB4, texture.mipmapCount > 1);
        compressed.name = texture.name;

        routines.Enqueue(() =>
        {
            GCHandle[] dataHandles = new GCHandle[texture.mipmapCount];
            IntPtr[] dataPtrs = new IntPtr[texture.mipmapCount];

            try
            {
                for (int i = 0; i < texture.mipmapCount; i++)
                {
                    Color32[] pixels = texture.GetPixels32(i);
                    dataHandles[i] = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                    dataPtrs[i] = dataHandles[i].AddrOfPinnedObject();
                }
            }
            catch (Exception e)
            {
                // The input texture is not readable.
                Debug.LogError(e);

                for (int i = 0; i < texture.mipmapCount; i++)
                {
                    if (dataHandles[i].IsAllocated)
                        dataHandles[i].Free();
                }

                if (compressed != null)
                    GameObject.Destroy(compressed);

                compressedTexture = null;
                isDone = true;

                return;
            }

#if UNITY_3_5
			IntPtr textureId = new IntPtr(compressed.GetNativeTextureID());
#else
            IntPtr textureId = compressed.GetNativeTexturePtr();
#endif
            IntPtr compressor = GenTextureCompressor(textureId, texture.width, texture.height,
                texture.mipmapCount, dataPtrs);

            Thread thread = new Thread(() => Compress(compressed, compressor, dataHandles));
            thread.Start();
        });

        if (removeReadableMemory)
        {
            routines.Enqueue(() =>
            {
                // Remove rgba32 data from main memory.
                // It may occur a small lag.
                compressed.Apply(false, true);
            });
        }
    }

	/// <summary>
	/// Compression thread.
	/// </summary>
	void Compress(Texture2D compressed, IntPtr compressor, GCHandle[] dataHandles)
	{
		CompressTextureAsync(compressor);

		for (int j = dataHandles.Length - 1; j >= 0; j--)
		{
			dataHandles[j].Free();
		}

		routines.Enqueue(() =>
		{
			UploadCompressedTexture(compressor);

			compressedTexture = compressed;
			isDone = true;
		});
	}

	[DllImport("ETCPlugin")]
	static extern IntPtr GenTextureCompressor(IntPtr texturePtr, int width, int height,
		int mipmapCount, IntPtr[] data);

	[DllImport("ETCPlugin", EntryPoint = "CompressTexture")]
	static extern void CompressTextureAsync(IntPtr compressor);

	[DllImport("ETCPlugin")]
	static extern void UploadCompressedTexture(IntPtr compressor);

}
