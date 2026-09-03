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
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackOpenParameter : OpenParameter
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string TenantName { get; set; }
        public string TenantId { get; set; }
        public string AuthenticationURL { get; set; }
        public bool CdnEnabled { get; set; }
        public string SystemLocation { get; set; }

        public int AuthenticationVersion { get; set; }
        public string AuthenticationType { get; set; }
        public bool CreateIfNotExists { get; set; }
        public long SingleUploadMaxSize { get; set; }
        public long SegmentMinSize { get; set; }
        public long MaxFileSize { get; set; }
        public bool UploadCheckMD5 { get; set; }
        public bool EnableSLO { get; set; }
        public bool EnableBulkDelete { get; set; }

        public OpenStackOpenParameter()
        {
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
