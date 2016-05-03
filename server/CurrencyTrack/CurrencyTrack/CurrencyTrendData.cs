using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace CurrencyTrack
{
    public class CurrencyTrendData
    {
        public int responseType = 1;
        public string name = "";
        public List<decimal> value;// = "";
        public int time;
    }

}