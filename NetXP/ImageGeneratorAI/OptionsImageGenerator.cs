namespace NetXP.ImageGeneratorAI
{
    public class OptionsImageGenerator
    {
        public string Prompt { get; set; }
        public string NegativePrompt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NumImages { get; set; }
        public string ModelId { get; set; }
        public dynamic ExtraOptions { get; set; }
    }
}