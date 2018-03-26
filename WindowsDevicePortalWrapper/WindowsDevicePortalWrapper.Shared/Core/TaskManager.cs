//----------------------------------------------------------------------------------------------
// <copyright file="TaskManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Task Manager methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for starting or stopping a modern application.
        /// </summary>
        public static readonly string TaskManagerApi = "api/taskmanager/app";

        /// <summary>
        /// Starts running the specified application.
        /// </summary>
        /// <param name="appid">Application ID</param>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>Process identifier for the application instance.</returns>
        public async Task<uint> LaunchApplicationAsync(
            string appid,
            string packageName)
        {
            string payload = string.Format(
                "appid={0}&package={1}",
                Utilities.Hex64Encode(appid), 
                Utilities.Hex64Encode(packageName));

            await this.PostAsync(
                TaskManagerApi, 
                payload);

            RunningProcesses runningApps = await this.GetRunningProcessesAsync();

            uint processId = 0;
            foreach (DeviceProcessInfo process in runningApps.Processes)    
            {
                if (string.Compare(process.PackageFullName, packageName) == 0)
                {
                    processId = process.ProcessId;
                    break;
                }
            }

            return processId;
        }

        public async Task<List<string>> GetRunningAppsAsync()
        {
            RunningProcesses runningApps = await this.GetRunningProcessesAsync();
            List<string> runningProcessNames = new List<string>();

            foreach (DeviceProcessInfo process in runningApps.Processes)
            {
                // There can be multiple processes per app, so only add an app package name
                // if it doesn't already exist in the list
                if (process.PackageFullName != null && !runningProcessNames.Contains(process.PackageFullName))
                {
                    runningProcessNames.Add(process.PackageFullName);
                }
            }

            return runningProcessNames;
        }

        /// <summary>
        /// Stops the specified application from running.
        /// </summary>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>
        /// Task for tracking termination completion
        /// </returns>
        public async Task TerminateApplicationAsync(string packageName)
        {
            await this.DeleteAsync(
                TaskManagerApi,
                string.Format("package={0}", Utilities.Hex64Encode(packageName)));
        }
    }
}
