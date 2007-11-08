/*=======================================================================
  Copyright (C) Microsoft Corporation.  All rights reserved.
 
  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.
=======================================================================*/

namespace Cassini {
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using Microsoft.Win32;

    //
    // Server class that provides public Cassini APIs to start and stop the Web Server.
    // Instances of Server are created in the caller's App Domain.
    //
    // Server creates and configures the new App Domain for processing of HTTP requests
    // using System.Web.Hosting.ApplicationHost.CreateApplicationHost()
    //
    public class Server : MarshalByRefObject {
        private int _port;
        private String _virtualPath;
        private String _physicalPath;
        private String _installPath;

        private WaitCallback _restartCallback;

        private Host _host;

        public Server(int port, String virtualPath, String physicalPath) {
            _port = port;
            _virtualPath = virtualPath;
            _physicalPath = physicalPath.EndsWith("\\") ? physicalPath : physicalPath + "\\";

            _restartCallback = new WaitCallback(RestartCallback);

            _installPath = GetInstallPathAndConfigureAspNetIfNeeded();

            CreateHost();
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        public string PhysicalPath {
            get {
                return _physicalPath;
            }
        }

        public int Port {
            get {
                return _port;
            }
        }

        public string RootUrl {
            get {
                return "http://localhost:" + _port + _virtualPath;
            }
        }

        public string VirtualPath {
            get {
                return _virtualPath;
            }
        }

        public string InstallPath {
            get {
                return _installPath;
            }
        }

        private void CreateHost() {
            _host = (Host)ApplicationHost.CreateApplicationHost(typeof(Host), _virtualPath, _physicalPath);
            _host.Configure(this, _port, _virtualPath, _physicalPath, _installPath);
        }

        public void Restart() {
            ThreadPool.QueueUserWorkItem(_restartCallback);
        }

        private void RestartCallback(Object unused) {
            CreateHost();
            Start();
        }

        public void Start() {
            if (_host != null)
                _host.Start();
        }

        public void Stop() {
            if (_host != null) {
                try {
                    _host.Stop();
                }
                catch {
                    // don't throw on error to stop
                }
            }
        }

        private String GetInstallPathAndConfigureAspNetIfNeeded() {
            // If ASP.NET was never registered on this machine, the registry 
            // needs to be patched up for System.Web.dll to find aspnet_isapi.dll
            //
            // If HKLM\Microsoft\ASP.NET key is missing, this will be added
            //      (adjusted for the correct directory and version number
            // [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET]
            //      "RootVer"="1.0.3514.0"
            // [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\ASP.NET\1.0.3514.0]
            //      "Path"="E:\WINDOWS\Microsoft.NET\Framework\v1.0.3514"
            //      "DllFullPath"="E:\WINDOWS\Microsoft.NET\Framework\v1.0.3514\aspnet_isapi.dll"

            const String aspNetKeyName = @"Software\Microsoft\ASP.NET";

            RegistryKey aspNetKey = null;
            RegistryKey aspNetVersionKey = null;
            RegistryKey frameworkKey = null;

            String installPath = null;

            try {
                // get the version corresponding to System.Web.Dll currently loaded
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(typeof(HttpRuntime).Module.FullyQualifiedName);
                String aspNetVersion = string.Format("{0}.{1}.{2}.{3}", ver.FileMajorPart, ver.FileMinorPart, ver.FileBuildPart, ver.FilePrivatePart);
                String aspNetVersionKeyName = aspNetKeyName + "\\" + aspNetVersion;

                // non 1.0 names should have 0 QFE in the registry
                if (!aspNetVersion.StartsWith("1.0."))
                    aspNetVersionKeyName = aspNetVersionKeyName.Substring(0, aspNetVersionKeyName.LastIndexOf('.')+1) + "0";

                // check if the subkey with version number already exists
                aspNetVersionKey = Registry.LocalMachine.OpenSubKey(aspNetVersionKeyName);

                if (aspNetVersionKey != null) {
                    // already created -- just get the path
                    installPath = (String)aspNetVersionKey.GetValue("Path");
                }
                else {
                    // open/create the ASP.NET key
                    aspNetKey = Registry.LocalMachine.OpenSubKey(aspNetKeyName);
                    if (aspNetKey == null) {
                        aspNetKey = Registry.LocalMachine.CreateSubKey(aspNetKeyName);
                        // add RootVer if creating
                        aspNetKey.SetValue("RootVer", aspNetVersion);
                    }

                    // version dir name is almost version: "1.0.3514.0" -> "v1.0.3514"
                    String versionDirName = "v" + aspNetVersion.Substring(0, aspNetVersion.LastIndexOf('.'));

                    // install directory from "InstallRoot" under ".NETFramework" key
                    frameworkKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework");
                    String rootDir = (String)frameworkKey.GetValue("InstallRoot");
                    if (rootDir.EndsWith("\\"))
                        rootDir = rootDir.Substring(0, rootDir.Length-1);

                    // create the version subkey
                    aspNetVersionKey = Registry.LocalMachine.CreateSubKey(aspNetVersionKeyName);

                    // install path
                    installPath = rootDir + "\\" + versionDirName;

                    // set path and dllfullpath
                    aspNetVersionKey.SetValue("Path", installPath);
                    aspNetVersionKey.SetValue("DllFullPath", installPath + "\\aspnet_isapi.dll");
                }
            }
            catch {
            }
            finally {
                if (aspNetVersionKey != null)
                    aspNetVersionKey.Close();
                if (aspNetKey != null)
                    aspNetKey.Close();
                if (frameworkKey != null)
                    frameworkKey.Close();
            }

            return installPath;
        }
    }
}

