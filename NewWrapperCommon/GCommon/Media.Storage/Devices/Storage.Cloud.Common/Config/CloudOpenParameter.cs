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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection;
    using AvePoint.Media.Storage.Util;
    #endregion

    class CloudOpenParameter : OpenParameter
    {
        public string AccessPoint { get; set; }
        public string UserName{ get; set; }
        public string Password { get; set; }
        public string Region { get; set; }
        public bool CdnEnaled { get; set; }
        public string CdnGuid { get; set; }
        public string CType { get; set; }
        public int ModuleType { get; set; }
        public string SystemLocation  { get; set; }
        public bool FlushDNS { get; set; }
        public String Protocol { get; set; }

        private long blockLength = 64;
        public long BlockLength
        {
            get { return blockLength; }
            set { blockLength = value; }
        }

        public CloudOpenParameter()
        {
            this.FlushDNS = true;
            this.NeedRetry = true;
        }

        public string this[string key]
        {
            get 
            {
                Type t = this.GetType();
                return null;
            }

            set
            {

                PropertyInfo pi = this.GetType().GetProperty(key);
                if (pi != null)
                {
                    Type t = pi.PropertyType;
                    object o = value;
                    if (value.GetType() != t)
                    {
                        o = pi.PropertyType.GetMethod("Parse").Invoke(pi.PropertyType, new object[] { value });
                    }
                    pi.GetSetMethod().Invoke(this, new object[] { o });
                }
            }
        }
    }
}
