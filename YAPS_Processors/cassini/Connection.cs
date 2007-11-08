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
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    //
    // Internal class to handle HTTP connections using System.Net
    //
    internal class Connection {
        private Host _host;
        private Socket _socket;

        public Connection(Host host, Socket socket) {
            _host = host;
            _socket = socket;
        }

        public bool Connected {
            get { return _socket.Connected; }
        }

        public String LocalIP {
            get {
                IPEndPoint endPoint = (IPEndPoint)_socket.LocalEndPoint;
                if (endPoint != null && endPoint.Address != null)
                    return endPoint.Address.ToString();
                else
                    return "127.0.0.1";
            }
        }

        public String RemoteIP {
            get {
                IPEndPoint endPoint = (IPEndPoint)_socket.RemoteEndPoint;
                if (endPoint != null && endPoint.Address != null)
                    return endPoint.Address.ToString();
                else
                    return "127.0.0.1";
            }
        }

        public bool IsLocal {
            get {
                return (LocalIP == RemoteIP);
            }
        }

        public void Close() {
            try {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch {
            }
            finally {
                _socket = null;
            }
        }

        private static String MakeResponseHeaders(int statusCode, String moreHeaders, int contentLength, bool keepAlive) {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 " + statusCode + " " + HttpWorkerRequest.GetStatusDescription(statusCode) + "\r\n");
            sb.Append("Server: Microsoft-Cassini/"+Messages.VersionString+"\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");
            if (contentLength >= 0)
                sb.Append("Content-Length: " + contentLength + "\r\n");
            if (moreHeaders != null)
                sb.Append(moreHeaders);
            if (!keepAlive)
                sb.Append("Connection: Close\r\n");
            sb.Append("\r\n");
            return sb.ToString();
        }

        private static String MakeContentTypeHeader(String fileName) {
            String contentType = null;
            
            int lastDot = fileName.LastIndexOf('.');

            if (lastDot >= 0) {
                switch (fileName.Substring(lastDot)) {
                case ".js":      contentType = "application/x-javascript";   break;
                case ".gif":     contentType = "image/gif";                  break;
                case ".jpg":     contentType = "image/jpeg";                 break;
                }
            }

            if (contentType == null)
                return null;

            return "Content-Type: " + contentType + "\r\n";
        }

        public void ProcessOneRequest() {
            // wait for at least some input
            if (WaitForRequestBytes() == 0) {
                WriteErrorAndClose(400);
                return;
            }

            Request request = new Request(_host, this);
            request.Process();
        }

        public byte[] ReadRequestBytes(int maxBytes) {
            try {
                if (WaitForRequestBytes() == 0)
                    return null;

                int numBytes = _socket.Available;
                if (numBytes > maxBytes)
                    numBytes = maxBytes;

                int numReceived = 0;
                byte[] buffer = new byte[numBytes];

                if (numBytes > 0) {
                    numReceived = _socket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes) {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0) {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }

                return buffer;
            }
            catch {
                return null;
            }
        }

        public void Write100Continue() {
            WriteEntireResponseFromString(100, null, null, true);
        }

        public void WriteBody(byte[] data, int offset, int length) {
            _socket.Send(data, offset, length, SocketFlags.None);
        }

        public void WriteEntireResponseFromString(int statusCode, String extraHeaders, String body, bool keepAlive) {
            try {
                int bodyLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;
                String headers = MakeResponseHeaders(statusCode, extraHeaders, bodyLength, keepAlive);
                _socket.Send(Encoding.UTF8.GetBytes(headers + body));
            }
            finally {
                if (!keepAlive)
                    Close();
            }
        }

        public void WriteEntireResponseFromFile(String fileName, bool keepAlive) {
            if (!File.Exists(fileName)) {
                WriteErrorAndClose(404);
                return;
            }

            bool completed = false;
            FileStream fs = null;

            try {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int len = (int)fs.Length;
                byte[] fileBytes = new byte[len];
                int bytesRead = fs.Read(fileBytes, 0, len);

                String headers = MakeResponseHeaders(200, MakeContentTypeHeader(fileName), bytesRead, keepAlive);
                _socket.Send(Encoding.UTF8.GetBytes(headers));

                _socket.Send(fileBytes, 0, bytesRead, SocketFlags.None);

                completed = true;
            }
            finally {
                if (!keepAlive || !completed)
                    Close();

                if (fs != null)
                    fs.Close();
            }
        }

        public void WriteErrorAndClose(int statusCode, string message) {
            String body = Messages.FormatErrorMessageBody(statusCode, _host.VirtualPath);
            if (message != null && message.Length > 0)
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            WriteEntireResponseFromString(statusCode, null, body, false);
        }

        public void WriteErrorAndClose(int statusCode) {
            WriteErrorAndClose(statusCode, null);
        }

        private int WaitForRequestBytes() {
            int availBytes = 0;

            try {
                if (_socket.Available == 0) {
                    // poll until there is data
                    _socket.Poll(100000 /* 100ms */, SelectMode.SelectRead);
                    if (_socket.Available == 0 && _socket.Connected)
                        _socket.Poll(10000000 /* 10sec */, SelectMode.SelectRead);
                }

                availBytes = _socket.Available;
            }
            catch {
            }

            return availBytes;
        }

        public void WriteHeaders(int statusCode, String extraHeaders) {
            String headers = MakeResponseHeaders(statusCode, extraHeaders, -1, false);
            _socket.Send(Encoding.UTF8.GetBytes(headers));
        }
    }
}
