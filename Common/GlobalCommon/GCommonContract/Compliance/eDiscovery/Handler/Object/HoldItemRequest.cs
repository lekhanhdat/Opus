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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class HoldItemRequest : EDiscoveryRequest
    {
        [DataMember]
        public HoldItemAction HoldItemAction { get; set; }
        [DataMember]
        public FarmDto Farm { get; set; }
        [DataMember]
        public HoldItemDto HoldItem { get; set; }
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }

        [DataMember]
        public List<HeldFileDto> HeldFiles { get; set; }

        [DataMember]
        public List<SearchResult> SearchResults { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HoldItemAction : uint
    {
        [EnumMember]
        LoadFarms = 1,
        [EnumMember]
        LoadHoldItems = 2,
        [EnumMember]
        AddNewHoldItem = 3,
        [EnumMember]
        CheckManagedBy = 4,
        [EnumMember]
        UpdateHoldItem = 5,
        [EnumMember]
        DeleteHoldItems = 6,
        [EnumMember]
        LoadHeldFiles = 7,
        [EnumMember]
        SearchManagedBy = 8,
        [EnumMember]
        HoldFromHoldManager = 9,
        [EnumMember]
        HoldFromSearchResult = 10,
        [EnumMember]
        ReleaseFromHoldManager = 11,
        [EnumMember]
        LoadFarmNode = 12
    }
}
