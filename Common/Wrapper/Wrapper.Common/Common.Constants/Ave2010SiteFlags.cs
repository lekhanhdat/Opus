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
using System.Text;
//using Microsoft.SharePoint;

namespace AvePoint.Wrapper.Common
{
    public class Ave2010SiteFlags
    {
        public static bool HasSiteScopedUserCustomActions(int Flags)
        {
            return (((AveBitField)0) != ( (AveBitField)Flags & AveBitField.hasSiteScopedUserCustomActions));
        }
        public static bool ReadLocked(int Flags)
        {
            return ( ( (AveBitField)Flags & AveBitField.readLock ) > ((AveBitField)0) );
        }
        public static bool ReadOnly(int Flags)
        {
            return (( (AveBitField)Flags & AveBitField.readOnlyLock) > ((AveBitField)0));
        }
        public static bool ResourceQuotaExceeded(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.resourceMaxLock) > ((AveBitField)0));
        }
        public static bool ResourceQuotaExceededNotificationSent(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.resourceMaxSent) > ((AveBitField)0));
        }
        public static bool ResourceQuotaWarningNotificationSent(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.resourceWarnSent) > ((AveBitField)0));
        }
        public static bool SyndicationEnabled(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.syndicationDisabled) == ((AveBitField)0));
        }
        public static bool TrimAuditLog(int Flags)
        {
            return (( (AveBitField)Flags & AveBitField.trimAuditLog) > ((AveBitField)0));
        }
        public static bool UIVersionConfigurationEnabled(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.uiVersionConfigurationEnabled) > ((AveBitField)0));
        }
        public static bool UserDefinedWorkflowsEnabled(int Flags)
        {
            return (( (AveBitField)Flags & AveBitField.userDefinedWorkflowsDisabled) == ((AveBitField)0));
        }
        public static bool UserSolutionActivated(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.userSolutionActivated) > ((AveBitField)0));
        }
        public static bool WarningNotificationSent(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.diskWarningSent) > ((AveBitField)0));
        }
        public static bool WriteLocked(int Flags)
        {
            return (((AveBitField)Flags & AveBitField.writeLock) > ((AveBitField)0));
        }
    }

    [Flags]
    public enum AveBitField:uint
    {
        bandwidthLock = 0x10,
        bandwidthLockMsgSent = 0x100,
        bandwidthWarningSent = 0x800,
        diskLock = 8,
        diskLockMsgSent = 0x80,
        diskWarningSent = 0x400,
        emailDisabled = 0x4000,
        hasSiteScopedUserCustomActions = 0x1000000,
        Invalid = 0xffffffff,
        mayHaveSiteAlerts = 0x8000,
        nonpaymentLock = 0x20,
        otherLock = 4,
        prescanned = 0x40000,
        readLock = 2,
        readOnlyLock = 0x20000,
        recycleBinDisabled = 0x1000,
        resourceMaxLock = 0x800000,
        resourceMaxSent = 0x400000,
        resourceWarnSent = 0x200000,
        syndicationDisabled = 0x2000,
        tenantAdministrationSite = 0x8000000,
        trimAuditLog = 0x10000,
        uiVersionConfigurationEnabled = 0x2000000,
        userAccountRestriction = 0x80000,
        userDefinedWorkflowsDisabled = 0x10000000,
        userLockMsgSent = 0x200,
        userSolutionActivated = 0x100000,
        violationLock = 0x40,
        writeLock = 1
    }
}
