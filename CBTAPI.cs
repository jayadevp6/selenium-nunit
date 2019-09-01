using System;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using OpenQA.Selenium;

namespace CBT_NUnit

{
    [SetUpFixture]
    public class CBTAPI
    {
        protected RemoteWebDriver driver;
        protected string browser;
        protected string session_id;
        public string BaseURL = "https://crossbrowsertesting.com/api/v3/selenium";
        public string username = "YOUR_USERNAME";
        public string authkey = "YOUR_AUTHKEY";

        public CBTAPI()
        {
        
        }
        public CBTAPI(string browser)
        {
            this.browser = browser;
        }

        [OneTimeSetUp]
        public void Initialize()
        {
            var caps = new RemoteSessionSettings();

            caps.AddMetadataSetting("name", "NUnit Test");
            caps.AddMetadataSetting("username", username);
            caps.AddMetadataSetting("password", authkey);
            caps.AddMetadataSetting("platform", "Windows 10");

            switch (browser)
            {
                // These all pull the latest version by default
                // To specify version add SetCapability("version", "desired version")
                case "chrome":
                    caps.AddMetadataSetting("browserName", "Chrome");
                    break;
                case "ie":
                    caps.AddMetadataSetting("browserName", "Internet Explorer");
                    break;
                case "edge":
                    caps.AddMetadataSetting("browserName", "MicrosoftEdge");
                    break;
                case "firefox":
                    caps.AddMetadataSetting("browserName", "Firefox");
                    break;
                default:
                    caps.AddMetadataSetting("browserName", "Chrome");
                    break;
            }


            driver = new RemoteWebDriver(new Uri("http://hub.crossbrowsertesting.com:80/wd/hub/"), caps);
        }

        [OneTimeTearDown]
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
