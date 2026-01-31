using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Aqlstore
{
    public class PersonEntry
    {
        public int Id { get; set; }          // ✅ int ONLY
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string City { get; set; } = "";
    }
}
