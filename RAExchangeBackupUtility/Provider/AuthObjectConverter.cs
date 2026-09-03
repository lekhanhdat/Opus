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

namespace ExchangeBackupUtility
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using ExchangeUtility;

    /// <summary>
    /// Provides conversion utilities between different AuthObject types.
    /// </summary>
    public static class AuthObjectConverter
    {
        /// <summary>
        /// Creates an EWS-compatible AuthObject from a Graph AuthObject by using the BposInfo.
        /// </summary>
        /// <param name="bposInfo">The BPOS information used to create the original auth object.</param>
        /// <returns>An AuthObject compatible with EWS operations.</returns>
        public static AuthObject CreateEwsAuthObjectFromBposInfo(BposInfo bposInfo)
        {
            if (bposInfo == null)
            {
                throw new ArgumentNullException(nameof(bposInfo));
            }

            return AuthObjectFactory.CreateAuthObject(bposInfo, AuthResourceType.EWS);
        }

        /// <summary>
        /// Attempts to convert an IAuthObject to ExchangeUtility.AuthObject.
        /// If direct cast fails, creates a new EWS auth object from BposInfo.
        /// </summary>
        /// <param name="authObject">The auth object to convert.</param>
        /// <param name="bposInfo">The BPOS info to use if conversion is needed.</param>
        /// <returns>An EWS-compatible AuthObject.</returns>
        public static AuthObject ToEwsAuthObject(ExchangeUtility.Graph.IAuthObject authObject, BposInfo bposInfo)
        {
            if (authObject is AuthObject ewsAuth)
            {
                return ewsAuth;
            }

            // If it's a Graph auth object, create a new EWS auth object from BposInfo
            return CreateEwsAuthObjectFromBposInfo(bposInfo);
        }
    }
}
