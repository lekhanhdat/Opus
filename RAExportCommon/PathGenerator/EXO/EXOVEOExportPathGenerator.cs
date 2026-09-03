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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Backup;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Utility;

namespace RAExportCommon
{

    public class EXOVEOExportPathGenerator : EXOExportPathGeneratorBase
    {



        private string _PhysicalDeviceDtoPath = string.Empty;
        private string _RevIMGlobalSettingColumnName;
        private string mConvertIllegalCharacterTo = "_";
        private const string EXPORTMETADATAFORMAT = "{0}.veo"; //e.g. fileId_Version.Extension

        public EXOVEOExportPathGenerator(String ConvertIllegalCharacterTo, String physicalDeviceDtoPath, string ColumnName)
        {
            _PhysicalDeviceDtoPath = physicalDeviceDtoPath;
            mConvertIllegalCharacterTo = ConvertIllegalCharacterTo;
            _RevIMGlobalSettingColumnName = ColumnName;
        }

        public override EXOExportInfo GeneratEXOFolderExportInfo(EXOFolderPathGeneratorInfo folderInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = folderInfo.JobId;
            info.FolderPath = info.MetaDataFilePath = folderInfo.JobId;
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, folderInfo.EXOFolder.DisplayName);
            info.service = folderInfo.service;
            return info;
        }

        public override EXOExportInfo GenerateEXOItemExportInfo(EXOItemPathGeneratorInfo itemInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = itemInfo.JobId;
            info.FolderPath = info.MetaDataFilePath = itemInfo.JobId;
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            if (string.IsNullOrEmpty(itemInfo.EXOItem.Subject))
            {
                info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, new Guid(HashCodeHelper.ToMD5HashCode(itemInfo.EXOItem.Id.ToString())) + "_");
            }
            else
            {
                info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, new Guid(HashCodeHelper.ToMD5HashCode(itemInfo.EXOItem.Id.ToString())) + "_" + ExchangeUtils.ReplaceSpecicalCharactersToUnderline(itemInfo.EXOItem.Subject));
            }
            info.service = itemInfo.service;
            info.Credentials = itemInfo.Credentials;
            info.MailFullPath = itemInfo.MailFullPath;
            return info;
        }

        public override EXOExportInfo GeneratEXOMailBoxExportInfo(EXOMailBoxPathGeneratorInfo mailBoxInfo)
        {
            EXOExportInfo info = new EXOExportInfo();
            info.JobID = mailBoxInfo.JobId;
            info.FolderPath = info.MetaDataFilePath = mailBoxInfo.JobId;
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, mailBoxInfo.EXOMailbox.Address);
            info.service = mailBoxInfo.service;
            return info;
        }

        public override EXOExportInfoV2 GenerateEXOItemExportInfoV2(EXOItemPathGeneratorInfoV2 itemInfo)
        {
            EXOExportInfoV2 info = new EXOExportInfoV2();
            info.JobID = itemInfo.JobId;
            info.FolderPath = info.MetaDataFilePath = itemInfo.JobId;
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            if (string.IsNullOrEmpty(itemInfo.EXOItem.ItemName))
            {
                info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, new Guid(HashCodeHelper.ToMD5HashCode(itemInfo.EXOItem.ItemId.ToString())) + "_");
            }
            else
            {
                info.ContentFilePath = string.Format(EXPORTMETADATAFORMAT, new Guid(HashCodeHelper.ToMD5HashCode(itemInfo.EXOItem.ItemId.ToString())) + "_" + ExchangeUtils.ReplaceSpecicalCharactersToUnderline(itemInfo.EXOItem.ItemName));
            }
            info.service = itemInfo.service;
            info.Credentials = itemInfo.Credentials;
            info.MailFullPath = itemInfo.MailFullPath;
            return info;
        }
    }
}