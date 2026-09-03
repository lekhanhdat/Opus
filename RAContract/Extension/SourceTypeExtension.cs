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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Extension
{
    public static class SourceTypeExtension
    {
        public static PaidForModule Map2PaidForModule(this SourceType singleSourceType)
        {
            var dic = new Dictionary<SourceType, PaidForModule> { 
                { SourceType.FileSystem, PaidForModule.FileSystem},
                { SourceType.SharePoint, PaidForModule.SharePointOnPrem}
            };

            return dic.ContainsKey(singleSourceType) ? dic[singleSourceType] : PaidForModule.None;

        }

        /// <summary>
        /// Map to single general service error
        /// </summary>
        /// <param name="singleSourceType"></param>
        /// <returns></returns>
        public static ServiceErrors Map2GeneralServerErrors(this SourceType singleSourceType)
        {
            var dic = new Dictionary<SourceType, ServiceErrors> {
                { SourceType.FileSystem, ServiceErrors.FileSystem},
                { SourceType.SharePoint, ServiceErrors.SharePoint}
            };

            return dic.ContainsKey(singleSourceType) ? dic[singleSourceType] : ServiceErrors.None;
        }

        /// <summary>
        /// Map to single license error
        /// </summary>
        /// <param name="singleSourceType"></param>
        /// <returns></returns>
        public static ServiceErrors Map2LicenseServerErrors(this SourceType singleSourceType)
        {
            var dic = new Dictionary<SourceType, ServiceErrors> {
                { SourceType.FileSystem, ServiceErrors.NoFileSystemLicense},
                { SourceType.SharePoint, ServiceErrors.NoSharePointLicense}
            };

            return dic.ContainsKey(singleSourceType) ? dic[singleSourceType] : ServiceErrors.None;
        }

        /// <summary>
        /// Map to all of the service error, including general and license error.
        /// </summary>
        /// <param name="singleSourceType"></param>
        /// <returns></returns>
        public static ServiceErrors Map2Errors(this SourceType singleSourceType)
        {
            var dic = new Dictionary<SourceType, ServiceErrors> {
                { SourceType.FileSystem, ServiceErrors.NoFileSystemLicense | ServiceErrors.FileSystem},
                { SourceType.SharePoint, ServiceErrors.NoSharePointLicense | ServiceErrors.SharePoint}
            };

            return dic.ContainsKey(singleSourceType) ? dic[singleSourceType] : ServiceErrors.None;
        }
    }
}
