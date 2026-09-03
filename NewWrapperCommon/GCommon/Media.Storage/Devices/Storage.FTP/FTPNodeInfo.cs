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
    using System.IO;
    using AvePoint.Media.Storage.Util;
    #endregion

    class FTPNodeInfo
    {
        private String schema;
        private String hostName;
        private String userName;
        private String password;
        private Int32 port;
        private String highName;
        private String lowName;
        private Int64 offset;
        private String ftpType = "win08";
        private Int32 maxRetryCount;
        private Boolean isRetry;
        private Int32 retryInternal;
        private Boolean usePassive = true;
        private Boolean useFluentFTP;
        public bool UsePassive
        {
            get { return usePassive; }
            set { usePassive = value; }
        }
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


        public string Schema
        {
            get { return schema; }
            set { schema = value; }
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

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string FtpType
        {
            get { return this.ftpType; }
            set { this.ftpType = value; }
        }

        public override bool Equals(object obj)
        {
            var result = default(bool);

            if (!string.IsNullOrEmpty(HighName) && !string.IsNullOrEmpty(FileName))
            {
                result = HighName.Equals(((FTPNodeInfo)obj).HighName) && lowName.Equals(((FTPNodeInfo)obj).FileName);
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

        public Int32 MaxRetryCount
        {
            get { return this.maxRetryCount; }
            set { this.maxRetryCount = value; }
        }
        public Boolean IsRetry
        {
            get { return this.isRetry; }
            set { this.isRetry = value; }
        }
        public Int32 RetryInternal
        {
            get { return this.retryInternal; }
            set { this.retryInternal = value; }
        }
        public Boolean UseFluentFTP
        {
            get { return this.useFluentFTP; }
            set { this.useFluentFTP = value; }
        }
    }
}
