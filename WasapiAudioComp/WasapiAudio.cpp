/**
 * Copyright (c) 2013 Nokia Corporation. All rights reserved.
 *
 * Nokia, Nokia Connecting People, Nokia Developer, and HERE are trademarks
 * and/or registered trademarks of Nokia Corporation. Other product and company
 * names mentioned herein may be trademarks or trade names of their respective
 * owners.
 *
 * See the license text file delivered with this project for more information.
 */

#include "pch.h"
#include "WasapiAudio.h"

using namespace WasapiAudioComp;
using namespace Platform;
using namespace Windows::System::Threading;

#define REFTIMES_PER_SEC       10000000
#define REFTIMES_PER_MILLISEC  10000

#define MY_MAX_RAW_BUFFER_SIZE 1024*128

/**
 * Helper function fill WAVEFORMATEX struct.
 * @param format WAVEFORMATEX struct to fill,
 * @param channels The number of channels in pcm data.
 * @param sampleRate The sample rate in pcm data.
 * @param bits The bit rate in pcm data.
 */
void MyFillPcmFormat(WAVEFORMATEX& format, WORD channels, int sampleRate, WORD bits)
{
    format.wFormatTag        = WAVE_FORMAT_PCM;
    format.nChannels         = channels;
    format.nSamplesPerSec    = sampleRate;
    format.wBitsPerSample    = bits;
    format.nBlockAlign       = format.nChannels * (format.wBitsPerSample / 8);
    format.nAvgBytesPerSec   = format.nSamplesPerSec * format.nBlockAlign;
    format.cbSize            = 0;
}

/**
 * @class WasapiAudio
 * @brief Class to capture and render audio through WASAPI.
 */

/**
 * Constructor.
 */
WasapiAudio::WasapiAudio() :
    m_waveFormatEx(NULL),
    m_pDefaultCaptureDevice(NULL),
    m_pDefaultRenderDevice(NULL),
    m_pCaptureClient(NULL),
    m_pRenderClient(NULL),
    m_sourceFrameSizeInBytes(0),
    started(false),
    audioBytes(NULL)
{
}

/**
 * Destructor.
 */
WasapiAudio::~WasapiAudio()
{
}

/**
 * Start audio capturing using WASAPI.
 * @return The success of the operation.
 */
bool WasapiAudio::StartAudioCapture()
{
    bool ret = false;

    if (!started)
    {
        HRESULT hr = InitCapture();
        if (SUCCEEDED(hr))
        {
            ret = started = true;
        }
    }

    return ret;
}

/**
 * Stop audio capturing using WASAPI.
 * @return The success of the operation.
 */
bool WasapiAudio::StopAudioCapture()
{
    bool ret = false;

    if (started)
    {
        HRESULT hr = S_OK;

        if (m_pDefaultCaptureDevice)
        {
            hr = m_pDefaultCaptureDevice->Stop();
        }

        if (m_pCaptureClient)
        {
            m_pCaptureClient->Release();
            m_pCaptureClient = NULL;
        }

        if (m_pDefaultCaptureDevice)
        {
            m_pDefaultCaptureDevice->Release();
            m_pDefaultCaptureDevice = NULL;
        }

        if (m_waveFormatEx)
        {
            CoTaskMemFree((LPVOID)m_waveFormatEx);
            m_waveFormatEx = NULL;
        }

        if (SUCCEEDED(hr))
        {
            started = false;
            ret = true;
        }
    }

    return ret;
}

/**
 * Read accumulated audio data.
 * @param byteArray The byte array to be filled with audio data.
 * @return The number of audio bytes returned.
 */
