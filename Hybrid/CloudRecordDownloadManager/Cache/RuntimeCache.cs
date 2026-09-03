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
using System.IO;

namespace CloudRecordDownloadManager.Cache {

    public static class RuntimeCache {

        // public const string InstallFolder = "Cloud Records Agent Service";
        public const string InstallFolder = "AvePoint";
        public static string InstallPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), InstallFolder);
        public static string DownloadPath { get; set; }
        // public static string InstallPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        public static string PackagePath { get; set; }
        public static string AgentInfoConfigPath { get; set; }


        public static bool LicenseAgreed { get; set; } = false;

        /// <summary>
        /// indicate if is major update
        /// </summary>
        public static bool IsMajorUpdate { get; set; } = false;

        public static bool IsMinorUpdate { get; set; } = false;

    }

}