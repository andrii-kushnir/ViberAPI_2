using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ViberAPI.Models
{
    public class ViberImputMessage
    {
        public string @event { get; set; }
        public long timestamp { get; set; }
        public string chat_hostname { get; set; }
        public long message_token { get; set; }
        public string context { get; set; }
        public ViberClient sender { get; set; }
        public ViberClient user { get; set; }
        public MessageViber message { get; set; }
        public bool silent { get; set; }
        public string user_id { get; set; }
    }

    public class ViberClient
    {
        public string id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public string language { get; set; }
        public string country { get; set; }
        public string primary_device_os { get; set; }
        public int api_version { get; set; }
        public string viber_version { get; set; }
        public int mcc { get; set; }
        public int mnc { get; set; }
        public string device_type { get; set; }
    }

    public class MessageViber
    {
        public string text { get; set; }
        public string type { get; set; }
        public PhoneNumber contact { get; set; }
        public string media { get; set; }
        public string thumbnail { get; set; }
        public int size { get; set; }
        public int duration { get; set; }
        public string file_name { get; set; }
    }

    public class PhoneNumber
    {
        public string phone_number { get; set; }
    }
}
