using System;
using System.Collections.Generic;

namespace jSniff
{
    public class Invocation
    {
        public DateTime ExecutionDate { get; set; }

        public long Duration { get; set; }

        public Dictionary<string, object> ExecutionParameters { get; set; }
    }
}
