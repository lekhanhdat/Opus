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

namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{
    public class RecordsAgentUpraderInfo
    {
        public Guid AgentId { get; set; }

        public bool IsMajorUpgrade { get; set; }

        public string CurrentVersion { get; set; }

        public string TargetVersion { get; set; }

        public string ServiceName { get; set; }

        public string ServiceUser { get; set; } = "";

        public string ServicePass { get; set; } = "";

        public string BatFilePath { get; set; }

        public string InstallerPath { get; set; } //The msi or msp installer path

        public string LogFilePath { get; set; } //The installation log file path
    }

    public enum InteralExitCode
    {
        SUCCESS = 0,
        GENERAL_FAILURE = 1,
        INVALID_PARAMETER = 2,
        INVALID_INSTALLER_EXTENTION = 3,
        REAPPLY_SERVICE_ACCOUNT_FAILURE = 4,
        REQUIRE_ADMINISTRATOR = 999,
        SUCCESS_REQUIRE_REBOOT = 3010,
        SUCCESS_REBOOT_INITIATED = 1641,
        PROCESS_TIMEOUT = -998,
        ABSOLUTE_FAILED = -999,
        UNKNOWN_FAILURE = -1000
    }
}
