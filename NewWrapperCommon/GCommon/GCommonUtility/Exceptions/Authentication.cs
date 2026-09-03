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

namespace AvePoint.GCommon.Utility.Exceptions.Authentication
{
    [Serializable]
    public class ConnectToDomainServerFailedException : AveException
    {
        public ConnectToDomainServerFailedException(string domainServer)
        {
            Contexts.Add(ContextKeys.Authentication.DomainServer,domainServer);
        }
    }

    [Serializable]
    public class DeactivatedUserException : AveException
    {
        public DeactivatedUserException()
        {
        }
    }

    [Serializable]
    public class LicenseAgreementException : AveException
    {
    }

    [Serializable]
    public class ExceededMaximumUserSessionException: AveException
    {
        public ExceededMaximumUserSessionException(int maximumUserSessionCount​)
        {
            Contexts.Add(ContextKeys.Authentication.MaximumUserSessionCount,maximumUserSessionCount.ToString());
        }
    }

    [Serializable]
    public class ForbiddenIPException : AveException
    {
        public ForbiddenIPException(string IP)
        {
            Contexts.Add(ContextKeys.Socket.IP,IP);
        }
    }

    [Serializable]
    public class InactiveUserException : AveException
    {
        public InactiveUserException()
        {
        }
    }

    [Serializable]
    public class IncorrectUserNameOrPasswordException : AveException
    {
        public IncorrectUserNameOrPasswordException()
        {
        }
    }

    [Serializable]
    public class UserNotAddInAccountManagerException : AveException
    {
        public UserNotAddInAccountManagerException()
        {
        }
    }

    [Serializable]
    public class UserPasswordExpiredException  : AveException
    {
        public UserPasswordExpiredException()
        {
        }
    }
}
