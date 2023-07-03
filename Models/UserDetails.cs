using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace ViberAPI.Models
{
    public class UserDetails
    {
        public int status { get; set; }
        public string status_message { get; set; }
        public string chat_hostname { get; set; }
        public ViberClient user { get; set; }
    }
}
