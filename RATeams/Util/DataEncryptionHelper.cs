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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.RA.RACommonUtility.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365GroupTeam
{
    public class DataEncryptionHelper
    {
        static readonly AveLogger logger = AveLogger.GetInstance(typeof(DataEncryptionHelper));

        public List<RestoreSecurityInfoWrapper> GetRestoreSecurityInfoList(List<ArchiverSiteMasterIndexContract> indexes)
        {
            List<RestoreSecurityInfoWrapper> restoreSecurityInfos = new List<RestoreSecurityInfoWrapper>();
            indexes.ForEach(index => index.SubInfo.ForEach(item =>
            {
                if (item.DataEncryptionInfo != null)
                {
                    var wrapper = new RestoreSecurityInfoWrapper() { BackupJobId = item.JobId };
                    string key = item.DataEncryptionInfo.ProfileGuid + item.DataEncryptionInfo.ProtectionGuid;

                    wrapper.SecurityInfo = InternalUnWrapperInfoForRestoreJob(item.DataEncryptionInfo);
                    logger.Debug("jobId is {0}, Whether find the job encrypted key or not : {1}.", item.JobId, wrapper.SecurityInfo != null);
                    if (wrapper.SecurityInfo != null)
                    {
                        restoreSecurityInfos.Add(wrapper);
                    }
                }
            }));
            return restoreSecurityInfos;
        }

        private DataEncryptionInfoWrapper InternalUnWrapperInfoForRestoreJob(DataEncryptionInfo info, bool forDA = false)
        {
            if (info != null)
            {
                // 三种情况
                //1.只有一个profileGuid
                //var encryptionInfo = SettingProfileDao.Load();
                var encryptionInfo = DaoService.SettingProfileDao.LoadById(new Guid(info.ProfileGuid));
                if (encryptionInfo == null)
                {
                    logger.Error("encryptionInfo not exit");
                    throw new Exception();
                }
                DataEncryptionProfile profile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);
                if (string.IsNullOrEmpty(info.ProtectionGuid))
                {
                    info.ProtectionGuid = info.ProfileGuid;
                }

                string protectionGuid = string.IsNullOrEmpty(info.ProtectionGuid) ? info.ProfileGuid : info.ProtectionGuid;

                //DataEncryptionProfile profile = DataProfileManager.GetDataEncryptionProfileById(info.ProfileGuid, protectionGuid);
                if (profile != null)
                {
                    if (profile.CurrentProtectionAlgorithm != null && profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                    {
                        DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
                        DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                        infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                        infoFromDB.ProfileGuid = info.ProfileGuid;
                        infoFromDB.ProtectionGuid = info.ProtectionGuid;
                        if (infoFromDB != null)
                        {
                            wrapper.EncryptionInfo = infoFromDB;

                            try
                            {
                                wrapper.DynamicKey = TryUnWraperDynamicKey(infoFromDB, profile);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e.Message, e);
                            }
                            return wrapper;
                        }
                    }
                    else
                    {
                        //2.没有EncryptedDynamickey 说明是老数据，只要返回当前Security profile 内部的key值就可以了
                        if (info.EncryptedDynamicKey == null || info.EncryptedDynamicKey.Length == 0)
                        {
                            return ConvertToWrapper(profile);
                        }
                        else
                        {
                            //3.所有数据都有，需要解密并返回值
                            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
                            DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                            infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                            infoFromDB.ProfileGuid = info.ProfileGuid;
                            infoFromDB.ProtectionGuid = info.ProtectionGuid;
                            if (infoFromDB != null)
                            {
                                wrapper.EncryptionInfo = infoFromDB;

                                try
                                {
                                    if (forDA)
                                    {
                                        wrapper.DynamicKey = TryUnWraperDynamicKeyForDA(infoFromDB, profile);
                                    }
                                    else
                                    {
                                        wrapper.DynamicKey = TryUnWraperDynamicKey(infoFromDB, profile);
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e.Message, e);
                                }
                                return wrapper;
                            }
                        }
                    }
                }

            }
            return null;
        }

        private string TryUnWraperDynamicKeyForDA(DataEncryptionInfo info, DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.KeyVault)
            {
                var aosProfile = profile.CurrentProtectionAlgorithm.AOSProfile ?? PortalUtil.GetSecurityProfileById(profile.CurrentProtectionAlgorithm.AosSecurityProfileId);
                var provider = new KeyVaultServiceProvider(aosProfile);


                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64StringByDefault(provider.DecryptBinary(info.EncryptedDynamicKey));
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
            {
                return Convert.ToBase64String(AesEncryptorWrapper.Decrypt(info.EncryptedDynamicKey));
            }
            else
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64StringByDefault(encryption.DecryptBinary(info.EncryptedDynamicKey));
            }
        }

        protected RMAesEncryptorWrapper AesEncryptorWrapper => new();

        private string TryUnWraperDynamicKey(DataEncryptionInfo info, DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.KeyVault)
            {
                var aosProfile = profile.CurrentProtectionAlgorithm.AOSProfile ?? PortalUtil.GetSecurityProfileById(profile.CurrentProtectionAlgorithm.AosSecurityProfileId);
                var provider = new KeyVaultServiceProvider(aosProfile);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(provider.DecryptBinary(info.EncryptedDynamicKey));
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
            {
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(AesEncryptorWrapper.Decrypt(info.EncryptedDynamicKey));
            }
            else
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(encryption.DecryptBinary(info.EncryptedDynamicKey));
            }
        }

        private DataEncryptionInfo AssemblyDataEncryptionInfo(DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm != null)
            {
                DataEncryptionInfo info = new DataEncryptionInfo();
                //info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                info.EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType;
                info.ProfileGuid = profile.Guid;
                //info.PromptMessage = profile.PromptMessage;
                //info.ProtectionAlgorithmType = profile.CurrentProtectionAlgorithm.Type;
                info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
                info.ProfileName = profile.Name;
                return info;
            }
            return null;
        }

        private DataEncryptionInfoWrapper ConvertToWrapper(DataEncryptionProfile profile)
        {
            if (profile != null)
            {
                var wrapper = AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement.DataEncryptionUtil.CreateDataEncryptionInfoWrapper(profile);
                if (wrapper != null)
                {
                    DataEncryptionInfoWrapper result = new DataEncryptionInfoWrapper();
                    result.DynamicKey = wrapper.DynamicKey;
                    result.EncryptionInfo = wrapper.EncryptionInfo;
                    return result;
                }
            }
            return null;
        }
    }
}
