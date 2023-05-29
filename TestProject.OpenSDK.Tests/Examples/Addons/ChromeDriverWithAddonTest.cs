﻿// <copyright file="ChromeDriverWithAddonTest.cs" company="TestProject">
// Copyright 2020 TestProject (https://testproject.io)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace TestProject.OpenSDK.Tests.Examples.Addons
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using TestProject.OpenSDK.Drivers.Web;
    using TestProject.OpenSDK.Tests.Examples.Addons.ExampleAddon;

    /// <summary>
    /// This class contains examples of using the TestProject C# SDK with Chrome.
    /// </summary>
    [TestClass]
    public class ChromeDriverWithAddonTest
    {
        /// <summary>
        /// The TestProject ChromeDriver instance to be used in this test class.
        /// </summary>
        private ChromeDriver driver;

        /// <summary>
        /// Starts a Chrome browser with default ChromeOptions before each test.
        /// </summary>
        [TestInitialize]
        public void StartBrowser()
        {
            this.driver = new ChromeDriver();
        }

        /// <summary>
        /// An example test logging in to the TestProject demo application with Chrome.
        /// </summary>
        [TestMethod]
        public void ExampleTestWithAddonsUsingChromeDriver()
        {
            By phoneField = By.CssSelector("#phone");

            this.driver.Navigate().GoToUrl("https://example.testproject.io");
            this.driver.FindElement(By.CssSelector("#name")).SendKeys("John Smith");
            this.driver.FindElement(By.CssSelector("#password")).SendKeys("12345");
            this.driver.FindElement(By.CssSelector("#login")).Click();

            new SelectElement(this.driver.FindElement(By.CssSelector("#country"))).SelectByText("Australia");
            this.driver.FindElement(By.CssSelector("#address")).SendKeys("Main Street 123");
            this.driver.FindElement(By.CssSelector("#email")).SendKeys("john@smith.org");

            // Generate a random phone number and type it into the phone field using an addon.
            this.driver.Addons().Execute(new TypeRandomPhoneAction("1", 10), phoneField);

            this.driver.FindElement(By.CssSelector("#save")).Click();

            Assert.IsTrue(this.driver.FindElement(By.CssSelector("#saved")).Displayed);
        }

        /// <summary>
        /// Closes the browser and ends the development session after each test.
        /// </summary>
        [TestCleanup]
        public void CloseBrowser()
        {
            this.driver.Quit();
        }
    }
}
