using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class Keyboard
    {
        public string Type { get; set; }
        public bool DefaultHeight { get; set; }
        public bool ShouldSerializeDefaultHeight() { return DefaultHeight; }
        public string BgColor { get; set; }
        public string InputFieldState { get; set; }
        public List<Button> Buttons { get; set; }
    }
}
