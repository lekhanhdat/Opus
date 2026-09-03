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






using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public PermissionScope Scope { get; set; }
        [DataMember]
        public FarmDto Farm { get; set; }
        [DataMember]
        public bool EnableSecurityTrimming { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public List<PermissionLevelDto> PermissionLevels { get; set; }

        public override string ToString()
        {
            if (PermissionLevels != null && PermissionLevels.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (PermissionLevelDto dto in PermissionLevels)
                {
                    sb.Append( "[" + Scope.ToString() + "] " + dto.Name);
                }
                return sb.ToString();
            }
            else
            {
                return "None";
            }
        }
    }
}
