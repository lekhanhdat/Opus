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
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IExportSettingService
    {
        Task<bool> UploadCoinfigAsync(string voeFilename, Stream veoInput, bool veoIsNoChangeDirectSave, string naaFileName, Stream naaInput, bool naaIsNoChangeDirectSave, string naraFileName, Stream naraInput, bool naraIsNoChangeDirectSave, bool enableExportEncrption,bool enabledDatasum, bool needToUpgradeVEOV3);

        Task<bool> UploadConfigAsyncForGoogleOne(string naraFileName, byte[] naraInput, bool naraIsNoChangeDirectSave, bool enabledDatasum);

        Task<ExportSettingEx> GetSavedFileInfosAsync();

        Task<ExportSettingEx> GetSavedFileInfosAsyncForGoogleOne();
        string GetSavedFileName(out double size, out bool isActive);

        Stream DownloadConfigureFileToStream(out string filename);

        Stream DownloadNAAConfigureFileToStream(out string filename);
        Stream DownloadNARAConfigureFileToStream(out string filename);

        string GetConfigureFileName(ExportSettingType type);
        bool UploadVEOV3Config(string filename, Stream inputStream);
        bool UploadNaraConfig(string filename, Stream inputStream);
        bool UploadNaaConfig(string filename, Stream inputStream);
        string DeleteConfigureFileName();
        string DownloadTemplateZip(string fileName);

        //bool ExportSettignsOnlyChangeActived(bool isActived);

        System.Threading.Tasks.Task MigrateVEOTemplateAsync(byte[] zipConfigContent, string fileName);

        void DeleteMigratedVeoConfig();
        Task<StorageInfoExportSetting> GetStorageInfoInExportSettingsAsync();
        Task<StorageInfoExportSetting> GetGoogleStorageInfoInExportSettingsAsync();
    }
}
