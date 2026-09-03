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


using Microsoft.Win32;

namespace AutoInstallation.Contract
{
    public class SoftWareRegeditInfo
    {
        public string RegistryKeyPath { get; set; } = string.Empty;

        public RegeditKey DisplayIcon { get; set; } = new RegeditKey {Name = "DisplayIcon"};

        public RegeditKey DisplayName { get; set; } = new RegeditKey {Name = "DisplayName"};

        public RegeditKey DisplayVersion { get; set; } = new RegeditKey {Name = "DisplayVersion"};

        public RegeditKey EstimatedSize { get; set; } =
            new RegeditKey {Name = "EstimatedSize", ValueKind = RegistryValueKind.DWord, Value = 0};

        public RegeditKey HelpLink { get; set; } = new RegeditKey {Name = "HelpLink"};

        public RegeditKey InstallDate { get; set; } = new RegeditKey {Name = "InstallDate"};

        public RegeditKey InstalledLocation { get; set; } = new RegeditKey {Name = "InstalledLocation"};

        public RegeditKey InstallSource { get; set; } = new RegeditKey {Name = "InstallSource"};

        public RegeditKey Publisher { get; set; } = new RegeditKey {Name = "Publisher"};

        public RegeditKey UninstallString { get; set; } = new RegeditKey {Name = "UninstallString"};

        public RegeditKey URLInfoAbout { get; set; } = new RegeditKey {Name = "URLInfoAbout"};

        public RegeditKey URLUpdateInfo { get; set; } = new RegeditKey {Name = "URLUpdateInfo"};
    }
}