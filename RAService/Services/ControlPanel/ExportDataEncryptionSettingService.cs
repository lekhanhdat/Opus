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
using AvePoint.RA.Contract.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.Common;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services;

namespace AvePoint.RA.Service.ControlPanel
{
    [Audit]
    public class ExportDataEncryptionSettingService : RMServiceBase, IExportDataEncryptionSettingService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExportSettingService));
        private readonly string ExportDataEncryptionKey = $"{KeyNameCollection.ExportDataEncryptionEnabled}{RMNameValueDto.Seprator}{RMNameValueType.ExportDataEncryption}";

        public IExportDataEncryptionSettingDao ExportDataEncryptionSettingDao => PlatformWindsorManager.GetService<IExportDataEncryptionSettingDao>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private RMAesEncryptorWrapper AesEncryptorWrapper => new();

        public RAReturnMessage GetCurrentAesKey()
        {
            RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            string AesKeyString = string.Empty;
            try
            {
                var setting = ExportDataEncryptionSettingDao.GetExportDataEncryptionSetting();
                if (setting != null)
                {
                    if (setting.AesKey != null && setting.IV != null)
                    {
                        setting.AesKey = AesEncryptorWrapper.CompatibleDecrypt(setting.AesKey);
                        setting.IV = AesEncryptorWrapper.CompatibleDecrypt(setting.IV);
                        AesKeyString = setting.AesKey + "|" + setting.IV;
                    }
                }
                message.Extension = AesKeyString;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while getting export encryption key. Error:{e.ToString()}");
                message.MessageType = RAMessageType.Failed;
            }
            return message;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.GenerateExportEncryptionKey, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public RAReturnMessage GenerateAesKey()
        {
            RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                RMExportDataEncryptionSetting exportDataEncryptionSetting = new RMExportDataEncryptionSetting();
                exportDataEncryptionSetting.IsCurrent = true;
                var keyString = KeyGenerator.Create(32);
                var ivString = KeyGenerator.Create(16);
                exportDataEncryptionSetting.AesKey = AesEncryptorWrapper.Encrypt(keyString);
                exportDataEncryptionSetting.IV = AesEncryptorWrapper.Encrypt(ivString);
                ExportDataEncryptionSettingDao.Save(exportDataEncryptionSetting);
                message.Extension = keyString + "|" + ivString;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while generating export encryption key. Error:{e.ToString()}");
                message.MessageType = RAMessageType.Failed;
            }
            return message;
        }

        public Task<bool> EnableExportDataEncryptionAsync()
        {
            //string aesKey = string.Empty;
            //var setting = ExportDataEncryptionSettingDao.GetExportDataEncryptionSetting();
            //if (setting == null)
            //{
            //    aesKey = GenerateAesKey().Extension;
            //}
            //else
            //{
            //    RsaHelper helper = new RsaHelper(GCommonRoleConfiguration.RECO_Certificate);
            //    setting.AesKey = helper.Decrypt(setting.AesKey);
            //    setting.IV = helper.Decrypt(setting.IV);
            //    aesKey = setting.AesKey + "|" + setting.IV;
            //}

            return RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = ExportDataEncryptionKey, Value = "True" });
        }
                
        public async Task<bool> DisableExportDataEncryptionAsync()
        {           
            var result = RMKeyValueDao.GetValueByKey(ExportDataEncryptionKey);
            if (result != null)
            {
                result.Value = "False";
                return await RMKeyValueDao.UpdateAsync(result);
            }
            return true;
        }
    }
}
