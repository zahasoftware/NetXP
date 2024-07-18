using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    //How to deserialize the next string using clases of this files {"custom_models":[{"id":"e71a1c2f-4f80-4800-934f-2c68979d8cc8","name":"Leonardo Anime XL","description":"A new high-speed Anime-focused model that excels at a range of anime, illustrative, and CG styles.","nsfw":false,"featured":false,"generated_image":{"id":"4fc2c951-5a86-4fc1-9ff2-d72a2213bb14","url":"https://cdn.leonardo.ai/users/384ab5c8-55d8-47a1-be22-6a274913c324/generations/16cbffcc-8672-47d6-8738-d22167dcea3f/Default_A_lush_vibrant_anime_hero_figure_emerges_from_the_shad_0.jpg"}}]} 

    public class GeneratedImage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class CustomModel
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("generated_image")]
        public GeneratedImage GeneratedImage { get; set; }
    }

    public class CustomModelRoot
    {
        [JsonProperty("custom_models")]
        public List<CustomModel> CustomModels { get; set; }
    }

}
