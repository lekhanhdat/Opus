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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAManageWebApplicationDefineManagedPathOperation : CAOperation
    {
        [DataMember]
        public String WebAppUrl { get; set; }

        [DataMember]
        public List<WebAppUrlPrefixInfo> WebAppUrlPrefixInfos { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebAppUrlPrefixInfo
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public SharePointPrefixType PrefixType { get; set; }
        public override bool Equals(object obj)
        {
            WebAppUrlPrefixInfo info = obj as WebAppUrlPrefixInfo;
            if (this.Name.Equals(info.Name) && this.PrefixType.Equals(info.PrefixType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode() + this.PrefixType.GetHashCode();
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointPrefixType
    {
        [EnumMember]
        Explicit,
        [EnumMember]
        ExplicitInclusion,
        [EnumMember]
        Wildcard,
        [EnumMember]
        WildcardInclusion,
        [EnumMember]
        Exclusion,        
    }
}
