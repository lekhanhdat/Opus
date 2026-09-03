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




namespace AvePoint.Media.Storage.SFTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.Media.Storage.Util;
    #endregion

    class SFTPNodeInfo
    {
        private string hostName;
        private string userName;
        private string password;
        private string privateKey;
        private string privateKeyPassword;
        private int port;
        private string highName;
        private string lowName;
        private long offset;
        private int bufferSize;

        public long Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public string FilePath
        {
            get { return PathUtil.CombinePath(highName, lowName); }
        }

        public string FileName
        {
            get { return lowName; }
            set { lowName = value; }
        }

        public string HighName
        {
            get { return highName; }
            set { highName = value; }
        }

        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string PrivateKey
        {
            get { return privateKey; }
            set { privateKey = value; }
        }

        public string PrivateKeyPassword
        {
            get { return privateKeyPassword; }
            set { privateKeyPassword = value; }
        }

        public int Port
        {
            get { return port; }
            set
            {
                if (value > 0)
                {
                    port = value;
                }
                else
                {
                    port = 22;
                }
            }
        }

        public int BufferSize
        {
            get { return bufferSize; }
            set { bufferSize = value; }
        }

        public override bool Equals(object obj)
        {
            var result = default(bool);

            if (!string.IsNullOrEmpty(HighName) && !string.IsNullOrEmpty(FileName))
            {
                result = HighName.Equals(((SFTPNodeInfo)obj).HighName) && lowName.Equals(((SFTPNodeInfo)obj).FileName);
            }
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Node path : " + highName + "\r\n NodeName: " + lowName;
        }
    }
}
