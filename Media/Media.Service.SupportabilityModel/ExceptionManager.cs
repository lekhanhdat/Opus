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



namespace AvePoint.Media.Service.SupportabilityModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel.Event;
    #endregion

    internal static class ExceptionManager
    {
        static List<Type> defaultAllowThrowableExceptions = new List<Type>();
        static List<Type> defaultAllowCatchableExceptions = new List<Type>();

        static ExceptionManager()
        {
            var assembly = typeof(EventExceptionBase).Assembly;
            defaultAllowThrowableExceptions.AddRange(
                Array.FindAll(assembly.GetExportedTypes(), type => typeof(EventExceptionBase).IsAssignableFrom(type)));
            defaultAllowCatchableExceptions.AddRange(
                Array.FindAll(assembly.GetExportedTypes(), type => typeof(EventExceptionBase).IsAssignableFrom(type)));
        }

        public static List<Type> DefaultAllowThrowableExceptions { get { return defaultAllowThrowableExceptions; } }
        public static List<Type> DefaultAllowCatchableExceptions { get { return defaultAllowCatchableExceptions; } }
    }
}