using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class MyStat1
    {
        public MyStat1()
        {
            Name = "";
            Count = 0;
        }
        public string Namesk { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int Rate { get; set; }
    }

    public class MyStat2
    {
        public MyStat2()
        {
            Nrating = 0;
            Count = 0;
        }
        public int Nrating { get; set; }
        public int Count { get; set; }
    }
}
