using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class MessageResponse
    {
        public int status { get; set; }
        public string status_message { get; set; }
        public long message_token { get; set; }
        public string chat_hostname { get; set; }
        public int billing_status { get; set; }
    }
}
