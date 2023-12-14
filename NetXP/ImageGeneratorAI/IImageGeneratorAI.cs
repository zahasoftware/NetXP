﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI
{   //Interface to make pictures with AI
    public interface IImageGeneratorAI
    {
        /// <summary>
        /// This method generate images, but then you need to check if finished with the identifier
        /// </summary>
        /// <param name="options">Options param to specify defails of picture to generate like prompts and dimensions</param>
        /// <returns>Return identifier then you need to call GetImages with the returned Id</returns>
        Task<ResultGenerate> Generate(OptionsImageGenerator options);

        /// <summary>
        /// Get images (You need to call generate before to get ResultGenerate)
        /// </summary>
        /// <param name="resultGenerate">Result generate is obtained from Generate method</param>
        /// <returns>List of images</returns>
        Task<ResultImagesGenerated> GetImages(ResultGenerate resultGenerate);

        /// <summary>
        /// Remove images of result genenerated
        /// </summary>
        /// <param name="resultGenerate">Result generate is obtained from Generate method</param>
        /// <returns></returns>
        Task Remove(ResultGenerate resultGenerate);
    }
}