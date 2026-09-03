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
using AvePoint.RA.Contract.Schedule;

namespace RAGoogleTests.FilterTests;

public class ValidScheduleSettingUnitTests
{
    
    private static HashSet<ScheduleType> _CommonScheduleTypes = new () {
        ScheduleType.ArchiverDedupJobSchedule,
    };

    private static HashSet<ScheduleType> _SOOnlyScheduleTypes = new() {
        ScheduleType.ArchiveDataRetentionSchedule,
        ScheduleType.ArchiverDeleteRestoredDataSchedule,
        ScheduleType.ApprovalProcessJob
    };

    private static HashSet<ScheduleType> _GoogleScheduleTypes = new()
    {
        ScheduleType.GoogleArchiveJobSchedule,
        ScheduleType.GoogleDataSyncSchedule,
        ScheduleType.GoogleDisposalSchedule,
        ScheduleType.GoogleSettingSchedule
    };

    private static HashSet<ScheduleType> _GoogleOnlyScheduleTypes = new()
    {
        ScheduleType.SyncSchedule,
        ScheduleType.GoogleArchiveJobSchedule,
        ScheduleType.GoogleDataSyncSchedule,
        ScheduleType.GoogleDisposalSchedule,
        ScheduleType.GoogleSettingSchedule,
        ScheduleType.ArchiverDedupJobSchedule,
        ScheduleType.ArchiveDataRetentionSchedule,
    };
    
    [Theory]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, true, false, false, true)]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, true, true, false)]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, true, true, true)]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, false, true, false)]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, false, true, true)]
    [InlineData(ScheduleType.ArchiveDataRetentionSchedule, false, false, true)]
    public void ValidateScheduleSetting(ScheduleType scheduleType,bool hasOpusILLicense, bool hasOpusSOLicense, bool hasOpusGoogleLicense, bool resultShouldBeFail = false)
    {
        bool result = true;
        if (!((hasOpusILLicense || hasOpusGoogleLicense) && hasOpusSOLicense))
        {
            if (hasOpusILLicense)
            {
                if (IsSOOnlyScheduleType(scheduleType))
                {
                    result = false;
                }
            }

            if (hasOpusSOLicense)
            {
                if (!IsSOScheduleType(scheduleType))
                {
                    result = false;
                }
            }
        }

        if (!hasOpusGoogleLicense)
        {
            if (IsGoogleScheduleType(scheduleType))
            {
                result = false;
            }
        }

        if (hasOpusGoogleLicense && !(hasOpusILLicense || hasOpusSOLicense))
        {
            if (!IsGoogleOnlyScheduleType(scheduleType))
            {
                result = false;
            }
        }

        if (resultShouldBeFail)
        {
            Assert.False(result);
        }
        else
        {
            Assert.True(result);
        }
    }
    
    private bool IsSOScheduleType(ScheduleType type)
    {
        return IsCommonScheduleType(type) || IsSOOnlyScheduleType(type);
    }

    private bool IsCommonScheduleType(ScheduleType type)
    {
        return _CommonScheduleTypes.Contains(type);
    }

    private bool IsSOOnlyScheduleType(ScheduleType type)
    { 
        return _SOOnlyScheduleTypes.Contains(type);
    }

    private bool IsGoogleScheduleType(ScheduleType type)
    {
        return _GoogleScheduleTypes.Contains(type);
    }

    private bool IsGoogleOnlyScheduleType(ScheduleType type)
    {
        return _GoogleOnlyScheduleTypes.Contains(type);
    }
}