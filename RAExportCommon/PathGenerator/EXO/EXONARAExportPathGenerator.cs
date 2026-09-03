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
using AvePoint.GCommon;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAExportCommon
{
    public class EXONARAExportPathGenerator : EXOExportPathGeneratorBase
    {
        //private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EXPORTMETADATAFORMAT = "{0}_NARA.CSV";//e.g. itemId_Version.Extension.idx
        private string mConvertIllegalCharacterTo = "_";

        private string _PhysicalDeviceDtoPath = string.Empty;
        private string _RevIMGlobalSettingColumnName;

        public EXONARAExportPathGenerator(String ConvertIllegalCharacterTo, String physicalDeviceDtoPath, string ColumnName)
        {
            _PhysicalDeviceDtoPath = physicalDeviceDtoPath;
            mConvertIllegalCharacterTo = ConvertIllegalCharacterTo;
            _RevIMGlobalSettingColumnName = ColumnName;
        }

        public override EXOExportInfo GeneratEXOMailBoxExportInfo(EXOMailBoxPathGeneratorInfo mailBoxInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = mailBoxInfo.JobId;
            info.FolderPath = Path.Combine(mailBoxInfo.JobId, mailBoxInfo.EXOMailbox.Address);
            info.MetaDataFilePath = mailBoxInfo.JobId;
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, mailBoxInfo.MailAddress);
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Empty;
            info.service = mailBoxInfo.service;
            return info;
        }

        public override EXOExportInfo GeneratEXOFolderExportInfo(EXOFolderPathGeneratorInfo folderInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = folderInfo.JobId;
            info.FolderPath = Path.Combine(folderInfo.JobId, folderInfo.EXOFolder.DisplayName);
            info.MetaDataFilePath = folderInfo.JobId;
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, folderInfo.MailAddress);
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Empty;
            info.service = folderInfo.service;
            return info;
        }

        public override EXOExportInfo GenerateEXOItemExportInfo(EXOItemPathGeneratorInfo itemInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = itemInfo.JobId;
            //info.FolderPath = string.Format("{0}\\{1}\\{2}", itemInfo.JobId, itemInfo.MailAddress, itemInfo.ParentFolderName);
            if (string.IsNullOrEmpty(itemInfo.EXOItem.Subject))
            {
                info.FolderPath = Path.Combine(itemInfo.JobId, itemInfo.MailFullPath);
            }
            else
            {
                info.FolderPath = Path.Combine(itemInfo.JobId, itemInfo.MailFullPath.Substring(0, itemInfo.MailFullPath.LastIndexOf(itemInfo.EXOItem.Subject) - 1));
            }
            info.MetaDataFilePath = itemInfo.JobId;
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, itemInfo.MailAddress);
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = ExchangeUtils.ReplaceSpecicalCharactersToUnderline(itemInfo.EXOItem.Subject) + ".msg";
            info.service = itemInfo.service;
            info.Credentials = itemInfo.Credentials;
            info.MailFullPath = itemInfo.MailFullPath;
            return info;
        }

        public override EXOExportInfoV2 GenerateEXOItemExportInfoV2(EXOItemPathGeneratorInfoV2 itemInfo)
        {
            EXOExportInfoV2 info = new EXOExportInfoV2();
            info.JobID = itemInfo.JobId;
            //info.FolderPath = string.Format("{0}\\{1}\\{2}", itemInfo.JobId, itemInfo.MailAddress, itemInfo.ParentFolderName);
            if (string.IsNullOrEmpty(itemInfo.EXOItem.ItemName))
            {
                info.FolderPath = Path.Combine(itemInfo.JobId, itemInfo.MailFullPath);
            }
            else
            {
                info.FolderPath = Path.Combine(itemInfo.JobId, itemInfo.MailFullPath.Substring(0, itemInfo.MailFullPath.LastIndexOf(itemInfo.EXOItem.ItemName) - 1));
            }
            info.MetaDataFilePath = itemInfo.JobId;
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, itemInfo.MailAddress);
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = ExchangeUtils.ReplaceSpecicalCharactersToUnderline(itemInfo.EXOItem.ItemName) + ".msg";
            info.service = itemInfo.service;
            info.Credentials = itemInfo.Credentials;
            info.MailFullPath = itemInfo.MailFullPath;
            return info;
        }
    }
}
