﻿// <copyright file="IWebDriver.cs" company="TestProject">
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

namespace TestProject.OpenSDK.Drivers
{
    using TestProject.OpenSDK.Internal.Addons;
    using TestProject.OpenSDK.Internal.Reporting;

    /// <summary>
    /// Interface defining methods that all TestProject driver classes should implement.
    /// </summary>
    public interface IWebDriver : OpenQA.Selenium.IWebDriver
    {
        /// <summary>
        /// Flag that indicates whether or not the driver instance is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Enables access to the TestProject reporting actions from the driver object.
        /// </summary>
        /// <returns><see cref="Reporter"/> object exposing TestProject reporting methods.</returns>
        Reporter Report();

        /// <summary>
        /// Enables access to the TestProject addon execution actions from the driver object.
        /// </summary>
        /// <returns><see cref="AddonHelper"/> object exposing TestProject action execution methods.</returns>
        AddonHelper Addons();

        /// <summary>
        /// Quits the driver and stops the session with the Agent, cleaning up after itself.
        /// </summary>
        new void Quit();

        /// <summary>
        /// Sends any pending reports and closes the browser session.
        /// </summary>
        void Stop();
    }
}
