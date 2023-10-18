using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NetXP.TTSs.OpenTTS
{

  
    public class OpenTtsModelResponse
    {
        public string Gender { get; set; }
        public string Id { get; set; }
        public string Language { get; set; }
        public string Locale { get; set; }
        public bool Multispeaker { get; set; }
        public string Name { get; set; }
        public object Speakers { get; set; }
        public object Tag { get; set; }
        public string tts_name { get; set; }
    }
}
