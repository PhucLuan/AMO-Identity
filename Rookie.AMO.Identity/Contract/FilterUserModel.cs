using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.Contract
{
    public class FilterUserModel
    {
        //string name, string type, int page, string propertyName
        //  , bool desc, string search, int limit = 3
        public string name { get; set; } = "";
        public string type { get; set; } = "";
        public int page { get; set; } = 1;
        public string propertyName { get; set; } = "";
        public bool desc { get; set; } = true;
        public string search { get; set; } = "";
        public int limit { get; set; } = 5;
    }

}
