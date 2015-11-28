using System.Collections.Generic;
using OpenQA.Selenium;

namespace jSniff
{
    public class Sniff
    {
        private readonly IJavaScriptExecutor driver;
        private readonly string name;

        public Sniff(IJavaScriptExecutor driver, string name)
        {
            this.driver = driver;
            this.name = name;
        }

        public Invocation[] GetInvocations(string sniffName)
        {
            var result = driver.ExecuteScript(string.Format("return jSniff.getInvocations('{0}')", sniffName), null);
            return Converter.ConvertToInvocation(result);
        }

        public Invocation GetLastInvocation(string sniffName)
        {
            var invocations = GetInvocations(sniffName);
            return invocations[invocations.Length - 1];
        }

        public Dictionary<string, object> GetLastInvocationParams(string sniffName)
        {
            return GetLastInvocation(sniffName).ExecutionParameters;
        }

        public long GetLastInvocationDuration(string sniffName)
        {
            return GetLastInvocation(sniffName).Duration;
        }
    }
}
