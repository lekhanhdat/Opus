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
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    public class AveUserAndDomainMappingContractConverter
    {
        public static AveUserAndDomainMappingConvertInfo Converter(UserAndDomainMapping mapping)
        {
            AveUserAndDomainMappingConvertInfo userAndDomainMappingInfo = new AveUserAndDomainMappingConvertInfo();
            userAndDomainMappingInfo.DefaultUser = string.Empty;
            if (mapping != null)
            {
                userAndDomainMappingInfo.UserMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                userAndDomainMappingInfo.DomainMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                userAndDomainMappingInfo.UserPlaceHolderAccount = mapping.placeHolderAccount;
                userAndDomainMappingInfo.DefaultUser = string.IsNullOrEmpty(mapping.destDefaultUser) ? string.Empty : mapping.destDefaultUser;
                if (mapping.UserMappings != null && mapping.UserMappings.UserMapping != null)
                {
                    foreach (UserMapping user in mapping.UserMappings.UserMapping)
                    {
                        if (!userAndDomainMappingInfo.UserMappings.ContainsKey(user.sourceUser))
                        {
                            userAndDomainMappingInfo.UserMappings[user.sourceUser] = user.destinationUser;
                        }
                    }
                }
                if (mapping.DomainMappings != null && mapping.DomainMappings.DomainMapping != null)
                {
                    foreach (DomainMapping domain in mapping.DomainMappings.DomainMapping)
                    {
                        if (!userAndDomainMappingInfo.DomainMappings.ContainsKey(domain.sourceDomain))
                        {
                            userAndDomainMappingInfo.DomainMappings[domain.sourceDomain] = domain.destinationDomain;
                        }
                    }
                }
            }

            return userAndDomainMappingInfo;
        }
    }

    public class AveUserAndDomainMappingConvertInfo
    {
        /// <summary>
        /// User Mapping列表.
        /// </summary>
        public Dictionary<string, string> UserMappings { get; set; }
        /// <summary>
        /// Domain Mapping 列表.
        /// </summary>
        public Dictionary<string, string> DomainMappings { get; set; }
        /// <summary>
        /// Keep User MetaData使用的用户.
        /// </summary>
        public string UserPlaceHolderAccount { get; set; }
        /// <summary>
        /// Default User是哪个.
        /// </summary>
        public string DefaultUser { get; set; }
    }
}
