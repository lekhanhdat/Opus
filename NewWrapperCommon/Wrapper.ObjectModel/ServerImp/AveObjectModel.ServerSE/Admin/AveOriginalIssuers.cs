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



using Microsoft.SharePoint.Administration.Claims;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveOriginalIssuers : IAveOriginalIssuers
    {
        public string Format(AveOriginalIssuerType type, string identifier)
        {
            return SPOriginalIssuers.Format((SPOriginalIssuerType)type, identifier);
        }

        public string Format(AveOriginalIssuerType type)
        {
            return SPOriginalIssuers.Format((SPOriginalIssuerType)type);
        }

        public bool DoesIssuerTypeNeedIdentifier(AveOriginalIssuerType type)
        {
            return SPOriginalIssuers.DoesIssuerTypeNeedIdentifier((SPOriginalIssuerType)type);
        }

        public bool Equals(string issuerOne, string issuerTwo)
        {
            return SPOriginalIssuers.Equals(issuerOne, issuerTwo);
        }

        public string GetIssuerIdentifier(string value)
        {
            return SPOriginalIssuers.GetIssuerIdentifier(value);
        }

        public AveOriginalIssuerType GetIssuerType(string value)
        {
            return (AveOriginalIssuerType)SPOriginalIssuers.GetIssuerType(value);
        }

        public bool IsIssuerType(AveOriginalIssuerType type, string issuer)
        {
            return SPOriginalIssuers.IsIssuerType((SPOriginalIssuerType)type, issuer);
        }

        public bool IsValidIssuer(string value)
        {
            return SPOriginalIssuers.IsValidIssuer(value);
        }

        public bool IsValidIssuerIdentifier(string value)
        {
            return SPOriginalIssuers.IsValidIssuerIdentifier(value);
        }
    }
}
