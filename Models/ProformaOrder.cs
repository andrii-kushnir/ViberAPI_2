using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViberAPI.Models
{
    public class ProformaOrder
    {
        public ProformaOrder()
        {
            Proforma = 0;
            Date = DateTime.MinValue;
            NameTv = "";
            Count = 0;
            Cena = 0;
            Suma = 0;
            Status = "невизначений";
        }

        public int Proforma { get; set; }
        public DateTime Date { get; set; }
        public string NameTv { get; set; }
        public double Count { get; set; }
        public double Cena { get; set; }
        public double Suma { get; set; }
        public string Status { get; set; }
    }
}