int WasapiAudio::ReadBytes(Platform::Array<byte>^* byteArray)
{
    int ret = 0;
    if (!started) return ret;
	
    BYTE *tempBuffer = new BYTE[MY_MAX_RAW_BUFFER_SIZE];
    UINT32 packetSize = 0;
    HRESULT hr = S_OK;
    long accumulatedBytes = 0;

    if (tempBuffer)
    {
        hr = m_pCaptureClient->GetNextPacketSize(&packetSize);

		while (SUCCEEDED(hr) && packetSize > 0 && (packetSize * m_sourceFrameSizeInBytes + accumulatedBytes < MY_MAX_RAW_BUFFER_SIZE))
		{
			BYTE* packetData = nullptr;
			UINT32 frameCount = 0;
			DWORD flags = 0;
			if (SUCCEEDED(hr))
			{
				hr = m_pCaptureClient->GetBuffer(&packetData, &frameCount, &flags, nullptr, nullptr);
				unsigned int incomingBufferSize = frameCount * m_sourceFrameSizeInBytes;

				memcpy(tempBuffer + accumulatedBytes, packetData, incomingBufferSize);
				accumulatedBytes += incomingBufferSize;
			}

            if (SUCCEEDED(hr))
            {
                hr = m_pCaptureClient->ReleaseBuffer(frameCount);
            }

            if (SUCCEEDED(hr))
            {
                hr = m_pCaptureClient->GetNextPacketSize(&packetSize);
            }
        }

        // Copy the available capture data to the array.
        auto temp = ref new Platform::Array<byte>(accumulatedBytes);
        for(long i = 0; i < accumulatedBytes; i++)
        {
                temp[i] = tempBuffer[i];
        }
        *byteArray = temp;
        ret = accumulatedBytes;

        // Reset byte counter
        accumulatedBytes = 0;
    }

    delete[] tempBuffer;

    return ret;
}

/**
 * Start audio rendering using WASAPI.
 * @return The success of the operation.
 */
bool WasapiAudio::StartAudioRender()
{
    bool ret = false;

    if (!started)
    {
        HRESULT hr = InitRender();
        if (SUCCEEDED(hr))
        {
            ret = started = true;
        }
    }

    return ret;
}

/**
 * Stop audio rendering using WASAPI.
 * @return The success of the operation.
 */
bool WasapiAudio::StopAudioRender()
{
    bool ret = false;

    if (started)
    {
        HRESULT hr = S_OK;

        if (m_pDefaultRenderDevice)
        {
            hr = m_pDefaultRenderDevice->Stop();
        }

        if (m_pRenderClient)
        {
            m_pRenderClient->Release();
            m_pRenderClient = NULL;
        }

        if (m_pDefaultRenderDevice)
        {
            m_pDefaultRenderDevice->Release();
            m_pDefaultRenderDevice = NULL;
        }

        if (m_waveFormatEx)
        {
            CoTaskMemFree((LPVOID)m_waveFormatEx);
            m_waveFormatEx = NULL;
        }

        if (SUCCEEDED(hr))
        {
            started = false;
            ret = true;
        }
    }

    return ret;
}

/**
 * Set audio data to be rendered.
 * @param byteArray The byte array to be rendered.
 */
void WasapiAudio::SetAudioBytes(const Platform::Array<byte>^ byteArray)
{
    delete audioBytes;

     // no need for the wav-header
    audioBytes = new BYTE[byteArray->Length-44];
    int availableBytes = byteArray->Length-44;

    for(long i = 0; i < availableBytes; i++)
    {
        audioBytes[i] = byteArray[i+44];
    }

    audioIndex = 0;
    audioByteCount = availableBytes;
}

/**
 * Feeds the render device with audio data.
 * @return True if there is audio data be rendered.
 */
bool WasapiAudio::Update()
{
    bool ret = false;
    if (!started) return ret;

    HRESULT hr = S_OK;
    UINT32 bufferFrameCount;
    UINT32 numFramesPadding;
    UINT32 numFramesAvailable;
    BYTE *pData = NULL;
    DWORD flags = 0;

    // Get the actual size of the allocated buffer.
    hr = m_pDefaultRenderDevice->GetBufferSize(&bufferFrameCount);

    // See how much buffer space is available.
    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultRenderDevice->GetCurrentPadding( &numFramesPadding);
        numFramesAvailable = bufferFrameCount - numFramesPadding;
    }

    if (SUCCEEDED(hr))
    {
        // Grab all the available space in the shared buffer.
        hr = m_pRenderClient->GetBuffer(numFramesAvailable, &pData);
    }

    if (SUCCEEDED(hr))
    {
        if (audioIndex + (long)numFramesAvailable * m_sourceFrameSizeInBytes < audioByteCount)
        {
            memcpy(pData, audioBytes+audioIndex, numFramesAvailable * m_sourceFrameSizeInBytes);
            audioIndex += numFramesAvailable * m_sourceFrameSizeInBytes;
            hr = m_pRenderClient->ReleaseBuffer(numFramesAvailable, 0);
        }
        else
        {
            hr = m_pRenderClient->ReleaseBuffer(0, AUDCLNT_BUFFERFLAGS_SILENT);
        }
    }
    
    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultRenderDevice->GetCurrentPadding(&numFramesPadding);
    }

    if (SUCCEEDED(hr) && numFramesPadding > 0)
    {
        ret = true;
    }

    return ret;
}

