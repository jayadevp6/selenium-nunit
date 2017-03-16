using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace CBT_NUnit
{
    [TestFixture("chrome")]
    [TestFixture("firefox")]
    [TestFixture("ie")]
    [TestFixture("edge")]
    [Parallelizable(ParallelScope.Fixtures)]
    public class ParallelTest : CBTAPI
    {
        public ParallelTest(string browser) : base(browser) { }
        [Test]
        public void TestTodos()
        {
            driver.Navigate().GoToUrl("http://crossbrowsertesting.github.io/todo-app.html");
            // Check the title
            driver.FindElement(By.Name("todo-4")).Click();
            driver.FindElement(By.Name("todo-5")).Click();

            // If both clicks worked, then the following List should have length 2
            IList<IWebElement> elems = driver.FindElements(By.ClassName("done-true"));
            // so we'll assert that this is correct.
            Assert.AreEqual(2, elems.Count);

            driver.FindElement(By.Id("todotext")).SendKeys("run your first selenium test");
            driver.FindElement(By.Id("addbutton")).Click();

            // lets also assert that the new todo we added is in the list
            string spanText = driver.FindElement(By.XPath("/html/body/div/div/div/ul/li[6]/span")).Text;
            Assert.AreEqual("run your first selenium test", spanText);
            driver.FindElement(By.LinkText("archive")).Click();

            elems = driver.FindElements(By.ClassName("done-false"));
            Assert.AreEqual(4, elems.Count);
        }

    }
}
