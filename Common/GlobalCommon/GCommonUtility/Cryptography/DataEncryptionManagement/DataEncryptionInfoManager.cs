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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager;

namespace AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement
{
    public static class DataEncryptionInfoManager
    {
        static Dictionary<string, DataEncryptionInfoWrapper> encryptionInfoTable = new Dictionary<string, DataEncryptionInfoWrapper>();
        static DynamicKeyInfo dynamicKeyInfo = new DynamicKeyInfo();
        static DataEncryptionInfo defaultEncryption;
        static DataEncryptionInfo staticEncryption;
        static DataEncryptionInfo staticBlowfishEncryption;
        static IMSecurityInformationManagerService securityService;
        static DataEncryptionInfo defaultDataEncryptionInfoForUpdate;
        readonly static object tableLock = new object();

        static DataEncryptionInfoManager()
        {

            InitBlowfishProfile();
            InitAesProfile();
        }
        private static void InitAesProfile()
        {
            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
            staticEncryption = new DataEncryptionInfo();
            staticEncryption.ProtectionGuid = "3D5757AA-8C55-4E35-9006-F4163FE75610";
            staticEncryption.EncryptionType = (int)EncryptionAlgorithm.AES_ENCRYPTION;
            byte[] aes = new byte[] { (Byte)201, (Byte)219, (Byte)55, (Byte)183, (Byte)156, (Byte)64, (Byte)85, (Byte)204, (Byte)201, (Byte)219, (Byte)55, (Byte)183, (Byte)156, (Byte)64, (Byte)85, (Byte)204 };
            wrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(aes);
            wrapper.EncryptionInfo = staticEncryption;
            PutEncryptionInfo(wrapper);

        }
        private static void InitBlowfishProfile() {


            DataEncryptionInfoWrapper blowfishWrapper = new DataEncryptionInfoWrapper();
            staticBlowfishEncryption = new DataEncryptionInfo();
            staticBlowfishEncryption.ProtectionGuid = "06523F02-AFB5-42E9-8422-8A14615CDB4B";
            staticBlowfishEncryption.EncryptionType = (int)EncryptionAlgorithm.BLOWFISH_ENCRYPTION;
            byte[] blowfish = new byte[] { (Byte)201, (Byte)219, (Byte)55, (Byte)183, (Byte)156, (Byte)64, (Byte)85, (Byte)204 };
            blowfishWrapper.DynamicKey = CspCommunicationWrapper.WrapKeyToBase64String(blowfish);
            blowfishWrapper.EncryptionInfo = staticBlowfishEncryption;
            PutEncryptionInfo(blowfishWrapper);

        }

        public static DataEncryptionInfo DefaultEncryptionInfo
        {
            get
            {
                return defaultEncryption;

            }
            set
            {
                defaultEncryption = value;

            }

        }


        public static DataEncryptionInfo StaticEncryptionInfo
        {

            get
            {
                return staticEncryption;

            }


        }


        public static DataEncryptionInfo StaticBlowfishEncryptionInfo
        {

            get
            {
                return staticBlowfishEncryption;

            }


        }

