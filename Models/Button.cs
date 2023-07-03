using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class Button
    {
        public int Columns { get; set; }
        public bool ShouldSerializeColumns() { return Columns != 0; }
        public int Rows { get; set; }
        public bool ShouldSerializeRows() { return Rows != 0; }
        public string BgColor { get; set; }
        public string BgMediaType { get; set; }
        public string BgMedia { get; set; }
        public bool BgLoop { get; set; }
        public bool ShouldSerializeBgLoop() { return BgLoop; }
        public string ActionType { get; set; }
        public string ActionBody { get; set; }
        public string Image { get; set; }
        public string Text { get; set; }
        public string TextVAlign { get; set; }
        public string TextHAlign { get; set; }
        public int TextOpacity { get; set; }
        public string TextSize { get; set; }
        public bool Silent { get; set; }
        public bool ShouldSerializeSilent() { return Silent; }
        public Frame Frame { get; set; } = new Frame()
        {
            BorderWidth = "2",
            BorderColor = "#74BC1F",
            CornerRadius = "6"
        };
    }

    public class Frame
    {
        public string BorderWidth { get; set; } // 0..10
        public string BorderColor { get; set; }
        public string CornerRadius { get; set; }  //0..10
        //public bool ShouldSerializeColumns() { return Columns != 0; }
    }
}
