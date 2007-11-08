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
    //  Internal Request class provides the implementation of
    //  HttpWorkerRequest (by deriving from SimpleWorkerRequest).
    //
    //  Request object are create one per client request as passed to
    //  ASP.NET using HttpRuntime.ProcessRequest
    //
    internal class Request : SimpleWorkerRequest {
        private Host _host;
        private Connection _conn;

        // raw request data
        private const int maxHeaderBytes = 32*1024;
        private byte[] _headerBytes;
        private int    _startHeadersOffset;
        private int    _endHeadersOffset;
        private ArrayList _headerByteStrings;

        // parsed request data
        private String _verb;
        private String _url;
        private String _prot;

        private String _path;
        private String _filePath;
        private String _pathInfo;
        private String _pathTranslated;
        private String _queryString;
        private byte[] _queryStringBytes;

        private int    _contentLength;
        private int    _preloadedContentLength;
        private byte[] _preloadedContent;

        private String     _allRawHeaders;
        private String[][] _unknownRequestHeaders;
        private String[]   _knownRequestHeaders;

        // cached response
        private bool          _headersSent;
        private int           _responseStatus;
        private StringBuilder _responseHeadersBuilder;
        private ArrayList     _responseBodyBytes;

        private bool          _specialCaseStaticFileHeaders;


        public Request(Host host, Connection conn) : base(String.Empty, String.Empty, null) {
            _host = host;
            _conn = conn;
        }

        public void Process() {

            ReadAllHeaders();

            if (_headerBytes == null || _endHeadersOffset < 0 || 
                _headerByteStrings == null || _headerByteStrings.Count == 0) {
                _conn.WriteErrorAndClose(400);
                return;
            }

            ParseRequestLine();

            // Check for bad path
            if (IsBadPath()) {
                _conn.WriteErrorAndClose(400);
                return;
            }

            // Limit to local requests only
            if (!_conn.IsLocal) {
                _conn.WriteErrorAndClose(403);
                return;
            }

            // Check if the path is not well formed or is not for the current app
            bool isClientScriptPath = false;
            String clientScript = null;

            if (!_host.IsVirtualPathInApp(_path, out isClientScriptPath, out clientScript)) {
                _conn.WriteErrorAndClose(404);
                return;
            }

            ParseHeaders();

            ParsePostedContent();

            if (_verb == "POST" && _contentLength > 0 && _preloadedContentLength < _contentLength) {
                _conn.Write100Continue();
            }

            // special case for client script
            if (isClientScriptPath) {
                _conn.WriteEntireResponseFromFile(_host.PhysicalClientScriptPath + clientScript, false);
                return;
            }

            // special case for directory listing
            if (ProcessDirectoryListingRequest()) {
                return;
            }

            PrepareResponse();

            // Hand the processing over to HttpRuntime

            HttpRuntime.ProcessRequest(this);
        }

        private bool TryReadAllHeaders() {
            // read the first packet (up to 32K)
            byte[] headerBytes = _conn.ReadRequestBytes(maxHeaderBytes);

            if (headerBytes == null || headerBytes.Length == 0)
                return false;

            if (_headerBytes != null) {
                // previous partial read
                int len = headerBytes.Length + _headerBytes.Length;
                if (len > maxHeaderBytes)
                    return false;

                byte[] bytes = new byte[len];
                Buffer.BlockCopy(_headerBytes, 0, bytes, 0, _headerBytes.Length);
                Buffer.BlockCopy(headerBytes, 0, bytes, _headerBytes.Length, headerBytes.Length);
                _headerBytes = bytes;
            }
            else {
                _headerBytes = headerBytes;
            }

            // start parsing
            _startHeadersOffset = -1;
            _endHeadersOffset = -1;
            _headerByteStrings = new ArrayList();

            // find the end of headers
            ByteParser parser = new ByteParser(_headerBytes);

            for (;;) {
                ByteString line = parser.ReadLine();

                if (line == null)
                    break;

                if (_startHeadersOffset < 0) {
                    _startHeadersOffset = parser.CurrentOffset;
                }

                if (line.IsEmpty) {
                    _endHeadersOffset = parser.CurrentOffset;
                    break;
                }

                _headerByteStrings.Add(line);
            }

            return true;
        }

        private void ReadAllHeaders() {
            _headerBytes = null;

            do {
                if (!TryReadAllHeaders())
                    break; // something bad happened
            }
            while (_endHeadersOffset < 0); // found \r\n\r\n
        }

        private void ParseRequestLine() {
            ByteString requestLine = (ByteString)_headerByteStrings[0];
            ByteString[] elems = requestLine.Split(' ');

            if (elems == null || elems.Length < 2 || elems.Length > 3) {
                return;
            }

            _verb = elems[0].GetString();

            ByteString urlBytes = elems[1];
            _url = urlBytes.GetString();

            if (elems.Length == 3)
                _prot = elems[2].GetString();
            else
                _prot = "HTTP/1.0";

            // query string

            int iqs = urlBytes.IndexOf('?');
            if (iqs > 0)
                _queryStringBytes = urlBytes.Substring(iqs+1).GetBytes();
            else
                _queryStringBytes = new byte[0];

            iqs = _url.IndexOf('?');
            if (iqs > 0) {
                _path = _url.Substring(0, iqs);
                _queryString = _url.Substring(iqs+1);
            }
            else {
                _path = _url;
                _queryStringBytes = new byte[0];
            }

            // url-decode path

            if (_path.IndexOf('%') >= 0) {
                _path = HttpUtility.UrlDecode(_path);
            }

            // path info

            int lastDot = _path.LastIndexOf('.');
            int lastSlh = _path.LastIndexOf('/');

            if (lastDot >= 0 && lastSlh >= 0 && lastDot < lastSlh) {
                int ipi = _path.IndexOf('/', lastDot);
                _filePath = _path.Substring(0, ipi);
                _pathInfo = _path.Substring(ipi);
            }
            else {
                _filePath = _path;
                _pathInfo = String.Empty;
            }

            _pathTranslated = MapPath(_filePath);
        }

        private static char[] s_badPathChars = new char[] { '%', '>', '<', '$', ':' };
        private bool IsBadPath() {
            if (_path == null)
                return true;

            if (_path.IndexOfAny(s_badPathChars) >= 0)
                return true;

            if (_path.IndexOf("..") >= 0)
                return true;

            return false;
        }

        private void ParseHeaders() {
            _knownRequestHeaders = new String[RequestHeaderMaximum];

            // construct unknown headers as array list of name1,value1,...
            ArrayList headers = new ArrayList();

            for (int i = 1; i < _headerByteStrings.Count; i++) {
                String s = ((ByteString)_headerByteStrings[i]).GetString();

                int c = s.IndexOf(':');

                if (c >= 0) {
                    String name = s.Substring(0, c).Trim();
                    String value = s.Substring(c+1).Trim();

                    // remember
                    int knownIndex = GetKnownRequestHeaderIndex(name);
                    if (knownIndex >= 0) {
                        _knownRequestHeaders[knownIndex] = value;
                    }
                    else {
                        headers.Add(name);
                        headers.Add(value);
                    }
                }
            }

            // copy to array unknown headers

            int n = headers.Count / 2;
            _unknownRequestHeaders = new String[n][];
            int j = 0;

            for (int i = 0; i < n; i++) {
                _unknownRequestHeaders[i] = new String[2];
                _unknownRequestHeaders[i][0] = (String)headers[j++];
                _unknownRequestHeaders[i][1] = (String)headers[j++];
            }

            // remember all raw headers as one string

            if (_headerByteStrings.Count > 1)
                _allRawHeaders = Encoding.UTF8.GetString(_headerBytes, _startHeadersOffset, _endHeadersOffset-_startHeadersOffset);
            else
                _allRawHeaders = String.Empty;
        }

        private void ParsePostedContent() {
            _contentLength = 0;
            _preloadedContentLength = 0;

            String contentLengthValue = _knownRequestHeaders[HttpWorkerRequest.HeaderContentLength];
            if (contentLengthValue != null) {
                try {
                    _contentLength = Int32.Parse(contentLengthValue);
                }
                catch {
                }
            }

            if (_headerBytes.Length > _endHeadersOffset) {
                _preloadedContentLength = _headerBytes.Length - _endHeadersOffset;

                if (_preloadedContentLength > _contentLength && _contentLength > 0)
                    _preloadedContentLength = _contentLength; // don't read more than the content-length

                _preloadedContent = new byte[_preloadedContentLength];
                Buffer.BlockCopy(_headerBytes, _endHeadersOffset, _preloadedContent, 0, _preloadedContentLength);
            }
        }

        private static String[] s_defaultFilenames = new String[] { "default.aspx", "default.htm", "default.html" };

        private bool ProcessDirectoryListingRequest() {
            if (_verb != "GET")
                return false;

            // last element extension-less?
            int i1 = _pathTranslated.LastIndexOf('\\');
            int i2 = _pathTranslated.IndexOf('.', i1);
            if (i2 >= i1)
                return false;

            // now check if directory
            if (!Directory.Exists(_pathTranslated))
                return false;

            // have to redirect /foo to /foo/ to allow relative links to work
            if (!_path.EndsWith("/")) {
                String newPath = _path + "/";
                String location = "Location: " + newPath + "\r\n";
                String body = "<html><head><title>Object moved</title></head><body>\r\n" +
                              "<h2>Object moved to <a href='" + newPath + "'>here</a>.</h2>\r\n" +
                              "</body></html>\r\n";

                _conn.WriteEntireResponseFromString(302, location, body, false);
                return true;
            }

            // check for the default file
            foreach (String filename in s_defaultFilenames) {
                String defaultFilePath = _pathTranslated + "\\" + filename;

                if (File.Exists(defaultFilePath)) {
                    // pretend the request is for the default file path
                    _path += filename;
                    _filePath = _path;
                    _url = (_queryString != null) ? (_path + "?" + _queryString) : _path;
                    _pathTranslated = defaultFilePath;
                    return false; // go through normal processing
                }
            }

            // get all files and subdirs
            FileSystemInfo[] infos = null;
            try {
                infos = (new DirectoryInfo(_pathTranslated)).GetFileSystemInfos();
            }
            catch {
            }

            // determine if parent is appropriate
            String parentPath = null;

            if (_path.Length > 1) {
                int i = _path.LastIndexOf('/', _path.Length-2);
                parentPath = (i > 0) ?_path.Substring(0, i) : "/";
                if (!_host.IsVirtualPathInApp(parentPath))
                    parentPath = null;
            }
            
            _conn.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n",
                    Messages.FormatDirectoryListing(_path, parentPath, infos), false);
            return true;
        }

        private void PrepareResponse() {
            _headersSent = false;
            _responseStatus = 200;
            _responseHeadersBuilder = new StringBuilder();
            _responseBodyBytes = new ArrayList();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation of HttpWorkerRequest
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////

        public override String GetUriPath() {
            return _path;
        }

        public override String GetQueryString() {
            return _queryString;
        }

        public override byte[] GetQueryStringRawBytes() {
            return _queryStringBytes;
        }

        public override String GetRawUrl() {
            return _url;
        }

        public override String GetHttpVerbName() {
            return _verb;
        }

        public override String GetHttpVersion() {
            return _prot;
        }

        public override String GetRemoteAddress() {
            return _conn.RemoteIP;
        }

        public override int GetRemotePort() {
            return 0;
        }

        public override String GetLocalAddress() {
            return _conn.LocalIP;
        }

        public override int GetLocalPort() {
            return _host.Port;
        }

        public override String GetFilePath() {
            return _filePath;
        }

        public override String GetFilePathTranslated() {
            return _pathTranslated;
        }

        public override String GetPathInfo() {
            return _pathInfo;
        }

        public override String GetAppPath() {
            return _host.VirtualPath;
        }

        public override String GetAppPathTranslated() {
            return _host.PhysicalPath;
        }

        public override byte[] GetPreloadedEntityBody() {
            return _preloadedContent;
        }

        public override bool IsEntireEntityBodyIsPreloaded() {
            return (_contentLength == _preloadedContentLength);
        }

        public override int ReadEntityBody(byte[] buffer, int size)  {
            int bytesRead = 0;
            byte[] bytes = _conn.ReadRequestBytes(size);

            if (bytes != null && bytes.Length > 0) {
                bytesRead = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRead);
            }

            return bytesRead;
        }

        public override String GetKnownRequestHeader(int index)  {
            return _knownRequestHeaders[index];
        }
    
        public override String GetUnknownRequestHeader(String name) {
            int n = _unknownRequestHeaders.Length;

            for (int i = 0; i < n; i++) {
                if (String.Compare(name, _unknownRequestHeaders[i][0], true, CultureInfo.InvariantCulture) == 0)
                    return _unknownRequestHeaders[i][1];
            }

            return null;
        }

        public override String[][] GetUnknownRequestHeaders() {
            return _unknownRequestHeaders;
        } 

        public override String GetServerVariable(String name) {
            String s = String.Empty;

            switch (name) {
            case "ALL_RAW":
                s = _allRawHeaders;
                break;
            case "SERVER_PROTOCOL":
                s = _prot;
                break;
            case "SERVER_SOFTWARE":
                s = "Microsoft-Cassini/" + Messages.VersionString;
                break;
            // more needed?
            }

            return s;
        }

        public override String MapPath(String path) {
            String mappedPath = String.Empty;

            if (path == null || path.Length == 0 || path.Equals("/")) {
                // asking for the site root
                if (_host.VirtualPath == "/") {
                    // app at the site root
                    mappedPath = _host.PhysicalPath;
                }
                else {
                    // unknown site root - don't point to app root to avoid double config inclusion
                    mappedPath = Environment.SystemDirectory;
                }
            }
            else if (_host.IsVirtualPathAppPath(path)) {
                // application path
                mappedPath = _host.PhysicalPath;
            }
            else if (_host.IsVirtualPathInApp(path)) {
                // inside app but not the app path itself
                mappedPath = _host.PhysicalPath + path.Substring(_host.NormalizedVirtualPath.Length);
            }
            else {
                // outside of app -- make relative to app path
                if (path.StartsWith("/"))
                    mappedPath = _host.PhysicalPath + path.Substring(1);
                else
                    mappedPath = _host.PhysicalPath + path;
            }

            mappedPath = mappedPath.Replace('/', '\\');

            if (mappedPath.EndsWith("\\") && !mappedPath.EndsWith(":\\"))
                mappedPath = mappedPath.Substring(0, mappedPath.Length-1);

            return mappedPath;
        }

        public override void SendStatus(int statusCode, String statusDescription) {
            _responseStatus = statusCode;
        }

        public override void SendKnownResponseHeader(int index, String value) {
            if (_headersSent)
                return;

            switch (index) {
                case HttpWorkerRequest.HeaderServer:
                case HttpWorkerRequest.HeaderDate:
                case HttpWorkerRequest.HeaderConnection:
                    // ignore these
                    return;

                // special case headers for static file responses
                case HttpWorkerRequest.HeaderAcceptRanges:
                    if (value == "bytes") {
                        _specialCaseStaticFileHeaders = true;
                        return;
                    }
                    break;
                case HttpWorkerRequest.HeaderExpires:
                case HttpWorkerRequest.HeaderLastModified:
                    if (_specialCaseStaticFileHeaders)
                        return;
                    break;
            }

            _responseHeadersBuilder.Append(GetKnownResponseHeaderName(index));
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public override void SendUnknownResponseHeader(String name, String value) {
            if (_headersSent)
                return;

            _responseHeadersBuilder.Append(name);
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public override void SendCalculatedContentLength(int contentLength) {
            if (!_headersSent) {
                _responseHeadersBuilder.Append("Content-Length: ");
                _responseHeadersBuilder.Append(contentLength.ToString());
                _responseHeadersBuilder.Append("\r\n");
            }
        }

        public override bool HeadersSent() {
            return _headersSent;
        }

        public override bool IsClientConnected() {
            return _conn.Connected;
        }

        public override void CloseConnection() {
            _conn.Close();
        }
    
        public override void SendResponseFromMemory(byte[] data, int length) {
            if (length > 0) {
                byte[] bytes = new byte[length];
                Buffer.BlockCopy(data, 0, bytes, 0, length);
                _responseBodyBytes.Add(bytes);
            }
        }

        public override void SendResponseFromFile(String filename, long offset, long length) {
            if (length == 0)
                return;

            FileStream f = null;

            try {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null)
                    f.Close();
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
            if (length == 0)
                return;

            FileStream f = null;

            try {
                f = new FileStream(handle, FileAccess.Read, false);
                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null)
                    f.Close();
            }
        }

        private void SendResponseFromFileStream(FileStream f, long offset, long length)  {
            const int maxChunkLength = 64*1024;
            long fileSize = f.Length;

            if (length == -1)
                length = fileSize - offset;

            if (length == 0 || offset < 0 || length > fileSize - offset)
                return;

            if (offset > 0)
                f.Seek(offset, SeekOrigin.Begin);

            if (length <= maxChunkLength) {
                byte[] fileBytes = new byte[(int)length];
                int bytesRead = f.Read(fileBytes, 0, (int)length);
                SendResponseFromMemory(fileBytes, bytesRead);
            }
            else {
                byte[] chunk = new byte[maxChunkLength];
                int bytesRemaining = (int)length;

                while (bytesRemaining > 0) {
                    int bytesToRead = (bytesRemaining < maxChunkLength) ? bytesRemaining : maxChunkLength;
                    int bytesRead = f.Read(chunk, 0, bytesToRead);
                    SendResponseFromMemory(chunk, bytesRead);
                    bytesRemaining -= bytesRead;

                    // flush to release keep memory
                    if (bytesRemaining > 0 && bytesRead > 0)
                        FlushResponse(false);
                }
            }
        }

        public override void FlushResponse(bool finalFlush) {
            if (!_headersSent) {
                _conn.WriteHeaders(_responseStatus, _responseHeadersBuilder.ToString());
                _headersSent = true;
            }

            for (int i = 0; i < _responseBodyBytes.Count; i++) {
                byte[] bytes = (byte[])_responseBodyBytes[i];
                _conn.WriteBody(bytes, 0, bytes.Length);
            }

            _responseBodyBytes = new ArrayList();

            if (finalFlush) {
                _conn.Close();
            }
        }

        public override void EndOfRequest() {
            // empty method
        }
    }
}
