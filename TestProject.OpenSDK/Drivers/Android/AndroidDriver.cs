﻿// <copyright file="AndroidDriver.cs" company="TestProject">
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

namespace TestProject.OpenSDK.Drivers.Android
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog;
    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Remote;
    using TestProject.OpenSDK.Enums;
    using TestProject.OpenSDK.Exceptions;
    using TestProject.OpenSDK.Internal.Addons;
    using TestProject.OpenSDK.Internal.CallStackAnalysis;
    using TestProject.OpenSDK.Internal.Helpers;
    using TestProject.OpenSDK.Internal.Helpers.CommandExecutors;
    using TestProject.OpenSDK.Internal.Helpers.Threading;
    using TestProject.OpenSDK.Internal.Reporting;
    using TestProject.OpenSDK.Internal.Rest;

    /// <summary>
    /// Driver.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public class AndroidDriver<T> : OpenQA.Selenium.Appium.Android.AndroidDriver<T>, IWebDriver, OpenQA.Selenium.IHasTouchScreen
        where T : OpenQA.Selenium.IWebElement
    {
        /// <summary>
        /// Flag that indicates whether or not the driver instance is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Allows execution of basic touch screen operations.
        /// </summary>
        public OpenQA.Selenium.ITouchScreen TouchScreen { get; }

        private readonly DriverShutdownThread driverShutdownThread;

        private readonly string sessionId;

        private readonly AppiumCustomHttpCommandExecutor commandExecutor;

        /// <summary>
        /// Logger instance for this class.
        /// </summary>
        private static Logger Logger { get; set; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidDriver{T}"/> class.
        /// </summary>
        /// <param name="remoteAddress">The base address for the Agent API (e.g. http://localhost:8585).</param>
        /// <param name="token">The development token used to communicate with the Agent, see <a href="https://app.testproject.io/#/integrations/sdk">here</a> for more info.</param>
        /// <param name="appiumOptions">See <see cref="AppiumOptions"/> for more details.</param>
        /// <param name="projectName">The project name to report.</param>
        /// <param name="jobName">The job name to report.</param>
        /// <param name="disableReports">Set to true to disable all reporting (no report will be created on TestProject).</param>
        /// <param name="reportType">The report type of the execution, can be local, cloud or both.</param>
        /// <param name="reportName">The name of the local generated report.</param>
        /// <param name="reportPath">The path of the local generated report.</param>
        /// <param name="remoteConnectionTimeout">Timeout for the remote connection to the WebDriver server executing the commands.</param>
        /// <param name="restClientTimeout"> The connection timeout to the agent in milliseconds. Default is 120 seconds.</param>
        public AndroidDriver(
            Uri remoteAddress = null,
            string token = null,
            AppiumOptions appiumOptions = null,
            string projectName = null,
            string jobName = null,
            bool disableReports = false,
            ReportType reportType = ReportType.CLOUD_AND_LOCAL,
            string reportName = null,
            string reportPath = null,
            TimeSpan? remoteConnectionTimeout = null,
            int restClientTimeout = AgentClient.DefaultRestClientTimeoutInMilliseconds)
            : base(
                  AgentClient.GetInstance(remoteAddress, token, appiumOptions, new ReportSettings(projectName, jobName, reportType, reportName, reportPath), disableReports, restClientTimeout).AgentSession.RemoteAddress,
                  AgentClient.GetInstance().AgentSession.Capabilities,
                  remoteConnectionTimeout ?? DefaultCommandTimeout)
        {
            this.sessionId = AgentClient.GetInstance().AgentSession.SessionId;

            // Set the session ID for the base driver object to the session ID returned by the Agent.
            FieldInfo sessionIdField = typeof(RemoteWebDriver).GetField("sessionId", BindingFlags.Instance | BindingFlags.NonPublic);
            sessionIdField.SetValue(this, new SessionId(this.sessionId));

            // Create a new command executor for this driver session and set disable reporting flag
            this.commandExecutor = new AppiumCustomHttpCommandExecutor(AgentClient.GetInstance().AgentSession.RemoteAddress, disableReports, remoteConnectionTimeout ?? AgentClient.DefaultConnectionTimeout);

            // If the driver returned by the Agent is in W3C mode, we need to update the command info repository
            // associated with the base RemoteWebDriver to the W3C command info repository (default is OSS).
            if (AgentClient.GetInstance().IsInW3CMode())
            {
                var executorField = this.GetType().GetInheritedField("executor", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                var executor = (ICommandExecutor)executorField.GetValue(this);
                var realExecutorField = executor.GetType().GetInheritedField("RealExecutor", BindingFlags.Instance | BindingFlags.NonPublic);
                executor = (ICommandExecutor)realExecutorField.GetValue(executor);
                var commandInfoRepository = executor.GetType().GetField("commandInfoRepository", BindingFlags.Instance | BindingFlags.NonPublic);
                commandInfoRepository.SetValue(executor, typeof(AppiumCommand).CallPrivateStaticMethod("Merge", new object[] { new W3CWireProtocolCommandInfoRepository() }));
            }

            this.IsRunning = true;

            // Add shutdown hook for gracefully shutting down the driver
            this.driverShutdownThread = new DriverShutdownThread(this);

            if (StackTraceHelper.Instance.TryDetectSpecFlow())
            {
                var report = this.Report();

                if (!StackTraceHelper.Instance.IsSpecFlowPluginInstalled())
                {
                    string message = "TestProject Plugin for SpecFlow is not installed, please install the plugin and run the Test again.";
                    report.Step(description: message, passed: false);
                    Logger.Error(message);
                    this.Stop();
                    throw new SdkException(message);
                }

                report.DisableCommandReports(DriverCommandsFilter.All);
                report.DisableAutoTestReports(true);
                Logger.Info("SpecFlow detected, applying SpecFlow-specific reporting settings...");
            }
        }

        /// <summary>
        /// Enables access to the TestProject reporting actions from the driver object.
        /// </summary>
        /// <returns><see cref="Reporter"/> object exposing TestProject reporting methods.</returns>
        public Reporter Report()
        {
            return new Reporter(this.commandExecutor.ReportingCommandExecutor);
        }

        /// <summary>
        /// Enables access to the TestProject addon execution actions from the driver object.
        /// </summary>
        /// <returns><see cref="AddonHelper"/> object exposing TestProject action execution methods.</returns>
        public AddonHelper Addons()
        {
            return new AddonHelper();
        }

        /// <summary>
        /// Quits the driver and stops the session with the Agent, cleaning up after itself.
        /// </summary>
        public new void Quit()
        {
            if (this.IsRunning)
            {
                // Avoid performing the graceful shutdown more than once
                this.driverShutdownThread.Dispose();

                this.Stop();
            }
            else
            {
                Logger.Info("Driver is not running, skipping shutdown sequence");
            }
        }

        /// <summary>
        /// Sends any pending reports and closes the browser session.
        /// </summary>
        public void Stop()
        {
            // Report any stashed commands
            this.commandExecutor.ReportingCommandExecutor.ClearStash();

            this.IsRunning = false;

            base.Quit();
        }

        /// <summary>
        /// Overrides the base Execute() method by redirecting the WebDriver command to our own command executor.
        /// </summary>
        /// <param name="driverCommandToExecute">The WebDriver command to execute.</param>
        /// <param name="parameters">Contains the parameters associated with this command.</param>
        /// <returns>The response returned by the Agent upon requesting to execute this command.</returns>
        protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
        {
            if (driverCommandToExecute.Equals(DriverCommand.NewSession))
            {
                var resp = new Response();
                resp.Status = OpenQA.Selenium.WebDriverResult.Success;
                resp.SessionId = this.sessionId;
                resp.Value = new Dictionary<string, object>();

                return resp;
            }

            // The Agent does not understand the default way Selenium sends the driver command parameters for SendKeys
            // This means we'll need to patch them so these commands can be executed.
            if (!AgentClient.GetInstance().IsInW3CMode() && driverCommandToExecute.ShouldBePatched())
            {
                parameters = CommandHelper.UpdateSendKeysParameters(parameters);
            }

            Command command = new Command(new SessionId(this.sessionId), driverCommandToExecute, parameters);

            return this.commandExecutor.Execute(command);
        }
    }
}
