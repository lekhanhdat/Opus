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
using AvePoint.Common.Portal;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.Util
{
    public class RMArchivedFullTextIndexSecurityUtil
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexSecurityUtil));

        private static readonly ISettingProfilesDao s_settingProfileDao = PlatformWindsorManager.GetService<ISettingProfilesDao>();

        public static DataEncryptionInfo GetEncryptionInfo(ArchiverIndexSubInfoContract jobInfo)
        {
            s_logger.Info($"Start get encryption info of index sub job [{jobInfo.Id}].");

            var encryptionInfo = jobInfo.DataEncryptionInfo;
            if(encryptionInfo == null)
            {
                s_logger.Warn($"The job [{jobInfo.Id}] did not found encryption info.");
                return null;
            }

            var profileInfo = s_settingProfileDao.LoadById(new Guid(encryptionInfo.ProfileGuid));
            var profile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(profileInfo.Settings);

            if (profile == null)
            {
                s_logger.Info($"The index sub job [{jobInfo.Id}] no security profile found.");
                return null;
            }

            var wrapper = new GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();

            if (!(profile.CurrentProtectionAlgorithm != null && profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                && (encryptionInfo.EncryptedDynamicKey == null || encryptionInfo.EncryptedDynamicKey.Length == 0))
            {
                var daoWrapper = DataEncryptionUtil.CreateDataEncryptionInfoWrapper(profile);
                if (wrapper != null)
                {
                    wrapper.DynamicKey = daoWrapper.DynamicKey;
                    wrapper.EncryptionInfo = daoWrapper.EncryptionInfo;
                }
            }

            var infoFromDB = new DataEncryptionInfo
            {
                EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType,
                ProfileName = profile.Name,
                EncryptedDynamicKey = encryptionInfo.EncryptedDynamicKey,
                ProfileGuid = encryptionInfo.ProfileGuid,
                ProtectionGuid = encryptionInfo.ProfileGuid,
            };
            wrapper.EncryptionInfo = infoFromDB;
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.KeyVault)
            {
                var aosProfile = profile.CurrentProtectionAlgorithm.AOSProfile ?? PortalUtil.GetSecurityProfileById(profile.CurrentProtectionAlgorithm.AosSecurityProfileId);
                var provider = new KeyVaultServiceProvider(aosProfile);
                wrapper.DynamicKey = GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(provider.DecryptBinary(encryptionInfo.EncryptedDynamicKey));
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
            {
                wrapper.DynamicKey = GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(new RMAesEncryptorWrapper().Decrypt(encryptionInfo.EncryptedDynamicKey));
            }
            else
            {
                var encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                wrapper.DynamicKey = GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(encryption.DecryptBinary(encryptionInfo.EncryptedDynamicKey));
            }

            DataEncryptionInfoManager.PutEncryptionInfo(wrapper.EncryptionInfo, wrapper.DynamicKey);
            s_logger.Info($"End get encryption info of index sub job [{jobInfo.Id}].");

            return wrapper.EncryptionInfo;

        }
    }
}
