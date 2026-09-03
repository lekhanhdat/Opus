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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net.Sockets;
    using System.Net;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon.Contract.CodeReview;
    #endregion

    [AveCodeReview(
    "2012/2/29",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_1},
    "ADO-26069",
    true)] 

    class FTPDotNetExtend : IDisposable
    {
        Socket clientSocket;
        StorageLogger logger = new StorageLogger(MethodBase.GetCurrentMethod().DeclaringType);
        List<string> replyLines = new List<string>();
        public string ReplyString { get; set;}
        public int ReplyCode { get; set;}

        public void Open(string hostName, int port, string userName, string password)
        {
            try
            {
                Connect(hostName, port);
                if (!(ReplyCode >= 200 && ReplyCode < 300))
                {
                    logger.Info(string.Format("FTP server refused connection. replyCode = {0}", ReplyCode));
                    DisConnect();
                }
                else
                {
                    if (!Login(userName, password))
                    {
                        logger.Info(string.Format("wrong user name or wrong password . replyCode = {0}", ReplyCode));
                        LogOut();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("can not connection to the server : please check the ftp server info or your firewall configure", e);
                throw;
            }
        }

        public void Connect(string hostName, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep= new IPEndPoint(Dns.GetHostEntry(hostName).AddressList[0], port); // need test detail
            try
            {
                clientSocket.SendTimeout = 120 * 1000;
                clientSocket.ReceiveTimeout = 120 * 1000;
                clientSocket.Connect(ep);
                GetReplyCode();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Connect ftp server : {0} failed", hostName), e);
                throw;
            }
        }

        public void DisConnect()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
        }

        public bool Login(string userName, string password)
        {
            var result = default(bool);
            sendCommend("USER", userName);
            if (ReplyCode >= 200 && ReplyCode < 300)
            {
                result = true;
            }
            if (ReplyCode >= 300 && ReplyCode < 400)
            {
                result = false;
            }
            sendCommend("PASS", password);

            result = ReplyCode >= 200 && ReplyCode < 300;
            return result;
        }

        private void sendCommend(string command, string args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(command);
            if (!string.IsNullOrEmpty(args))
            {
                sb.Append(' ');
                sb.Append(args);
            }
            byte[] cmdBytes = Encoding.UTF8.GetBytes((sb + "\r\n").ToCharArray());
            clientSocket.Send(cmdBytes, cmdBytes.Length, SocketFlags.None);
            GetReplyCode();
        }

        private void GetReplyCode()
        {
            ReplyCode = -1;
            ReplyString = string.Empty;
            ReplyString = GetFtpResponseString();
            try
            {
                ReplyCode = Int32.Parse(ReplyString.Substring(0, 3));
                logger.Debug(string.Format("Ftp server RelyCode is {0} and ReplyString is {1}", ReplyCode, ReplyString));
            }
            catch (Exception e)
            {
                logger.Error(e.Message + "ReplyString is" + ReplyString, e);
            }
        }

        private string GetFtpResponseString()
        {
            byte[] buffer = new byte[512];
            int length = 0;
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                length = clientSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                sb.Append(Encoding.UTF8.GetString(buffer, 0, length));
                if (length <= 0 || length < buffer.Length)
                {
                    break;
                }
            }
            string returnMessage = sb.ToString();

            string[] mess = returnMessage.Split(new char[]{'\n'});
            if (mess.Length > 2)
            {
                returnMessage = mess[mess.Length - 2];
            }
            else
            {
                returnMessage = mess[0];
            }
            if (!returnMessage.Substring(3,1).Equals(" "))
            {
                GetFtpResponseString();
            }
            return returnMessage;
        }
   
        public void Allo(int bytes)
        {
            sendCommend("allo", Convert.ToString(bytes));
        }

        public void Rnfr(string pathname)
        {
            sendCommend("rnfr", pathname);
        }

        public void LogOut()
        {
            sendCommend("quit", null);
        }

        public void Dispose()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
        }
    }
}
