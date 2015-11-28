using System;
using System.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;

namespace jSniff.Example
{
    [TestClass]
    public class jSniffTest
    {
        private ChromeDriver driver = new ChromeDriver(@"C:\Github\jSniff.NET\jSniff.Example");
        private readonly string exmampleUrl = "file:///" + Environment.CurrentDirectory.Replace('\\', '/') + "/../../example.html";

        [TestMethod]
        public void TestMethod1()
        {
            // setup
            driver.Navigate().GoToUrl(exmampleUrl);
            Manager manager = new Manager(driver);
            string a = "3";
            string b = "7";

            // execute
            Sniff sniff =  manager.Sniffify("window", "multiply", "window_multiply", "function() { console.log('log from custom function');}");
            driver.FindElement(By.Id("a")).Clear();
            driver.FindElement(By.Id("a")).SendKeys(a);
            driver.FindElement(By.Id("b")).Clear();
            driver.FindElement(By.Id("b")).SendKeys(b);
            driver.FindElement(By.Id("execute")).Click();
            driver.FindElement(By.Id("execute")).Click();

            var invocations = sniff.GetInvocations("window_multiply");
            var lastInvocation = sniff.GetLastInvocation("window_multiply");
            var parameters = manager.GetLastInvocationParams("window_multiply");

            // expect
            Assert.AreEqual(invocations.Length, 2);
            Assert.AreEqual(lastInvocation.ExecutionParameters["a"], a, "last invocation a value is incorrect");
            Assert.AreEqual(lastInvocation.ExecutionParameters["b"], b, "last invocation b value is incorrect");
            Assert.AreEqual(parameters["a"], a,"parameters a value is incorrect");
            Assert.AreEqual(parameters["b"], b,"parameters b value is incorrect");
            Assert.IsTrue(lastInvocation.Duration > 0);
        }

        // This closes the driver down after the test has finished.
        [TestCleanup]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
