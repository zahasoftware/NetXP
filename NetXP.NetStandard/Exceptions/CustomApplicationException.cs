using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class CustomApplicationException : ApplicationException
    {
        public string Label { get; set; } = string.Empty;

        public CustomApplicationException(string msg) : base(msg)
        {

        }
        public CustomApplicationException(string msg, Exception innerException) : base(msg, innerException)
        {
        }

        public CustomApplicationException(string label, string msg, Exception innerException) : base(msg, innerException)
        {
            this.Label = label;
        }


        public CustomApplicationException(string label, string msg) : base(msg)
        {
            this.Label = label;
        }
    }
}
