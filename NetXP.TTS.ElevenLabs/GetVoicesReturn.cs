using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.TTSs.ElevenLabs
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class FineTuning
    {
        public object language { get; set; }
        public bool is_allowed_to_fine_tune { get; set; }
        public bool fine_tuning_requested { get; set; }
        public string finetuning_state { get; set; }
        public object verification_attempts { get; set; }
        public List<object> verification_failures { get; set; }
        public int verification_attempts_count { get; set; }
        public object slice_ids { get; set; }
        public object manual_verification { get; set; }
        public bool manual_verification_requested { get; set; }
    }

    public class Labels
    {
        public string accent { get; set; }
        public string description { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        [JsonPropertyName("use case")]
        public string use_case { get; set; }
        public string description_ { get; set; }
    }

    public class Voice
    {
        public string voice_id { get; set; }
        public string name { get; set; }
        public object samples { get; set; }
        public string category { get; set; }
        public FineTuning fine_tuning { get; set; }
        public Labels labels { get; set; }
        public object description { get; set; }
        public string preview_url { get; set; }
        public List<object> available_for_tiers { get; set; }
        public object settings { get; set; }
        public object sharing { get; set; }
        public List<string> high_quality_base_model_ids { get; set; }
    }

    public class Root
    {
        public List<Voice> voices { get; set; }
    }
}
