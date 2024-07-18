using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.Tts
{
    public interface ITts
    {
        /// <summary>
        /// Get the audio from text
        /// </summary>
        /// <param name="text">Text you want to convert</param>
        /// <returns>Return TTSAudio object with the audio and format</returns>
        Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption);
        Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption, CancellationToken token);

        /// <summary>
        /// Get available voices options
        /// </summary>
        /// <returns></returns>
        Task<List<TtsVoice>> GetTtsVoices(string? language = null);
        Task<List<TtsVoice>> GetTtsVoices(string? language, CancellationToken token);

    }
}
