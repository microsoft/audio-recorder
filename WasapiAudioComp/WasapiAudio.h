/**
 * Copyright © 2013 Nokia Corporation.
 */

#pragma once

#include <windows.h>

#include <synchapi.h>
#include <audioclient.h>
#include <phoneaudioclient.h>

namespace WasapiAudioComp
{
    public ref class WasapiAudio sealed
    {
    public:
        WasapiAudio();
        virtual ~WasapiAudio();
            
        bool StartAudioCapture();
        bool StopAudioCapture();
        int ReadBytes(Platform::Array<byte>^* a);

        bool StartAudioRender();
        bool StopAudioRender();
        void SetAudioBytes(const Platform::Array<byte>^ a);
        bool Update();
        void SkipFiveSecs();

    private:
        HRESULT InitCapture();
        HRESULT InitRender();

        bool started;
        int m_sourceFrameSizeInBytes;

        WAVEFORMATEX* m_waveFormatEx;

        // Device
        IAudioClient2* m_pDefaultCaptureDevice;
        IAudioClient2* m_pDefaultRenderDevice;

        // Actual capture object
        IAudioCaptureClient* m_pCaptureClient;
        IAudioRenderClient* m_pRenderClient;

        // TEST AUDIO
        BYTE* audioBytes;
        long audioIndex;
        long audioByteCount;
    };
}