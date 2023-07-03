using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class FileSend
    {
        public string receiver { get; set; }
        public int min_api_version { get; set; }
        public bool ShouldSerializemin_api_version() { return min_api_version != 0; }
        public Sender sender { get; set; }
        public string tracking_data { get; set; }
        public string type { get; set; }
        public string text { get; set; }
        public bool ShouldSerializetext() { return !String.IsNullOrWhiteSpace(text); }
        public string media { get; set; }
        public string thumbnail { get; set; }
        public bool ShouldSerializethumbnail() { return !String.IsNullOrWhiteSpace(thumbnail); }
        public long size { get; set; }
        public bool ShouldSerializesize() { return size != 0; }
        public string file_name { get; set; }
        public bool ShouldSerializefile_name() { return !String.IsNullOrWhiteSpace(file_name); }
    }
}