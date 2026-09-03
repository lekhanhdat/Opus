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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    #region Authentication, UserName, Password

  
    [Serializable]
    public class NonOffice365AccountException : AveWrapperBaseException
    {
        public NonOffice365AccountException(string message) : base(message) { }

        public NonOffice365AccountException(AveInternalResourceKey key, params object[] args)
            : base(key,  args)
        {
        }
    }

    [Serializable]
    public class IncorrectUserNameOrPasswordException : AveWrapperBaseException
    {
        public IncorrectUserNameOrPasswordException(string message) : base(message) { }

        public IncorrectUserNameOrPasswordException(AveInternalResourceKey key,  params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class Office365SiteExpiredException : AveWrapperBaseException
    {
        public Office365SiteExpiredException(string message) : base(message) { }

        public Office365SiteExpiredException(AveInternalResourceKey key, params object[] args)
            : base(key,  args)
        {
        }
    }

    [Serializable]
    public class AccountDisableException : AveWrapperBaseException
    {
        public AccountDisableException(string message) : base(message) { }

        public AccountDisableException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AveChangeTokenExpireException : AveWrapperBaseException
    {
        public AveChangeTokenExpireException(string message)
            : base(message)
        {
        }
        public AveChangeTokenExpireException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    #endregion
}
