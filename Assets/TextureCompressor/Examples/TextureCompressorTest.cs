using System;
using System.Collections;
using UnityEngine;

class TextureCompressorTest : MonoBehaviour
{
	public Texture2D texture;
	public GUIText log;

	void Start()
	{
		StartCoroutine(Go());
	}
	
	IEnumerator Go()
	{
		log.text = "";

		if (texture == null)
		{
			log.text = "Downloading image...\n";

			WWW www = new WWW("http://upload.wikimedia.org/wikipedia/en/2/24/Lenna.png");

			while (!www.isDone)
			{
				log.text = string.Format("Downloading image... {0:0.00}%\n", www.progress * 100.0f);

				yield return null;
			}

			log.text = "Downloading image...\n";

			texture = www.texture;
			texture.name = "Lenna";

			www.Dispose();
		}

		log.text += string.Format("{0}: {1}\n", texture, texture.format);

        int bytesBefore = texture.GetRawTextureData().Length;
        log.text += "Bytes before compression: " + bytesBefore.ToString() + "\n";

        log.text += "Compressing...\n";

		float startTime = Time.realtimeSinceStartup;

		// -- Compression start! --
		CompressTexture compressor = new CompressTexture(texture);
        
		yield return StartCoroutine(compressor);

		// -- or --
		//
		// StartCoroutine(compressor);
		//
		// while (!compressor.isDone)
		//    yield return null;

		if (compressor.compressedTexture != null)
		{
			texture = compressor.compressedTexture;
		}
		
		float finishTime = Time.realtimeSinceStartup;

		log.text += string.Format("Done! ({0:0.0000}s)\n", finishTime - startTime);

		log.text += string.Format("{0}: {1}\n", texture, texture.format);
        int bytesAfter = texture.GetRawTextureData().Length;
        log.text += "Bytes after compression: " + bytesAfter.ToString() + "\n";
        log.text += "Compression rate: " + ((float)bytesAfter / bytesBefore).ToString();
		yield return null;
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	void OnGUI()
	{
		if (texture != null)
		{
			int x = 0, y = 0;
			int w = texture.width, h = texture.height;

			for (int i = 0; i < texture.mipmapCount; i++)
			{
				GUI.DrawTexture(new Rect(x, y, w, h), texture);

				if (Screen.width <= Screen.height)
					y += h;
				else
					x += w;

				w /= 2;
				h /= 2;
			}
		}

		if (GUI.Button(new Rect(10, Screen.height - 60, 120, 50), "WWW Test"))
		{
			texture = null;
			StartCoroutine(Go());
		}
	}
}
