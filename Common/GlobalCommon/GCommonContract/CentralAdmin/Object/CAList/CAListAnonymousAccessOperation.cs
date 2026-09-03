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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListAnonymousAccessOperation : CAOperation
    {
        [DataMember]
        public bool AddItemEnabled { get; set; }

        [DataMember]
        public bool EditItemEnabled { get; set; }

        [DataMember]
        public bool DeleteItemEnabled { get; set; }

        [DataMember]
        public bool ViewItemEnabled { get; set; }

        [DataMember]
        public bool BreakInheritNodes { get; set; }

        [DataMember]
        public AnonymousPermission SelectedAnonymousPerms { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum AnonymousPermission : int
    {
        [EnumMember]
        EmptyMask = 0,
        [EnumMember]
        ViewListItems = 1,
        [EnumMember]
        AddListItems = 2,
        [EnumMember]
        EditListItems = 4,
        [EnumMember]
        DeleteListItems = 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AnonymousEnableStatus : int
    {
        [EnumMember]
        AllDisable = 0,
        [EnumMember]
        ViewItemEnable = 1,
        [EnumMember]
        AddItemsEnable = 2,
        [EnumMember]
        EditItemsEnable = 4,
        [EnumMember]
        DeleteItemsEnable = 8,
        [EnumMember]
        AllEnable = 15,
    }
}
