using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jSniff
{
    public class Invocation
    {
        public DateTime ExecutionDate { get; set; }

        public Dictionary<string, object> ExecutionParameters { get; set; }
    }
}
