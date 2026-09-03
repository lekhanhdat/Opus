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

    internal static class IdcrlConstants
    {
        public const string BPOSIDCRL_AUTHORIZATION_HEADER_PREFIX = "BPOSIDCRL ";
        public const string ENV_INT_MSO = "INT-MSO";
        public const string ENV_PRODUCTION = "production";
        public const string False = "f";
        public const string HEADER_FORMS_BASED_AUTH_ACCEPTED = "X-FORMS_BASED_AUTH_ACCEPTED";
        public const string HEADER_IDCRL_AUTH_ACCEPTED = "X-IDCRL_ACCEPTED";
        public const string HEADER_IDCRL_AUTH_PARAMS_V1 = "X-IDCRL_AUTH_PARAMS_V1";
        public const string IDCRL_PARAM_ENDPOINT = "ENDPOINT";
        public const string IDCRL_PARAM_IDCRL_TYPE = "IDCRL TYPE";
        public const string IDCRL_PARAM_POLICY = "POLICY";
        public const string IDCRL_PARAM_ROOTDOMAIN = "ROOTDOMAIN";
        public const string IDCRLTYPE_BPOSIDRL = "BPOSIDCRL";
        public const string REGKEY_MSOIdentityCRL = @"SOFTWARE\Microsoft\MSOIdentityCRL";
        public const string REGVAL_ServiceEnvironment = "ServiceEnvironment";
        public const string True = "t";
    }
}

