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

using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// User Mapping including domain mapping and place holder
    /// </summary>
    public interface IUserMapping
    {
        /// <summary>
        /// Get Mapping login name according to the user mapping and domain mapping rule.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        string GetMappingLoginName(string userName);

        /// <summary>
        /// Get Mapping login name decoder
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        SPUserLoginNameDecoder GetMappingLoginNameDecoder(string userName);

        /// <summary>
        /// Is Default User enabled
        /// </summary>
        bool IsDefaultUserEnabled { get; }

        /// <summary>
        /// user this account when user not found
        /// </summary>
        string DefaultUserAccount { get; }

        /// <summary>
        /// if enabled, the dead account will be restored in destination
        /// </summary>
        bool IsPlaceHolderEnabled { get; }

        /// <summary>
        /// use this account to add account, and replace it with the restoring user.
        /// </summary>
        string PlaceHolderAccount { get; }

        /// <summary>
        /// compatible with old implementation
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        Dictionary<string, string> ExportUserMapping();

        /// <summary>
        /// compatible with old implementation
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        Dictionary<string, string> ExportDomainMapping();
    }
}
