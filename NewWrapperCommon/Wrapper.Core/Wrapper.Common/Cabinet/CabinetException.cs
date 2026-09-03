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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    internal class CabinetException : IOException
    {
        private int error;
        private int errorCode;
        private static ResourceManager errorResources;

        public CabinetException() : this(0, 0, null, null)
        {
        }

        public CabinetException(string message) : this(0, 0, message, null)
        {
        }

        protected CabinetException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.error = info.GetInt32("cabError");
            this.errorCode = info.GetInt32("cabErrorCode");
        }

        public CabinetException(string message, Exception innerException) : this(0, 0, message, innerException)
        {
        }

        internal CabinetException(int error, int errorCode, string message) : this(error, errorCode, message, null)
        {
        }

        internal CabinetException(int error, int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            this.error = error;
            this.errorCode = errorCode;
        }

        internal static string GetErrorMessage(int error, int errorCode, bool extracting)
        {
            int num = extracting ? 0x7d0 : 0x3e8;
            string str = "CabinetError" + ((num + error)).ToString(CultureInfo.InvariantCulture.NumberFormat);
            if (str == null)
            {
                str = "CabinetError" + num.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }
            if (errorCode != 0)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0} " + "CabinetError1", new object[] { str, errorCode });
            }
            return str;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("cabError", this.error);
            info.AddValue("cabErrorCode", this.errorCode);
            base.GetObjectData(info, context);
        }

        public int Error
        {
            get
            {
                return this.error;
            }
        }

        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        internal static ResourceManager ErrorResources
        {
            get
            {
                if (errorResources == null)
                {
                    errorResources = new ResourceManager(typeof(CabinetException).Namespace + ".Errors", typeof(CabinetException).Assembly);
                }
                return errorResources;
            }
        }
    }
}

