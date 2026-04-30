namespace NetXP.Tts.ChatterboxApi
{
    public class TtsChatterboxApiOptions
    {
        public string URL { get; set; } = "http://localhost:9000/";
        public string HealthUri { get; set; } = "health";
        public string VoicesUri { get; set; } = "voices";
        public string LanguagesUri { get; set; } = "languages";
        public string GenerateUri { get; set; } = "generate";
        public string DefaultModel { get; set; } = "turbo";
        public string DefaultDevice { get; set; } = "cuda";
        public string DefaultLanguage { get; set; } = "en";
        public double Temperature { get; set; } = 0.5;
        public double TopP { get; set; } = 0.8;
        public bool UseHealthCheck { get; set; } = true;
    }
}
