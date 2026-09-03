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




using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPWebInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPWeb mAveSPWeb = null;

        public AveSPWebInfo(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.WebInfo"))
            {
                var webInfo = GetWebInfo();
                OutputWebInfo(webInfo);
                output.WriteMetadata(AveMetadataType.WebBasicInfo, webInfo);
            }
        }

        private void OutputWebInfo(AveWebInfo webInfo)
        {
            var outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("      [ExportWebBasicInfo]");
            outputBuilder.AppendLine($"     [Url:{webInfo.Url}]");
            outputBuilder.AppendLine($"     [Name:{webInfo.Name}]");
            outputBuilder.AppendLine($"     [Title:{webInfo.Title}]");
            outputBuilder.AppendLine($"     [Description:{webInfo.Description}]");
            outputBuilder.AppendLine($"     [LCID:{webInfo.LCID}]");
            outputBuilder.AppendLine($"     [WebTemplate:{webInfo.WebTemplate}]");
            outputBuilder.AppendLine($"     [OldWebId:{webInfo.OldWebId}]");
            outputBuilder.AppendLine($"     [IsRootWeb:{webInfo.IsRootWeb}]");
            outputBuilder.AppendLine($"     [IsAppWeb:{webInfo.IsAppWeb}]");
            outputBuilder.AppendLine($"     [WorkingLanguage:{webInfo.WorkingLanguage}]");
            outputBuilder.AppendLine($"     [AppInstanceId:{webInfo.AppInstanceId}]");
            outputBuilder.AppendLine($"     [HasUniqueRoleDefinitions:{webInfo.HasUniqueRoleDefinitions}]");
            mLog.Info(outputBuilder.ToString());
        }

        public AveWebInfo GetWebInfo()
        {
            return mAveSPWeb.SPWeb.WebSerializer.GetObjectData() as AveWebInfo;
        }
    }

    public class AveSPWebSettingInfo
    {
        private AveSPWeb mAveSPWeb = null;

        private int mSettingTypes = -1;

        public AveSPWebSettingInfo(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public AveSPWebSettingInfo(AveSPWeb aveSPWeb, int settingTypes)
        {
            mAveSPWeb = aveSPWeb;
            mSettingTypes = settingTypes;
        }

        public AveWebSettingInfo GetWebSettingInfo()
        {
            return GetWebSettingInfo(true);
            //return mAveSPWeb.SPWeb.WebSettingSerializer.GetObjectData();
        }

        public AveWebSettingInfo GetWebSettingInfo(bool backupLookAndFeelSettings)
        {
            mAveSPWeb.SPWeb.WebSettingSerializer.SetLookAndFeelOption(backupLookAndFeelSettings);
            mAveSPWeb.SPWeb.WebSettingSerializer.SetBackupTypes(mSettingTypes);
            return mAveSPWeb.SPWeb.WebSettingSerializer.GetObjectData();
        }

        public void Export(IAveBackupStream output)
        {
            Export(output, true);
            //using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.WebSettingInfo"))
            //{
            //    output.WriteMetadata(AveMetadataType.WebProperty, GetWebSettingInfo());
            //}
        }

        public void Export(IAveBackupStream output, bool keepLookAndFeel)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.WebSettingInfo"))
            {
                output.WriteMetadata(AveMetadataType.WebProperty, GetWebSettingInfo(keepLookAndFeel));
            }
        }
    }
}