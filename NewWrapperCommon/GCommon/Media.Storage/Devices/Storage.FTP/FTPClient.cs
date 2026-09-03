/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace AvePoint.Media.Storage.FTP
{
    #region using directives
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using Wrapper;
    #endregion

    class FtpClient
    {
        Int32 port;
        String host;
        String userName;
        String password;
        FtpSchema schema;
        Int32 timeOut;
        Int32 bufferSize = 1024 * 64;
        String rootFolder;
        String ftpType;
        Boolean usePassive;
        FTPRetry ftpRetry;
        //Default value is true for ADO-197324
        Boolean useFluentFTP;
        readonly StorageLogger logger = new StorageLogger(typeof(FtpClient));

        public void Open(FtpSchema schema, String hostName, Int32 port, String userName, String password, String rootFolder, String ftpType, Boolean isRetry, Int32 maxRetryCount, Int32 retryInternal, Boolean usePassive, Boolean useFluentFTP)
        {
            this.host = hostName;
            this.schema = schema;
            this.rootFolder = rootFolder;
            this.port = port > 0 ? port : XRIParameterKeys.FTP_DEFAULT_PORT;
            this.userName = String.IsNullOrEmpty(userName) ? XRIParameterKeys.FTP_DEFAULT_NAME : userName;
            this.password = password;
            this.timeOut = 120 * 1000;
            this.ftpType = ftpType;
            this.usePassive = usePassive;
            this.useFluentFTP = useFluentFTP;
            this.ftpRetry = new FTPRetry(isRetry, maxRetryCount, retryInternal);
        }

        public FtpWebRequest BuildFtpWebRequest(String path, String method)
        {
            try
            {
                path = String.IsNullOrEmpty(rootFolder) ? path.Replace("\\", "/") : PathUtil.CombinePath(rootFolder, path).Replace("\\", "/");
                var requestUrl = new UriBuilder(FtpSchema.Ftp.ToString(), this.host, this.port) + Uri.EscapeDataString(path);
                var request = (FtpWebRequest)WebRequest.Create(requestUrl);
                request.Credentials = new NetworkCredential(userName, password);
                request.Method = method;
                request.UseBinary = true;
                request.UsePassive = this.usePassive;
                request.ReadWriteTimeout = this.timeOut;
                request.Timeout = this.timeOut;
                request.EnableSsl = schema == FtpSchema.Ftps;
                return request;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while building request, Schema : {0}, host : {1}, port : {2}, path : {3}, method : {4}, timeout : {5},exception:{6}.", schema, host, port, path, method, timeOut, ex.ToString());
                throw;
            }
        }

        public void MakeDirectory(String pathname)
        {
            if (this.useFluentFTP)
            {
                return;
            }
            pathname = pathname.Replace("/", "\\");
            var dirs = pathname.Split('\\');
            var dir = new StringBuilder();
            for (var i = 0; i < dirs.Length; i++)
            {
                if (i == 0)
                {
                    dir.Append(dirs[i]);
                }
                else
                {
                    dir.Append("\\" + dirs[i]);
                }
                if (!String.IsNullOrEmpty(dir.ToString().Trim()))
                {
                    if (!CheckDirectory(dir.ToString()))
                    {
                        if (CreateDirectory(dir.ToString()))
                        {
                            logger.Debug("Make the directory {0} successful", dir);
                        }
                    }
                }
            }
        }

        public bool CreateDirectory(String pathname)
        {
            if (this.useFluentFTP)
            {
                return true;
            }
            var result = default(bool);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.MakeDirectory);
            request.KeepAlive = false;
            try
            {
                return ftpRetry.Retry(delegate
                {
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.PathnameCreated || response.StatusCode == FtpStatusCode.FileActionOK)
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception("Make directory failed, StatusCode is " + response.StatusCode);
                        }
                        return result;
                    }
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient CreateDirectory failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Make directory {0} failed, detail:{1} ", pathname, e.ToString());
                throw;
            }
        }
        /// <summary>
        /// upload a test file;use for FtpSystem.Validate()
        /// </summary>
        /// <param name="fileName">test file</param>
        /// <param name="localStream">test file's MemoryStream</param>
        /// <returns></returns>
        public Boolean StoreFile(string fileName, Stream localStream)
        {
            var result = default(bool);
            var request = BuildFtpWebRequest(fileName, WebRequestMethods.Ftp.UploadFile);
            request.KeepAlive = true;
            request.ContentLength = localStream.Length;
            try
            {
                return ftpRetry.Retry(delegate
                {
                    using (var requestStream = request.GetRequestStream())
                    {
                        Int32 readLength;
                        var buffer = new byte[bufferSize];
                        while ((readLength = localStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            requestStream.Write(buffer, 0, readLength);
                        }
                    }
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception("Create file failed:" + response.StatusCode);
                        }
                    }
                    return result;
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient StoreFile failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Can not create file {0} on this ftp server, please check the user's authority {1}", fileName, e.ToString());
                throw;
            }
            catch (Exception e)
            {
                logger.Error("Can not create file {0} on this ftp server, please check the user's authority {1}", fileName, e.ToString());
                throw;
            }
        }
        public Boolean DeleteFile(String pathname)
        {
            var result = default(bool);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.DeleteFile);
            request.KeepAlive = true;
            try
            {
                return ftpRetry.Retry(delegate ()
                {
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.FileActionOK || response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception("Delete file failed:" + response.StatusCode.ToString());
                        }
                        return result;
                    }
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient DeleteFile failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Delete file {0} failed {1}", pathname, e.ToString());
                throw;
            }
        }
        public StorageDeleteResult DeleteDirectory(String pathname)
        {
            var result = new StorageDeleteResult();
            pathname = AssmbleDirectoryPathName(pathname);
            var files = ListDirectoryAndFiles(pathname);
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = file.Name;
                    //different ftp server will return different path
                    if (!fileName.Contains(pathname))
                    {
                        fileName = PathUtil.CombinePath(pathname, fileName);
                    }
                    if (!file.IsDirectory)
                    {
                        var fileSize = GetFileSize(fileName);
                        DeleteFile(fileName);
                        result.DeletedFileSize += fileSize;
                    }
                    else
                    {
                        result.DeletedFileSize += DeleteDirectory(fileName).DeletedFileSize;
                    }
                }
            }
            result.IsDeleted = RemoveDirectory(pathname);
            return result;
        }

        public Boolean RemoveDirectory(string pathname)
        {
            var result = default(bool);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.RemoveDirectory);
            request.KeepAlive = false;
            try
            {
                return ftpRetry.Retry(delegate
                {
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.FileActionOK || response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            result = true;
                        }
                        else
                        {
                            throw new Exception("Delete directory failed:" + response.StatusCode.ToString());
                        }
                    }
                    return result;
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient RemoveDirectory failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Delete directory {0} failed:{1}", pathname, e.ToString());
                throw;
            }
        }
        public Stream GetUploadStream(string pathname)
        {
            if (this.useFluentFTP)
            {
                return this.CreateCommandUploadStream(pathname);
            }
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.UploadFile);
            request.KeepAlive = true;
            try
            {
                return ftpRetry.Retry(request.GetRequestStream);
            }
            catch (WebException e)
            {
                request.Abort();
                logger.Error("FtpClient GetUploadStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Get ftp upload stream failed {0}", e.ToString());
                throw;
            }
        }
        public Stream GetAppendStream(string pathname)
        {
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.AppendFile);
            request.KeepAlive = true;
            try
            {
                return ftpRetry.Retry(request.GetRequestStream);
            }
            catch (WebException e)
            {
                request.Abort();
                logger.Error("FtpClient GetAppendStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Get ftp append stream failed, please check the FTP server allows users to append. {0}", e);
                throw;
            }
        }
        public Stream GetDownloadStream(string pathname, long offset)
        {
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.DownloadFile);
            request.KeepAlive = true;
            request.ContentOffset = offset;
            try
            {
                return ftpRetry.Retry(() => request.GetResponse().GetResponseStream());
            }
            catch (WebException e)
            {
                request.Abort();
                logger.Error("FtpClient GetDownloadStream failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Get ftp down stream  failed {0}", e.ToString());
                throw;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100016:CheckIfTheReturnValueIsNoGreaterThanZero")]
        private String ReadLine(Socket clientSocket)
        {
            var message = String.Empty;
            var buffer = new Byte[4096];
            while (true)
            {
                var readLength = clientSocket.Receive(buffer, buffer.Length, 0);
                message += Encoding.ASCII.GetString(buffer, 0, readLength);
                if (readLength < buffer.Length) break;
            }
            var messageBlocks = message.Split(new[] { '\n' });
            message = message.Length > 2 ? messageBlocks[messageBlocks.Length - 2] : messageBlocks[0];
            if (!message.Substring(3, 1).Equals(" "))
            {
                return ReadLine(clientSocket);
            }
            return message;
        }

        private String SendCommand(String command, Socket clientSocket)
        {
            var cmdBytes = Encoding.UTF8.GetBytes((command + "\r\n").ToCharArray());
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
            return ReadLine(clientSocket);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "utf")]
        public Int64 GetFileSize(String pathname)
        {
            var size = default(Int64);
            var relativePath = String.Empty;
            var flag = default(Int32);
            var fileName = pathname;
            if ((flag = pathname.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                relativePath = pathname.Substring(0, flag);
                fileName = pathname.Substring(flag + 1);
            }
            if ((size = this.GetFileSizeWithCommand(pathname)) == -1)
            {
                foreach (var file in this.ListDirectoryAndFiles(relativePath))
                {
                    if (file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        size = file.Size;
                        break;
                    }
                }
            }
            return size;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "utf")]
        private Int64 GetFileSizeWithCommand(String pathname)
        {
            var size = default(Int64);
            Socket clientSocket = null;
            try
            {
                var reply = String.Empty;
                Int32 statusCode;
                String highName;
                String lowName;
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (pathname.Contains("\\"))
                {
                    highName = pathname.Substring(0, pathname.LastIndexOf("\\", StringComparison.Ordinal));
                    lowName = pathname.Substring(pathname.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                }
                else
                {
                    highName = String.Empty;
                    lowName = pathname.TrimStart('\\');
                }
                var filePath = PathUtil.CombinePath(rootFolder, highName).Replace("\\", "/");
                clientSocket.Connect(this.host, this.port);
                reply = ReadLine(clientSocket);
                statusCode = Int32.Parse(reply.Substring(0, 3));
                if (statusCode != 220)
                {
                    throw new IOException("An error occurred when socket connecting, message:" + reply.Substring(4));
                }
                if (this.schema == FtpSchema.Ftps)
                {
                    size = this.GetFileSizeBySslStream(clientSocket, filePath, lowName);
                }
                else
                {
                    reply = SendCommand(String.Format("USER {0}", this.userName), clientSocket);
                    statusCode = Int32.Parse(reply.Substring(0, 3));
                    if (!(statusCode == 331 || statusCode == 230))
                    {
                        throw new IOException("An error occurred when send user command, message:" + reply.Substring(4));
                    }
                    if (statusCode != 230)
                    {
                        reply = SendCommand(String.Format("PASS {0}", this.password), clientSocket);
                        statusCode = Int32.Parse(reply.Substring(0, 3));
                        if (!(statusCode == 230 || statusCode == 202))
                        {
                            throw new IOException("An error occurred when send pass command, message:" + reply.Substring(4));
                        }
                    }
                    SendCommand("OPTS utf8 on", clientSocket);
                    reply = SendCommand(String.Format("CWD {0}", filePath), clientSocket);
                    statusCode = Int32.Parse(reply.Substring(0, 3));
                    if (statusCode != 250)
                    {
                        throw new IOException("An error occurred when send CWD command, message:" + reply.Substring(4));
                    }
                    reply = SendCommand("TYPE I", clientSocket);
                    statusCode = Int32.Parse(reply.Substring(0, 3));
                    if (statusCode != 200)
                    {
                        throw new IOException("An error occurred when send type i command, message:" + reply.Substring(4));
                    }
                    reply = SendCommand(String.Format("SIZE {0}", lowName), clientSocket);
                    statusCode = Int32.Parse(reply.Substring(0, 3));
                    if (statusCode == 213)
                    {
                        size = Int64.Parse(reply.Substring(4));
                        logger.Debug("size = {0}, lowName = {1}", size, lowName);
                    }
                    else
                    {
                        throw new IOException("An error occurred when send size command, message:" + reply.Substring(4));
                    }
                }
            }
            catch (Exception e)
            {
                size = -1;
                logger.Warn("Get file {0} 's size with command failed {1}", pathname, e.ToString());
            }
            finally
            {
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
            }
            return size;
        }

        private Int64 GetFileSizeBySslStream(Socket socket, String filePath, String fileName)
        {
            Stream socketStream = null;
            String command = default(String);
            String message = default(String);
            Int64 size = default(Int64);
            Byte[] cmdBytes = new Byte[4096];
            Int64 readLen = default(Int64);
            var buffer = new Byte[4096];
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, "e1 7b ed e9 31 c3 19 86 5a ba 06 73 e1 53 17 7f 55 57 73 5b", false);
            if (certCollection.Count != 1)
            {
                throw new ArgumentException("can not find certificate.");
            }
            var serverCertificate = certCollection[0];
            var certificateCollection = new X509CertificateCollection();
            certificateCollection.Add(serverCertificate);
            store.Close();
            SendCommand(String.Format("AUTH {0}", "TLS"), socket);

            socketStream = new NetworkStream(socket, false);
            var sslStream = new SslStream(socketStream, false, delegate { return true; });
            sslStream.AuthenticateAsClient(this.host, certificateCollection, SslProtocols.Tls, false);
            socketStream = sslStream;
            command = String.Format("USER {0}", this.userName);
            cmdBytes = Encoding.UTF8.GetBytes((command + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            cmdBytes = Encoding.UTF8.GetBytes(((String.Format("PASS {0}", this.password)) + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            cmdBytes = Encoding.UTF8.GetBytes(((String.Format("OPTS utf8 on")) + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            cmdBytes = Encoding.UTF8.GetBytes(((String.Format("CWD {0}", filePath)) + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            cmdBytes = Encoding.UTF8.GetBytes(((String.Format("TYPE I")) + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            cmdBytes = Encoding.UTF8.GetBytes(((String.Format("SIZE {0}", fileName)) + "\r\n").ToCharArray());
            socketStream.Write(cmdBytes, 0, cmdBytes.Length);
            readLen = socketStream.Read(buffer, 0, buffer.Length);
            message = Encoding.ASCII.GetString(buffer);

            size = Int64.Parse(message.Substring(4, message.IndexOf("\r") - 3));

            return size;
        }

        public DateTime GetLastModifiedTime(string pathname)
        {
            var lastModifyTime = default(DateTime);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.GetDateTimestamp);
            request.KeepAlive = false;
            try
            {
                return ftpRetry.Retry(delegate
                {
                    using (var response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.FileStatus)
                        {
                            lastModifyTime = response.LastModified;
                        }
                        else
                        {
                            throw new Exception("Make directory failed:" + response.StatusCode.ToString());
                        }
                    }
                    return lastModifyTime;
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient GetLastModifiedTime failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("Get file {0} 's size failed {1}", pathname, e.ToString());
                throw;
            }
        }
        public FileStruct[] ListDirectoryAndFiles(String pathname)
        {
            pathname = AssmbleDirectoryPathName(pathname);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.ListDirectoryDetails);
            request.KeepAlive = false;
            FileStruct[] result = null;
            var ftpParser = new FTPDirectoryListParser();
            try
            {
                return ftpRetry.Retry(delegate
                {
                    if (CheckDirectory(pathname))
                    {
                        using (var response = (FtpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode == FtpStatusCode.OpeningData || response.StatusCode == FtpStatusCode.DataAlreadyOpen)
                            {
                                var en = this.ftpType.Equals("win03", StringComparison.OrdinalIgnoreCase) ? Encoding.Default : Encoding.UTF8;
                                using (var reader = new StreamReader(response.GetResponseStream(), en))
                                {
                                    result = ftpParser.GetList(reader.ReadToEnd());
                                }
                            }
                            else
                            {
                                throw new Exception("List directories failed:" + response.StatusCode);
                            }
                        }
                    }
                    return result;
                });
            }
            catch (WebException e)
            {
                logger.Error("FtpClient ListDirectoryAndFiles failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                logger.Error("list ftp files from directory {0} failed {1}", pathname, e.ToString());
                throw;
            }
        }
        /// <summary>
        /// because of the special of FileZilla; so don't use .net api
        /// use socket connect to the ftp server and send rnfr
        /// </summary>
        public Boolean CheckFile(string pathname)
        {
            if (this.useFluentFTP)
            {
                return false;
            }
            var result = default(bool);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.DownloadFile);
            request.KeepAlive = false;
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == FtpStatusCode.OpeningData || response.StatusCode == FtpStatusCode.DataAlreadyOpen)
                    {
                        result = true;
                    }
                    else
                    {
                        logger.Warn("Check file finished, but status is {0}. We will use list command to check whether the file exist or not.", response.StatusCode);
                        var folder = pathname.Contains("\\") ? pathname.Substring(0, pathname.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase)) : String.Empty;
                        var file = pathname.Substring(pathname.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                        result = ListDirectoryAndFiles(folder).ToList().Exists(f => f.Name.Equals(file, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Trace.TraceWarning(e.Message);
                var folder = pathname.Contains("\\") ? pathname.Substring(0, pathname.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase)) : String.Empty;
                var file = pathname.Substring(pathname.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                result = ListDirectoryAndFiles(folder).ToList().Exists(f => f.Name.Equals(file, StringComparison.OrdinalIgnoreCase));
            }
            catch (WebException e)
            {
                logger.Error("FtpClient CheckFile failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                var resp = e.Response as FtpWebResponse;
                if (resp != null && resp.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    result = false;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }
        public Boolean CheckDirectory(string pathname)
        {
            if (this.useFluentFTP)
            {
                return true;
            }
            var result = default(bool);
            pathname = AssmbleDirectoryPathName(pathname);
            var request = BuildFtpWebRequest(pathname, WebRequestMethods.Ftp.ListDirectory);
            request.KeepAlive = false;
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == FtpStatusCode.OpeningData || response.StatusCode == FtpStatusCode.DataAlreadyOpen)
                    {
                        result = true;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Trace.TraceWarning(e.Message);
                result = false;
            }
            catch (WebException e)
            {
                logger.Error("FtpClient CheckDirectory failed, status description {0}.", ((FtpWebResponse)e.Response).StatusDescription);
                var resp = e.Response as FtpWebResponse;
                if (resp != null && resp.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    result = false;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        private String AssmbleDirectoryPathName(String pathName)
        {
            var path = pathName.Replace("\\", "/");
            return path.TrimEnd('/') + "/";
        }

        public Stream CreateCommandUploadStream(string path)
        {
            WrapperFtpClient client = null;
            try
            {
                client = this.GetWrapperConnection();
                path = String.IsNullOrEmpty(rootFolder) ? path.Replace("\\", "/") : PathUtil.CombinePath(rootFolder, path).Replace("\\", "/");
                var stream = client.OpenWrite(path);
                return new WrapperFTPStream(stream, client);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when create stream, message: " + e.ToString());
                try
                {
                    client?.Dispose();
                    client = null;
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred when close connection, message: " + ex.ToString());
                }
                throw;
            }
        }

        public void CheckConnection()
        {
            WrapperFtpClient client = null;
            try
            {
                client = this.GetWrapperConnection();
            }
            finally
            {
                try
                {
                    client?.Dispose();
                    client = null;
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred when close connection, message: " + ex.ToString());
                }
            }
        }

        public WrapperFtpClient GetWrapperConnection()
        {
            WrapperFtpClient client = null;
            try
            {
                client = new AvePoint.Media.Storage.FTP.Wrapper.WrapperFtpClient();
                client.Host = this.host;
                client.Port = this.port;
                client.DataConnectionType = usePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
                client.EncryptionMode = schema == FtpSchema.Ftps ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
                client.Credentials = new NetworkCredential(this.userName, this.password);
                client.Connect();
                return client;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when connect to ftp, message: " + e.ToString());
                try
                {
                    client?.Dispose();
                    client = null;
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred when close connection, message: " + ex.ToString());
                }
                throw;
            }
        }
    }

    class WrapperFTPStream : Stream
    {
        private WrapperFtpClient ftpClient;
        private Stream innerStream;

        public WrapperFTPStream(Stream innerStream, WrapperFtpClient ftpClient)
        {
            this.innerStream = innerStream;
            this.ftpClient = ftpClient;
        }

        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return innerStream.Position;
            }

            set
            {
                innerStream.Position = value;
            }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            try
            {
                innerStream?.Close();
                innerStream = null;
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }

            try
            {
                ftpClient?.Dispose();
                ftpClient = null;
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
        }
    }
}
