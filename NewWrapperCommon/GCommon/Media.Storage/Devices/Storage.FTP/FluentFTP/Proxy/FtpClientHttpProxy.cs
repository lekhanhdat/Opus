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

namespace AvePoint.Media.Storage.FTP.Wrapper.Proxy
{
    #region using directives
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary> A FTP client with a HTTP 1.1 proxy implementation. </summary>
    public class FtpClientHttpProxy : FtpClientProxy
    {
        /// <summary> A FTP client with a HTTP 1.1 proxy implementation </summary>
        /// <param name="proxy">Proxy information</param>
		public FtpClientHttpProxy(ProxyInfo proxy)
            : base(proxy)
        {
            ConnectionType = "HTTP 1.1 Proxy";
        }

        /// <summary> Redefine the first dialog: HTTP Frame for the HTTP 1.1 Proxy </summary>
        protected override void Handshake()
        {
            var proxyConnectionReply = GetReply();
            if (!proxyConnectionReply.Success)
                throw new FtpException("Can't connect " + Host + " via proxy " + Proxy.Host + ".\nMessage : " +
                                        proxyConnectionReply.ErrorMessage);
        }

        protected override WrapperFtpClient Create()
        {
            return new FtpClientHttpProxy(Proxy);
        }

        protected override void Connect(FtpSocketStream stream)
        {
            Connect(stream, Host, Port, FtpIpVersion.ANY);
        }

        protected override void Connect(FtpSocketStream stream, string host, int port, FtpIpVersion ipVersions)
        {
            base.Connect(stream);

            var writer = new StreamWriter(stream);
            writer.WriteLine("CONNECT {0}:{1} HTTP/1.1", host, port);
            writer.WriteLine("Host: {0}:{1}", host, port);
            if (Proxy.Credentials != null)
            {
                var credentialsHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", Proxy.Credentials.UserName, Proxy.Credentials.Password)));
                writer.WriteLine("Proxy-Authorization: Basic {0}", credentialsHash);
            }
            writer.WriteLine("User-Agent: custom-ftp-client");
            writer.WriteLine();
            writer.Flush();

            ProxyHandshake(stream);
        }

        private void ProxyHandshake(FtpSocketStream stream)
        {
            var proxyConnectionReply = GetProxyReply(stream);
            if (!proxyConnectionReply.Success)
                throw new FtpException("Can't connect " + Host + " via proxy " + Proxy.Host + ".\nMessage : " + proxyConnectionReply.ErrorMessage);
        }

        private FtpReply GetProxyReply(FtpSocketStream stream)
        {

            FtpReply reply = new FtpReply();
            string buf;

            lock (Lock)
            {
                if (!IsConnected)
                    throw new InvalidOperationException("No connection to the server has been established.");

                stream.ReadTimeout = ReadTimeout;
                while ((buf = stream.ReadLine(Encoding)) != null)
                {
                    Match m;

                    FtpTrace.WriteLine(buf);

                    if ((m = Regex.Match(buf, @"^HTTP/.*\s(?<code>[0-9]{3}) (?<message>.*)$")).Success)
                    {
                        reply.Code = m.Groups["code"].Value;
                        reply.Message = m.Groups["message"].Value;
                        break;
                    }

                    reply.InfoMessages += string.Format("{0}\n", buf);
                }
            }

            return reply;
        }
    }
}