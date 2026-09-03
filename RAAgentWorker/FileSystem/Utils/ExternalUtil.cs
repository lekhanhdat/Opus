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

using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.I18N.Core;
using System;
using System.Globalization;
using System.Text;
using AvePoint.GCommon;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.FileSystem.Utils
{
    public class ExternalUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly IReadOnlyList<string> FR_LANGUAGE = new List<string> { "fr-FR", "fr-CA" };
        public static readonly string MEDIASTORAGE_PROTOCOL = "DOCAVE-XAM://".ToLower(CultureInfo.InvariantCulture);
        public static string CombinePath(string path, string path1, string path2 = "", string path3 = "")
        {
            var p = string.IsNullOrEmpty(path) ? "" : path;
            var p1 = string.IsNullOrEmpty(path1) ? "" : path1;
            var temp = Alphaleonis.Win32.Filesystem.Path.Combine(p, p1.TrimStart('\\'));
            var p2 = string.IsNullOrEmpty(path2) ? "" : path2;
            var p3 = string.IsNullOrEmpty(path3) ? "" : path3;
            var temp1 = Alphaleonis.Win32.Filesystem.Path.Combine(p2.TrimStart('\\'), p3.TrimStart('\\'));

            return Alphaleonis.Win32.Filesystem.Path.Combine(temp, temp1.TrimStart('\\'));
        }
        public static int ComputeDisposalActionFromRule(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return (int)RMContentDisposalAction.LeaveStub;
            }
            else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }

        internal static string ComputeDisposalTimeFromRule(Rule rule)
        {
            //TODO HYW NEED TO DISCUSS HOW TO DEFINE THE TIME
            return String.Empty;
        }
        public static IXSystem OpenXSystem(string path)
        {
            IXSystem _system;
            path = AvePoint.Media.Storage.Util.XRI.ValueEncode(path);
            //var pwd = AvePoint.GCommon.Utility.Cryptography.CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes((encryptedPassword)));
            //AvePoint.GCommon.Utility.Cryptography.CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(encryptedPassword));
            var xri = string.Format(MEDIASTORAGE_PROTOCOL + "fs_vim?location={0}&" + "IS".ToLower(CultureInfo.InvariantCulture) + "validate=false&creation=true", path);
            xri = xri.Replace("creation=true", "creation=false");
            xri = string.Format("{0}&culture={1}", xri, CultureInfo.CurrentUICulture.Name);
            _system = XFactory.InstanceSystem(xri);
            _system.Open();
            return _system;
        }

        internal static System.Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto> FindTop3LevelNodes(FSTreeNodeDto node)
        {
            if (node.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent.Parent == null)
            {
                return new System.Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto>(node.Parent.Parent, node.Parent, node);
            }
            var tempNode = node;
            while (tempNode.Parent.Parent.Parent != null)
            {
                tempNode = tempNode.Parent;
            }
            return new System.Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto>(tempNode.Parent.Parent, tempNode.Parent, tempNode);
        }

        public static string ConvertToFormatSize(long size)
        {
            if (size < 1024)
            {
                return I18NDataSize(string.Format("{0}{1}", size, I18NEntity.GetString("RM_FS_JobReportSizeUnitBytes")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / 1024.0, I18NEntity.GetString("RM_FS_JobReportSizeUnitKB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitMB")));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitGB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("RM_FS_JobReportSizeUnitTB")));
            }
        }

        public static string I18NDataSize(string size)
        {
            if (FR_LANGUAGE.Contains(I18NUtility.curCulture))
            {
                return size.Replace(".", ",");
            }
            return size;
        }

        public static int TransferDataCount
        {
            get
            {
                int count = 100;
                try
                {
                    count = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.TransferDataCount));
                }
                catch (Exception e)
                {
                    count = 100;
                    logger.Warn("An error occurred while getting transfer data count. Error:{0}", e.ToString());
                }
                return count;
            }
        }       

        public static bool CheckEnableFSJPMCFeature(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Equals("ENABLE_JPMC_FILE_SYSTEM_FEATURE", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
