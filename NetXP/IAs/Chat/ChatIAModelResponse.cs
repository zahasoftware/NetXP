using System;
using System.Runtime.InteropServices;

namespace NetXP.IAs.Chat
{
    public class ChatIAModelResponse
    {
        //Properties Name, Description, Id, Size, ModifiedAt
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public long Size { get; set; }
        public DateTime ModifiedAt { get; set; }

    }
}