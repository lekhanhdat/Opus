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

namespace AvePoint.ObjectModel.ClientOM
{
    public enum ListItemUpdateMethodKind
    {
        None,
        Update,
        SystemUpdate,
        CustomSystemUpdate
    }

    public static class ErrorCodes
    {
        // Fields
        public const int AccessDenied = -2147024891;
        public const int DocAlreadyExists = -2130575257;
        public const int FieldValueFailedValidation = -2130575163;
        public const int GenericError = -1;
        public const int InvalidFieldValue = -2130575155;
        public const int ItemValueFailedValidation = -2130575162;
        public const int ListItemDeleted = -2130575338;
        public const int NotSupported = -2147024846;
        public const int NotSupportedRequestVersion = -2130575151;
        public const int Redirect = -2130575152;
        public const int VersionConflict = -2130575339;
    }

    public static class AveStandardErrorCode
    {
        public const int COR_E_APPLICATION = -2146232832;
        public const int COR_E_ARGUMENT = -2147024809;
        public const int COR_E_ARGUMENTOUTOFRANGE = -2146233086;
        public const int COR_E_DIRECTORYNOTFOUND = -2147024893;
        public const int COR_E_FILENOTFOUND = -2147024894;
        public const int COR_E_NULLREFERENCE = -2147467261;
        public const int COR_E_TIMEOUT = -2146233083;
        public const int COR_E_UNAUTHORIZEDACCESS = -2147024891;
        public const int E_POINTER = -2147467261;
    }

    public static class AveSPErrorCode
    {
        public const int TP_E_CANCELLED_BY_EVENT_HANDLER = -2130575223;
    }
}
