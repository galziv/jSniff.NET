using System;
using System.Collections.Generic;
using System.Linq;

namespace jSniff
{
    public static class Converter
    {
        private const string EXECUTION_DATE = "executionDate";
        private const string DURATION = "duration";
        private const string PARAMS = "params";

        public static Invocation[] ConvertToInvocation(object result)
        {
            return ((IReadOnlyCollection<object>)result).Select(x =>
            {
                var dictionary = (Dictionary<string, object>)x;
                var parameters = ((Dictionary<string, object>)dictionary[PARAMS]);
                var paramsDictionary = new Dictionary<string, object>(parameters.Count);

                foreach (string key in parameters.Keys)
                {
                    paramsDictionary.Add(key, parameters[key]);
                }

                return new Invocation
                {
                    ExecutionDate = ConvertJsDateToDateTime(dictionary[EXECUTION_DATE].ToString()),
                    Duration = Int64.Parse(dictionary[DURATION].ToString()),
                    ExecutionParameters = paramsDictionary
                };

            }).ToArray();
        }

        public static DateTime ConvertJsDateToDateTime(string date)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(date));
        }
    }
}
