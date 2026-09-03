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

using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.Adonis.Replicator.Contract.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EventReceiverType
    {
        [EnumMember]
        ContextEvent = 0x7ffe,
        [EnumMember]
        EmailReceived = 0x4e20,
        [EnumMember]
        FieldAdded = 0x2775,
        [EnumMember]
        FieldAdding = 0x65,
        [EnumMember]
        FieldDeleted = 0x2777,
        [EnumMember]
        FieldDeleting = 0x67,
        [EnumMember]
        FieldUpdated = 0x2776,
        [EnumMember]
        FieldUpdating = 0x66,
        [EnumMember]
        InvalidReceiver = -1,
        [EnumMember]
        ItemAdded = 0x2711,
        [EnumMember]
        ItemAdding = 1,
        [EnumMember]
        ItemAttachmentAdded = 0x2717,
        [EnumMember]
        ItemAttachmentAdding = 7,
        [EnumMember]
        ItemAttachmentDeleted = 0x2718,
        [EnumMember]
        ItemAttachmentDeleting = 8,
        [EnumMember]
        ItemCheckedIn = 0x2714,
        [EnumMember]
        ItemCheckedOut = 0x2715,
        [EnumMember]
        ItemCheckingIn = 4,
        [EnumMember]
        ItemCheckingOut = 5,
        [EnumMember]
        ItemDeleted = 0x2713,
        [EnumMember]
        ItemDeleting = 3,
        [EnumMember]
        ItemFileConverted = 0x271a,
        [EnumMember]
        ItemFileMoved = 0x2719,
        [EnumMember]
        ItemFileMoving = 9,
        [EnumMember]
        ItemUncheckedOut = 0x2716,
        [EnumMember]
        ItemUncheckingOut = 6,
        [EnumMember]
        ItemUpdated = 0x2712,
        [EnumMember]
        ItemUpdating = 2,
        [EnumMember]
        ItemVersionDeleted = 0x271b,
        [EnumMember]
        ListAdded = 0x2778,
        [EnumMember]
        ListAdding = 0x68,
        [EnumMember]
        ListDeleted = 0x2779,
        [EnumMember]
        ListDeleting = 0x69,
        [EnumMember]
        SiteDeleted = 0x27d9,
        [EnumMember]
        SiteDeleting = 0xc9,
        [EnumMember]
        WebAdding = 0xcc,
        [EnumMember]
        WebDeleted = 0x27da,
        [EnumMember]
        WebDeleting = 0xca,
        [EnumMember]
        WebMoved = 0x27db,
        [EnumMember]
        WebMoving = 0xcb,
        [EnumMember]
        WebProvisioned = 0x27dc,
        [EnumMember]
        WorkflowCompleted = 0x2907,
        [EnumMember]
        WorkflowPostponed = 0x2906,
        [EnumMember]
        WorkflowStarted = 0x2905,
        [EnumMember]
        WorkflowStarting = 0x1f5,
        [EnumMember]
        GroupAdded = 0x283d,
        [EnumMember]
        GroupAdding = 0x12d,
        [EnumMember]
        GroupDeleted = 0x283f,
        [EnumMember]
        GroupDeleting = 0x12f,
        [EnumMember]
        GroupUpdated = 0x283e,
        [EnumMember]
        GroupUpdating = 0x12e,
        [EnumMember]
        GroupUserAdded = 0x2840,
        [EnumMember]
        GroupUserAdding = 0x130,
        [EnumMember]
        GroupUserDeleted = 0x2841,
        [EnumMember]
        GroupUserDeleting = 0x131,
        [EnumMember]
        InheritanceBreaking = 0x137,
        [EnumMember]
        InheritanceBroken = 0x2847,
        [EnumMember]
        InheritanceReset = 0x2848,
        [EnumMember]
        InheritanceResetting = 0x138,
        [EnumMember]
        RoleAssignmentAdded = 0x2845,
        [EnumMember]
        RoleAssignmentAdding = 0x135,
        [EnumMember]
        RoleAssignmentDeleted = 0x2846,
        [EnumMember]
        RoleAssignmentDeleting = 0x136,
        [EnumMember]
        RoleDefinitionAdded = 0x2842,
        [EnumMember]
        RoleDefinitionAdding = 0x132,
        [EnumMember]
        RoleDefinitionDeleted = 0x2844,
        [EnumMember]
        RoleDefinitionDeleting = 0x134,
        [EnumMember]
        RoleDefinitionUpdated = 0x2843,
        [EnumMember]
        RoleDefinitionUpdating = 0x133
    }
}
