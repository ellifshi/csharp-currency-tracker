using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurrencyTrack
{
    public class CurrencyRates
    {
        //public string objDictCurrencyName { get; set; }
        //public double objDictCurrencyValue { get; set; }
        public Dictionary<string, decimal> quotes { get; set; }
        public int TimeStamp { get; set; }
        public string success { get; set; }
        public Dictionary<string, string> error { get; set; }
        public string source { get; set; }
    }//End class CurrencyRates

}