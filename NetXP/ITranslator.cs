using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP
{
    public interface ITranslator
    {
        Task<string> TranslateTextAsync(string text, string toLanguage);
    }
}
