﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.TTS
{
    public interface ITTS
    {
        /// <summary>
        /// Get the audio from text
        /// </summary>
        /// <param name="text">Text you want to convert</param>
        /// <returns>Return TTSAudio object with the audio and format</returns>
        Task<TTSAudio> Convert(TTSConvertOption ttsConvertOption);
        Task<TTSAudio> Convert(TTSConvertOption ttsConvertOption, CancellationToken token);

        /// <summary>
        /// Get availabe voices options
        /// </summary>
        /// <returns></returns>
        Task<List<TTSVoice>> GetTTSVoices(string language = null);
        Task<List<TTSVoice>> GetTTSVoices(string language, CancellationToken token);

    }
}
