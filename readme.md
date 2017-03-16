# Getting Started with NUnit and CrossBrowserTesting

Originally ported from Java's JUnit, [NUnit](https://www.nunit.org/) provides a powerful platform for performing unit tests. Combined with the capabilities of [Selenium](http://www.seleniumhq.org/), you can quickly start testing your web application. If you bring our platform into the mix, you now have hundereds of browsers at your disposal. Here, we'll get you started with a single NUnit test to a simple Angular ToDo application. We'll then move up to the point of running 2 tests in parallel for faster execution. 

For this example, I'm using Visual Studio 2015. Let's get started by installing some necessary dependencies. From NuGet, we'll install [NUnit](https://www.nuget.org/packages/NUnit/) and [Selenium-WebDriver](https://www.nuget.org/packages/Selenium.WebDriver/). From there, we can start putting our tests together. There are a couple components we'll need. We'll separate starting/closing our WebDriver and running our tests. For starting up, we can use this code to generate our WebDriver:

```C#

using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CBT_NUnit
{
    [TestFixture]
    public class CBTAPI
    {
        protected RemoteWebDriver driver;
        protected string browser;
        protected string session_id;
        public string BaseURL = "https://crossbrowsertesting.com/api/v3/selenium";
        public string username = "chase@crossbrowsertesting.com";
        public string authkey = "12345";
        
        public CBTAPI(string browser)
        {
            this.browser = browser;
        }

        [SetUp]
        public void Initialize()
        {
            DesiredCapabilities capability = new DesiredCapabilities();

            capability.SetCapability("name", "NUnit-CBT");
            capability.SetCapability("record_video", "true");
            capability.SetCapability("build", "1.0");
            capability.SetCapability("os_api_name", "Win10");

            switch (browser)
            {
                case "chrome": 
                    capability.SetCapability("browser_api_name", "Chrome56x64");
                    break;
                case "ie":
                    capability.SetCapability("browser_api_name", "FF46x64");
                    break;
                case "edge":
                    capability.SetCapability("browser_api_name", "Edge20");
                    break;
                default:
                    capability.SetCapability("browser_api_name", "IE11");
                    break;
            }

            capability.SetCapability("username", username);
            capability.SetCapability("password", authkey);

            driver = new RemoteWebDriver(new Uri("http://hub.crossbrowsertesting.com:80/wd/hub/"), capability);
        }

        [TearDown]
        public void Cleanup()
        {
            var session_id = driver.SessionId.ToString();
            driver.Quit();
            setScore(session_id, "pass");
        }

        public void setScore(string sessionId, string score)
        {
            string url = BaseURL + "/" + sessionId;
            // encode the data to be written
            ASCIIEncoding encoding = new ASCIIEncoding();
            string data = "action=set_score&score=" + score;
            byte[] putdata = encoding.GetBytes(data);
            // Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentLength = putdata.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // Write data to stream
            Stream newStream = request.GetRequestStream();
            newStream.Write(putdata, 0, putdata.Length);
            WebResponse response = request.GetResponse();
            newStream.Close();
        }
    }
}
```

 Notice the use of the decorators used by NUnit, [SetUp] and [TearDown]. Setup is used before each test unit is performed, and in this case we are instantiating the WebDriver object based of a browser parameter. After it's pointed to our hub with our os/browser api names, you should see the changes reflected in the app. The TearDown decorator is code that is run after each test is performed. In this case, we're making the call to driver.Quit() which ends the test session in our app. Additionally, we are using the API here to set the score to pass if we made it through our tests successfully. This is great for quickly seeing the results of your tests from our app rather than just from VS.

 At this point, we've only created the driver object. Now let's create the test to be performed:

```C#
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
    public class BasicTest : CBTAPI
    {
        public BasicTest(string browser) : base(browser) { }

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
```

Here, we're creating a test that clicks a few checkboxes, creates a new ToDo, and archives the ones we checked. Additionally, its asserting throughout the test to ensure that the changes we made worked correctly. Notice also that our test extends our CBTAPI class so that the driver instantiation and tear down is handled outside of our class. To run it, simply right click within the method and click "Run Test". Switch over to the app, and you can see it running. 


## Parallel Testing

Want to get the same job done in half of the time? That's where parallel testing comes into play, and we're all for parallel testing at CBT. NUnit makes it simple by providing a single additional decorator, [Parallelizable(ParallelScope.Fixtures)]. Additionally, we'll give our test a few more browser parameters. Check out the below code:

```C#
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
```

Running this should start a test to two different browsers at once. This halves the execution time. Increasing your level of parallelization similarly cuts time and makes your job easier :) If you have any trouble getting setup, don't hesitate to [reach out to us](mailto:support@crossbrowsertesting.com). Happy Testing!