/**
 * Skips the next five seconds of audio.
 */
void WasapiAudio::SkipFiveSecs()
{
    int newAudioIndex = audioIndex + 2 * 2 * 44100 * 5;
    if (newAudioIndex < audioByteCount)
    {
        audioIndex = newAudioIndex;
    }
}

/**
 * Initialize WASAPI audio capture device.
 * @return The success of the operation.
 */
HRESULT WasapiAudio::InitCapture()
{
    HRESULT hr = E_FAIL;

    LPCWSTR captureId = GetDefaultAudioCaptureId(AudioDeviceRole::Default);

    if (NULL == captureId)
    {
        hr = E_FAIL;
    }
    else
    {
        hr = ActivateAudioInterface(captureId, __uuidof(IAudioClient2), (void**)&m_pDefaultCaptureDevice);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultCaptureDevice->GetMixFormat(&m_waveFormatEx);
    }

    // Set the category through SetClientProperties
    AudioClientProperties properties = {};
    if (SUCCEEDED(hr))
    {
        properties.cbSize = sizeof AudioClientProperties;
        properties.eCategory = AudioCategory_Other;
        // Note that AudioCategory_Other is the only valid category for capture and loopback streams.
        // From: http://msdn.microsoft.com/en-us/library/windows/desktop/hh404178(v=vs.85).aspx
        hr = m_pDefaultCaptureDevice->SetClientProperties(&properties);
    }

    if (SUCCEEDED(hr))
    {
        WAVEFORMATEX temp;
        MyFillPcmFormat(temp, 2, 44100, 16); // stereo, 44100 Hz, 16 bit

        *m_waveFormatEx = temp;
        m_sourceFrameSizeInBytes = (m_waveFormatEx->wBitsPerSample / 8) * m_waveFormatEx->nChannels;

        // using device to capture stereo requires the flag 0x8800000, or at least some part of it
        hr = m_pDefaultCaptureDevice->Initialize(AUDCLNT_SHAREMODE_SHARED, 0x88000000, 1000 * 10000, 0, m_waveFormatEx, NULL);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultCaptureDevice->GetService(__uuidof(IAudioCaptureClient), (void**)&m_pCaptureClient);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultCaptureDevice->Start();
    }

    if (captureId)
    {
        CoTaskMemFree((LPVOID)captureId);
    }

    return hr;
}

/**
 * Initialize WASAPI audio render device.
 * @return The success of the operation.
 */
HRESULT WasapiAudio::InitRender()
{
    HRESULT hr = E_FAIL;

    LPCWSTR renderId = GetDefaultAudioRenderId(AudioDeviceRole::Default);

    if (NULL == renderId)
    {
        hr = E_FAIL;
    }
    else
    {
        hr = ActivateAudioInterface(renderId, __uuidof(IAudioClient2), (void**)&m_pDefaultRenderDevice);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultRenderDevice->GetMixFormat(&m_waveFormatEx);
    }

    // Set the category through SetClientProperties
    AudioClientProperties properties = {};
    if (SUCCEEDED(hr))
    {
        properties.cbSize = sizeof AudioClientProperties;
        properties.eCategory = AudioCategory_Other;
        hr = m_pDefaultRenderDevice->SetClientProperties(&properties);
    }

    if (SUCCEEDED(hr))
    {
        WAVEFORMATEX temp;
        MyFillPcmFormat(temp, 2, 44100, 16); // stereo, 44100 Hz, 16 bit
        
        *m_waveFormatEx = temp;
        m_sourceFrameSizeInBytes = (m_waveFormatEx->wBitsPerSample / 8) * m_waveFormatEx->nChannels;

        hr = m_pDefaultRenderDevice->Initialize(AUDCLNT_SHAREMODE_SHARED, 0x88000000, 1000 * 10000, 0, m_waveFormatEx, NULL);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultRenderDevice->GetService(__uuidof(IAudioRenderClient), (void**)&m_pRenderClient);
    }

    if (SUCCEEDED(hr))
    {
        hr = m_pDefaultRenderDevice->Start();
    }

    if (renderId)
    {
        CoTaskMemFree((LPVOID)renderId);
    }

    return hr;
}