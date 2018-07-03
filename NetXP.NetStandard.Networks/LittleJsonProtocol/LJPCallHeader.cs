using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{

    /// <summary>
    /// Common class for class of sending and received call class.
    /// </summary>
    public class LJPCallHeader : LJPHeader
    {
        public bool NeedResponse { get; set; } = true;
        public bool KeepAlive { get; set; }

        /// <summary>
        /// For Javascript Request, To Follow And Validate a Call To Server
        /// </summary>
        public long? Id { get; set; }
        public string Version { get; set; }

        public LJPCredential Credential { get; set; }

        private List<object> _Parameters;
        public List<object> Parameters
        {
            get
            {
                if (_Parameters == null) { _Parameters = new List<object>(); }
                return _Parameters;
            }
            set { _Parameters = value; }
        }
    }
}
