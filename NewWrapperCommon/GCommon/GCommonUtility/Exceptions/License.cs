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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.GCommon.Utility.Exceptions.License
{

    [Serializable]
    public class LicenseException : AveException
    {
        public LicenseException() { }

        public LicenseException(string msg)
            : base(msg)
        { }
    }

    [Serializable]
    public class LicenseExpireException : LicenseException
    {
        public LicenseExpireException() { }

        public LicenseExpireException(string msg)
            : base(msg)
        { }
    }

    [Serializable]
    public class IncorrectFormatException : LicenseException
    {
        public IncorrectFormatException() { }

    }

    [Serializable]
    public class DecryptionException : LicenseException
    {
        public DecryptionException() { }

    }

    [Serializable]
    public class LicenseNotFoundException : LicenseException
    {
        public LicenseNotFoundException(string filePath)
        {
            Contexts.Add(ContextKeys.File.Path, filePath);
        }
    }

    [Serializable]
    public class IllegalLicenseException : LicenseException
    {
        public IllegalLicenseException() { }
    }

    [Serializable]
    public class IPMismatchException : LicenseException
    {
        public IPMismatchException(string hostAddress, string licenseAddress)
        {
            Contexts.Add(ContextKeys.License.HostAddress, hostAddress);
            Contexts.Add(ContextKeys.License.LicenseAddress, licenseAddress);
        }
    }

}