        static public DataEncryptionInfoWrapper PutEncryptionInfo(DataEncryptionInfo info, string dynamicKey = null)
        {

            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
            wrapper.DynamicKey = dynamicKey;
            wrapper.EncryptionInfo = info;
            if (wrapper.EncryptionInfo.EncryptedDynamicKey != null && wrapper.EncryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    if (!dynamicKeyInfo.isContainsKey(Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey)))
                    {
                        if (securityService != null)
                        {
                            try
                            {
                                GetDataEncryptionWrapperFromControl(info, false);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else
                        {
                            dynamicKeyInfo.AddDynamicKey(Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey), wrapper);
                        }
                    }
                    wrapper = dynamicKeyInfo.DynamicDic[Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey)];
                    return wrapper;
                }
            }
            else
            {
                lock (tableLock)
                {

                    if (!encryptionInfoTable.ContainsKey(wrapper.EncryptionInfo.ProtectionGuid))
                    {
                        encryptionInfoTable.Add(wrapper.EncryptionInfo.ProtectionGuid, wrapper);

                    }
                    return wrapper;
                }
            }
        }

        static public DataEncryptionInfoWrapper PutEncryptionInfo(DataEncryptionInfoWrapper wrapper)
        {

            if (wrapper.EncryptionInfo.EncryptedDynamicKey != null && wrapper.EncryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    if (!dynamicKeyInfo.isContainsKey(Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey)))
                    {
                        dynamicKeyInfo.AddDynamicKey(Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey), wrapper);
                    }
                    return wrapper;
                }
            }
            else
            {
                lock (tableLock)
                {

                    if (!encryptionInfoTable.ContainsKey(wrapper.EncryptionInfo.ProtectionGuid))
                    {
                        encryptionInfoTable.Add(wrapper.EncryptionInfo.ProtectionGuid, wrapper);

                    }
                    return wrapper;
                }
            }
        }

        static public DataEncryptionInfoWrapper ResolveDynamicKey(DataEncryptionInfo encryptionInfo)
        {
            if (encryptionInfo.EncryptedDynamicKey != null && encryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    DataEncryptionInfoWrapper wrapper = null;
                    if (dynamicKeyInfo.DynamicDic == null || !dynamicKeyInfo.DynamicDic.TryGetValue(Convert.ToBase64String(encryptionInfo.EncryptedDynamicKey), out wrapper))
                    {
                        try
                        {
                            GetDataEncryptionWrapperFromControl(encryptionInfo, false);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                    }

                    dynamicKeyInfo.DynamicDic.TryGetValue(Convert.ToBase64String(encryptionInfo.EncryptedDynamicKey), out wrapper);
                    if (wrapper.DynamicKey == null)
                    {
                        throw new Exception(string.Format("The dynamic key of profile:{0} is empty.", encryptionInfo.ProtectionGuid));
                    }

                    return wrapper;

                }
            }
            else
            {
                lock (tableLock)
                {
                    DataEncryptionInfoWrapper wrapper = null;
                    if (!encryptionInfoTable.TryGetValue(encryptionInfo.ProtectionGuid, out wrapper))
                    {
                        try
                        {
                            GetDataEncryptionWrapperFromControl(encryptionInfo, true);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    wrapper = encryptionInfoTable[encryptionInfo.ProtectionGuid];
                    if (wrapper.DynamicKey == null)
                    {
                        throw new Exception(string.Format("The dynamic key of profile:{0} is empty.", encryptionInfo.ProtectionGuid));
                    }
                    return wrapper;
                }
            }
        }
        

        static private DataEncryptionInfoWrapper GetDataEncryptionWrapperFromControl(DataEncryptionInfo info, bool isById)
        {
            if (info == null)
            {
                return null;
            }
            bool notFound = true;
            AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper profile = null;
            DataEncryptionInfoWrapper wrapperInfo = null;
            if (securityService != null)
            {
                if (isById)
                {
                    profile = securityService.GetDataEncryptionProfileById(info.ProfileGuid, info.ProtectionGuid);
                }
                else
                {
                    profile = securityService.UnWrapperInfoForRestoreJob(info);
                }

                if (profile != null)
                {
                    wrapperInfo = ConvertWrapper(profile);
                    if (isById)
                    {
                        encryptionInfoTable.Add(info.ProtectionGuid, wrapperInfo);
                    }
                    else
                    {
                        dynamicKeyInfo.AddDynamicKey(Convert.ToBase64String(info.EncryptedDynamicKey), wrapperInfo);
                    }
                    notFound = false;
                }
            }
            if (notFound)
            {
                throw new Exception("Can't find encryption information by id:" + info.ProtectionGuid);
            }
            return wrapperInfo;
        }


        static public void InitSecurityService(IMSecurityInformationManagerService service)
        {
            securityService = service;
        }

        static private DataEncryptionInfoWrapper ConvertWrapper(AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper profile)
        {
            if (profile == null)
            {
                return null;
            }
            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
            wrapper.DynamicKey = profile.DynamicKey;
            wrapper.EncryptionInfo = profile.EncryptionInfo;
            return wrapper;
        }

        #region Default Encryption Profile Handle

        public static DataEncryptionInfoWrapper ResolveDefaultEncryptionInfoWrapper()
        {
            if (defaultDataEncryptionInfoForUpdate == null)
            {
                if (securityService != null)
                {
                    AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper profile = securityService.GetDefaultEncryptionInfoWrapper();
                    if (profile != null)
                    {
                        DataEncryptionInfoWrapper wrapperInfo = ConvertWrapper(profile);
                        defaultDataEncryptionInfoForUpdate = profile.EncryptionInfo;
                        lock (tableLock)
                        {
                            if (!encryptionInfoTable.ContainsKey(profile.EncryptionInfo.ProtectionGuid))
                            {
                                encryptionInfoTable.Add(profile.EncryptionInfo.ProtectionGuid, wrapperInfo);
                            }
                        }
                        return wrapperInfo;
                    }
                }
            }
            else
            {
                return ResolveDynamicKey(defaultDataEncryptionInfoForUpdate);
            }

            return null;
        }

        #endregion
    }


    public class DataEncryptionInfoWrapper
    {
        public DataEncryptionInfo EncryptionInfo { get; set; }
        public string DynamicKey { get; set; }

    }

    public class DynamicKeyInfo
    {
        public List<string> DynamicKeyList { get; set; }
        public Dictionary<string, DataEncryptionInfoWrapper> DynamicDic { get; set; }

        public DynamicKeyInfo()
        {
            DynamicKeyList = new List<string>();
            DynamicDic = new Dictionary<string, DataEncryptionInfoWrapper>();
        }

        public void AddDynamicKey(string key, DataEncryptionInfoWrapper val)
        {
            if (this.DynamicKeyList == null || this.DynamicDic == null)
            {
                DynamicKeyList = new List<string>();
                DynamicDic = new Dictionary<string, DataEncryptionInfoWrapper>();
            }
            //else
            //{
            //    if (DynamicKeyList.Count == MAX_DYNA_SIZE)
            //    {
            //        string tempKey = DynamicKeyList[0];
            //        DynamicDic.Remove(tempKey);
            //        DynamicKeyList.RemoveAt(0);
            //    }
            //}
            DynamicKeyList.Add(key);
            DynamicDic.Add(key, val);
        }
        public bool isContainsKey(string key)
        {
            return this.DynamicDic.ContainsKey(key);
        }
    }
}

