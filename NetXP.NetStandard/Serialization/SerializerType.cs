using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Serialization
{
    public enum SerializerType
    {
        Xml, //DataContractSerializer
        Json,//DataContractJsonSerializer
        XmlSerializer,//XmlSerializer
        JsonConvert,//JsonConverter
    }
}
