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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.Adonis.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccessListDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public SPTreeNodeDto Farm { get; set; }

        [DataMember]
        public List<AdministratorProfileInfo> Profiles { get; set; }

        [DataMember]
        public List<DefinedGroupInfo> DefinedGroupInfos { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccessListForDisplay
    {
        [DataMember]
        public List<AccessListDto> accessList { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> accessListAndProfiles { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> profileAndUsedRules { get; set; }

        [DataMember]
        public List<NameAndIdDto> allUsedRules { get; set; }

        [DataMember]
        public List<NameAndIdDto> allUsedProfiles { get; set; }
    }
}
