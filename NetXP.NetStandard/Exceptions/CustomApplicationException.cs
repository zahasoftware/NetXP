using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class CustomApplicationException : NetXPApplicationException
    {
        public string Label { get; set; } = string.Empty;

        public CustomApplicationException(string msg) : base(msg)
        {

        }

        public CustomApplicationException(string label, string msg) : base(msg)
        {
            this.Label = label;
        }
    }
}
