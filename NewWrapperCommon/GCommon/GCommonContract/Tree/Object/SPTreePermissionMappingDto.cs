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
namespace AvePoint.GCommon.Contract.Tree.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPTreePermissionMappingDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public SPTreePermission Permission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPTreePermission
    {
        [DataMember]
        public bool IsSiteCollectionAdmin { get; set; }

        [DataMember]
        public long GrantMask { get; set; }

        [DataMember]
        public long DenyMask { get; set; }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}", IsSiteCollectionAdmin, GrantMask, DenyMask);
        }

        public static SPTreePermission Parse(string permission)
        {
            var arr = permission.Split('|');
            if (arr.Length >= 3)
            {
                return new SPTreePermission
                {
                    IsSiteCollectionAdmin = bool.Parse(arr[0]),
                    GrantMask = long.Parse(arr[1]),
                    DenyMask = long.Parse(arr[2]),
                };
            }
            if (arr.Length == 2)//兼容旧数据
            {
                return new SPTreePermission
                {
                    IsSiteCollectionAdmin = bool.Parse(arr[0]),
                    GrantMask = long.Parse(arr[1]),
                    DenyMask = 0,
                };
            }
            return null;
        }
    }
}
