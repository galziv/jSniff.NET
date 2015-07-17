using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace jSniff.Example
{
    [TestClass]
    public class UnitTest1
    {
        private ChromeDriver driver = new ChromeDriver();
        private readonly string exmampleUrl = "file:///" + Environment.CurrentDirectory.Replace('\\', '/') + "/../../example.html";

        [TestMethod]
        public void TestMethod1()
        {
            driver.Navigate().GoToUrl(exmampleUrl);

            Manager manager = new Manager(driver);

            manager.Sniffify("window", "multiply", "window_multiply", "function() { console.log('log from custom function');}");
            driver.FindElement(By.Id("execute")).Click();
            driver.FindElement(By.Id("a")).SendKeys("1");
            driver.FindElement(By.Id("b")).SendKeys("2");
            driver.FindElement(By.Id("execute")).Click();

            var invocations = manager.GetInvocations("window_multiply");
            var last = manager.GetLastInvocation("window_multiply");
            var parameters = manager.GetLastInvocationParams("window_multiply");
        }

        // This closes the driver down after the test has finished.
        [TestCleanup]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
