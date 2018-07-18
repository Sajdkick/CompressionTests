/*
 * Copyright 2015 The Etc2Comp Authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include "EtcConfig.h"
#include "Etc.h"
#include "EtcFilter.h"
#include "EtcFile.h"
#include "EtcFileHeader.h"

#include <string.h>
#include <iostream>

namespace Etc
{
	
	int test(){return 1337;}
	
	// ----------------------------------------------------------------------------------------------------
	// C-style inteface to the encoder
	//
	void Encode(float *a_pafSourceRGBA,
				unsigned int a_uiSourceWidth, 
				unsigned int a_uiSourceHeight,
				Image::Format a_format,
				ErrorMetric a_eErrMetric,
				float a_fEffort,
				unsigned int a_uiJobs,
				unsigned int a_uiMaxJobs,
				char **output,
				unsigned int *size,
				unsigned int *a_puiExtendedWidth,
				unsigned int *a_puiExtendedHeight, 
				int *a_piEncodingTime_ms, bool a_bVerboseOutput)
	{

		// Image image(a_pafSourceRGBA, a_uiSourceWidth,
					// a_uiSourceHeight,
					// a_eErrMetric);
		// image.m_bVerboseOutput = a_bVerboseOutput;
		// image.Encode(a_format, a_eErrMetric, a_fEffort, a_uiJobs, a_uiMaxJobs);

		// *a_puiExtendedWidth = image.GetExtendedWidth();
		// *a_puiExtendedHeight = image.GetExtendedHeight();
		// *a_piEncodingTime_ms = image.GetEncodingTimeMs();

        // Etc::File file("test.ktx", Etc::File::Format::INFER_FROM_FILE_EXTENSION, a_format, image.GetEncodingBits(), image.GetEncodingBitsBytes(),
            // image.GetSourceWidth(), image.GetSourceHeight(),
            // image.GetExtendedWidth(), image.GetExtendedHeight());

        //*size = image.GetEncodingBitsBytes();
        // char* fullFile = new char[*size];
        // char* start = fullFile;

        //Etc::FileHeader_Ktx header(&file);
        //char* step1 = reinterpret_cast<char*>(header.GetData());
        //for (int i = 0; i < sizeof(Etc::FileHeader_Ktx::Data); i++)
        //{
        //    *fullFile = step1[i];
        //    fullFile++;
        //}

        //char* step2 = reinterpret_cast<char*>(image.GetEncodingBitsBytes());
        //for (int i = 0; i < sizeof(image.GetEncodingBitsBytes()); i++)
        //{
        //    *fullFile = step2[i];
        //    fullFile++;
        //}

        // char* step3 = reinterpret_cast<char*>(image.GetEncodingBits());
        // for (int i = 0; i < image.GetEncodingBitsBytes(); i++)
        // {
            // *fullFile = step3[i];
            // fullFile++;
        // }

        //*output = reinterpret_cast<char*>(image.GetEncodingBits());

	}

	void EncodeMipmaps(float *a_pafSourceRGBA,
		unsigned int a_uiSourceWidth,
		unsigned int a_uiSourceHeight,
		Image::Format a_format,
		ErrorMetric a_eErrMetric,
		float a_fEffort,
		unsigned int a_uiJobs,
		unsigned int a_uiMaxJobs,
		unsigned int a_uiMaxMipmaps,
		unsigned int a_uiMipFilterFlags,
		RawImage* a_pMipmapImages,
		int *a_piEncodingTime_ms, 
		bool a_bVerboseOutput)
	{
		auto mipWidth = a_uiSourceWidth;
		auto mipHeight = a_uiSourceHeight;
		int totalEncodingTime = 0;
		for(unsigned int mip = 0; mip < a_uiMaxMipmaps && mipWidth >= 1 && mipHeight >= 1; mip++)
		{
			float* pImageData = nullptr;
			float* pMipImage = nullptr;

			if(mip == 0)
			{
				pImageData = a_pafSourceRGBA;
			}
			else
			{
				pMipImage = new float[mipWidth*mipHeight*4];
				if(FilterTwoPass(a_pafSourceRGBA, a_uiSourceWidth, a_uiSourceHeight, pMipImage, mipWidth, mipHeight, a_uiMipFilterFlags, Etc::FilterLanczos3) )
				{
					pImageData = pMipImage;
				}
			}

			if ( pImageData )
			{
			
				Image image(pImageData, mipWidth, mipHeight,	a_eErrMetric);

			image.m_bVerboseOutput = a_bVerboseOutput;
			image.Encode(a_format, a_eErrMetric, a_fEffort, a_uiJobs, a_uiMaxJobs);

			a_pMipmapImages[mip].paucEncodingBits = std::shared_ptr<unsigned char>(image.GetEncodingBits(), [](unsigned char *p) { delete[] p; });
			a_pMipmapImages[mip].uiEncodingBitsBytes = image.GetEncodingBitsBytes();
			a_pMipmapImages[mip].uiExtendedWidth = image.GetExtendedWidth();
			a_pMipmapImages[mip].uiExtendedHeight = image.GetExtendedHeight();

			totalEncodingTime += image.GetEncodingTimeMs();
			}

			if(pMipImage)
			{
				delete[] pMipImage;
			}

			if (!pImageData)
			{
				break;
			}

			mipWidth >>= 1;
			mipHeight >>= 1;
		}

		*a_piEncodingTime_ms = totalEncodingTime;
	}


	// ----------------------------------------------------------------------------------------------------
	//

}
