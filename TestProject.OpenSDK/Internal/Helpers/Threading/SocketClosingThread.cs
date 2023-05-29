﻿// <copyright file="SocketClosingThread.cs" company="TestProject">
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

namespace TestProject.OpenSDK.Internal.Helpers.Threading
{
    using System;
    using NLog;
    using TestProject.OpenSDK.Internal.Rest;
    using TestProject.OpenSDK.Internal.Tcp;

    /// <summary>
    /// A class that spawns a separate thread to gracefully close an open development socket.
    /// </summary>
    public class SocketClosingThread : BaseThread, IDisposable
    {
        private static Logger Logger { get; set; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClosingThread"/> class.
        /// </summary>
        public SocketClosingThread()
            : base()
        {
            ShutdownThreadHelper.Instance.Register(int.MaxValue, this);
        }

        /// <summary>
        /// Closes the open development socket.
        /// </summary>
        public override void RunThread()
        {
            Logger.Info("Closing socket gracefully...");
            AgentClient.GetInstance().Stop();
            SocketManager.GetInstance().CloseSocket();
        }

        /// <inheritdoc cref="IDisposable"/>
        public void Dispose()
        {
            ShutdownThreadHelper.Instance.Unregister(int.MaxValue);
        }
    }
}
