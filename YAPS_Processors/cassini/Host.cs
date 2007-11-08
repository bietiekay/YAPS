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
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    //
    // Instances of an internal Host class are created in the App Domain
    // processing HTTP request. Typeof(Host) is passed to CreateApplicationHost()
    //
    // Host uses System.Net to listen to the configured port and accept
    // incoming HTTP connections.
    //
    internal class Host : MarshalByRefObject {
        private bool _started;
        private bool _stopped;

        private Server _server;

        private int  _port;
        private String _virtualPath;
        private String _lowerCasedVirtualPath;
        private String _lowerCasedVirtualPathWithTrailingSlash;
        private String _physicalPath;
        private String _installPath;
        private String _physicalClientScriptPath;
        private String _lowerCasedClientScriptPathWithTrailingSlashV10;
        private String _lowerCasedClientScriptPathWithTrailingSlashV11;

        private Socket _socket;

        private WaitCallback _onStart;
        private WaitCallback _onSocketAccept;
        private EventHandler _onAppDomainUnload;

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        public void Configure(Server server, int port, String virtualPath, String physicalPath, String installPath) {
            _server = server;

            _port = port;
            _virtualPath = virtualPath;
            _lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(_virtualPath);
            _lowerCasedVirtualPathWithTrailingSlash = virtualPath.EndsWith("/") ? virtualPath : virtualPath + "/";
            _lowerCasedVirtualPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(_lowerCasedVirtualPathWithTrailingSlash);
            _physicalPath = physicalPath;
            _installPath = installPath;
            _physicalClientScriptPath = installPath + "\\asp.netclientfiles\\";

            String version4 = FileVersionInfo.GetVersionInfo(typeof(HttpRuntime).Module.FullyQualifiedName).FileVersion;
            String version3 = version4.Substring(0, version4.LastIndexOf('.'));
            _lowerCasedClientScriptPathWithTrailingSlashV10 = "/aspnet_client/system_web/" + version4.Replace('.', '_') + "/";
            _lowerCasedClientScriptPathWithTrailingSlashV11 = "/aspnet_client/system_web/" + version3.Replace('.', '_') + "/";

            _onSocketAccept = new WaitCallback(OnSocketAccept);
            _onStart = new WaitCallback(OnStart);

            // start watching for app domain unloading
            _onAppDomainUnload = new EventHandler(OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += _onAppDomainUnload;
        }

        public String NormalizedVirtualPath {
            get { return _lowerCasedVirtualPathWithTrailingSlash; }
        }

        public String PhysicalPath {
            get { return _physicalPath; } 
        }

        public String InstallPath {
            get { return _installPath; } 
        }

        public String PhysicalClientScriptPath {
            get { return _physicalClientScriptPath; } 
        }

        public int Port {
            get { return _port; }
        }

        public String VirtualPath {
            get { return _virtualPath; } 
        }

        public bool IsVirtualPathInApp(String path) {
            bool isClientScriptPath;
            String clientScript;
            return IsVirtualPathInApp(path, out isClientScriptPath, out clientScript);
        }

        public bool IsVirtualPathInApp(String path, out bool isClientScriptPath, out String clientScript) {
            isClientScriptPath = false;
            clientScript = null;

            if (path == null)
                return false;

            if (_virtualPath == "/" && path.StartsWith("/")) {
                if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlashV10)) {
                    isClientScriptPath = true;
                    clientScript = path.Substring(_lowerCasedClientScriptPathWithTrailingSlashV10.Length);
                }

                if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlashV11)) {
                    isClientScriptPath = true;
                    clientScript = path.Substring(_lowerCasedClientScriptPathWithTrailingSlashV11.Length);
                }

                return true;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            if (path.StartsWith(_lowerCasedVirtualPathWithTrailingSlash))
                return true;

            if (path == _lowerCasedVirtualPath)
                return true;

            if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlashV10)) {
                isClientScriptPath = true;
                clientScript = path.Substring(_lowerCasedClientScriptPathWithTrailingSlashV10.Length);
                return true;
            }

            if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlashV11)) {
                isClientScriptPath = true;
                clientScript = path.Substring(_lowerCasedClientScriptPathWithTrailingSlashV11.Length);
                return true;
            }

            return false;
        }

        public bool IsVirtualPathAppPath(String path) {
            if (path == null)
                return false;

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            return (path == _lowerCasedVirtualPath || path == _lowerCasedVirtualPathWithTrailingSlash);
        }

        private void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
            Thread.GetDomain().DomainUnload -= _onAppDomainUnload;

            if (_stopped)
                return;

            Stop();

            _server.Restart();
            _server = null;
        }

        private void OnSocketAccept(Object acceptedSocket) {
            Connection conn =  new Connection(this, (Socket)acceptedSocket);
            conn.ProcessOneRequest();
        }

        private void OnStart(Object unused) {
            while (_started) {
                try {
                    Socket socket = _socket.Accept();
                    ThreadPool.QueueUserWorkItem(_onSocketAccept, socket);
                }
                catch {
                    Thread.Sleep(100);
                }
            }

            _stopped = true;
        }

        public void Start() {
            if (_started)
                throw new InvalidOperationException();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socket.Listen((int)SocketOptionName.MaxConnections);

            _started = true;
            ThreadPool.QueueUserWorkItem(_onStart);
        }

        public void Stop() {
            if (!_started)
                return;

            _started = false;

            try {
                // _socket.Shutdown(SocketShutdown.Both);  /* blocks! */
                _socket.Close();
            }
            catch {
            }
            finally {
                _socket = null;
            }

            while (!_stopped)
                Thread.Sleep(100);
        }

    }
}
