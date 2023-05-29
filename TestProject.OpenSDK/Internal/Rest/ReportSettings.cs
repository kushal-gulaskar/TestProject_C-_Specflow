﻿// <copyright file="ReportSettings.cs" company="TestProject">
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

namespace TestProject.OpenSDK.Internal.Rest
{
    using TestProject.OpenSDK.Enums;

    /// <summary>
    /// Report settings model provided to the Agent upon session initialization.
    /// </summary>
    public class ReportSettings
    {
        /// <summary>
        /// The project name to report.
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// The job name to report.
        /// </summary>
        public string JobName { get; }

        /// <summary>
        /// The name of the local generated report.
        /// </summary>
        public string ReportName { get; }

        /// <summary>
        /// The path of the local generated report.
        /// </summary>
        public string ReportPath { get; }

        /// <summary>
        /// The report type of the execution.
        /// </summary>
        public ReportType ReportType { get;  }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSettings"/> class.
        /// </summary>
        /// <param name="projectName">The project name to report.</param>
        /// <param name="jobName">The job name to report.</param>
        /// <param name="reportType">The report type of the execution (local, cloud, both).</param>
        /// <param name="reportName">The name of the local generated report.</param>
        /// <param name="reportPath">The path to the local generated report.</param>
        public ReportSettings(
            string projectName,
            string jobName,
            ReportType reportType = ReportType.CLOUD_AND_LOCAL,
            string reportName = null,
            string reportPath = null)
        {
            this.ProjectName = projectName;
            this.JobName = jobName;
            this.ReportType = reportType;
            this.ReportName = reportName;
            this.ReportPath = reportPath;
        }

        /// <summary>
        /// Override equals method of <see cref="ReportSettings"/> class.
        /// </summary>
        /// <param name="obj">Target object to compare to.</param>
        /// <returns> True if both objects are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || !this.GetType().Name.Equals(obj.GetType().Name))
            {
                return false;
            }

            ReportSettings that = obj as ReportSettings;
            return object.Equals(this.ProjectName, that.ProjectName) && object.Equals(this.JobName, that.JobName);
        }

    }
}
