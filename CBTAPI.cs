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
