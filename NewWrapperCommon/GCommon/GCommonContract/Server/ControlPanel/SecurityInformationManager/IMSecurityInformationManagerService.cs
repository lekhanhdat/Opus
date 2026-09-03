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
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using System.IO;


namespace AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMSecurityInformationManagerService
    {
        [OperationContract]
        DocAveEncryptionInfo GetDocAveDBEncryptionKeyBackupInfo();

        [OperationContract]
        void SaveDocAveDBEncryptionKeyBackupInfo(DocAveEncryptionInfo backupInfo);

        [OperationContract]
        bool GetDataEncryptionProfileBackupInfo(string filepath);

        [OperationContract]
        Stream LoadDataEncryptionProfileBackupInfo();

        [OperationContract]
        bool SaveDataEncryptionProfileBackupInfo(string filepath);

        [OperationContract]
        void SaveFipsAlgorithmPolicy(bool enabled);

        [OperationContract]
        bool SaveDataEncryptionProfileBackupInfoForAPI(byte[] zipStream);

        [OperationContract]
        List<string> GetAllBackupKeyList();

        [OperationContract]
        void BackupSecurityInfo();

        [OperationContract]
        List<DataEncryptionInfoWrapper> GetAllDataEncryptionAndHistorey();

        [OperationContract]
        List<DataEncryptionInfoWrapper> GetAllCurrentDataEncryption();

        [OperationContract]
        DataEncryptionInfoWrapper GetDataEncryptionProfileById(string profileGUID, string protectionKeyGUID);

        [OperationContract]
        int CheckProtectionKey(string profileGUID, string protectionKeyGUID, string protectionKey);

        [OperationContract]
        DataEncryptionInfoWrapper GetDataEncryptionWrapperByInfo(DataEncryptionInfo info);

        [OperationContract]
        DataEncryptionInfoWrapper GetCurrentSecurityWrapperById(string profileId);

        [OperationContract]
        int ValidateDataEncryptionProfileStatus(DataEncryptionInfo info);

        [OperationContract]
        List<DataEncryptionProfile> GetAllDataEncryptionProfiles();

        [OperationContract]
        bool CheckProtectionKeyByInfo(DataEncryptionInfo encryptionInfo, string protectionKey);

        [OperationContract]
        List<DataEncryptionProfile> GetAllDataEncyrptionProfilesForClient();

        [OperationContract]
        DataEncryptionInfoWrapper GetDefaultEncryptionInfoWrapper();

        [OperationContract]
        string SaveSecurityProfile(DataEncryptionProfile profile);

        [OperationContract]
        string GetSecurityKeyBackupPath();

        /// <summary>
        /// 该方法用来update(或 create) SystemSetting 表中的，控制是否停止check schedule job 的开关
        /// </summary>
        /// <param name="isStop"></param>
        /// <returns></returns>
        [OperationContract]
        string SetScheduleCheckerTaskController(bool isStop);

        #region For Dynamic key
        /// <summary>
        /// 用于当启动job时获取security profile current key的相关信息，当前方法每次调用都会在DataEncryptionInfoWrapper中生成一个Dynamic key用于加密
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [OperationContract]
        DataEncryptionInfoWrapper GetCurrentSecurityProfileForJob(string profileGuid);


        [OperationContract]
        DataEncryptionInfoWrapper GetSecurityProfileForEncryptDynamicKey(string profileGuid, string encryptDynamicKey);

        /// <summary>
        /// 用于批量获取Security profile Current key 相关信息，每次调用都会在每个DataEncryptionInfoWrapper生成一个Dynamic key
        /// </summary>
        /// <param name="profileGuidsList"></param>
        /// <returns></returns>

        [OperationContract]
        List<DataEncryptionInfoWrapper> GetCurrentSecurityProilesForJobs(List<string> profileGuidsList);


        /// <summary>
        /// 当需要解密数据时，需要将保存的DataEncryptionInfo 信息传回，这里会判断info 中是否有EncryptedDynamicKey ，如果有就会读取相应的值进行解密
        /// 如果没有EncryptedDynamickey值，则直接返回Security profile中key的值
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>

        [OperationContract]
        DataEncryptionInfoWrapper UnWrapperInfoForRestoreJob(DataEncryptionInfo info);


        ///// <summary>
        ///// 当需要解密数据时，需要将保存的DataEncryptionInfo 信息传回，这里会判断info 中是否有EncryptedDynamicKey ，如果有就会读取相应的值进行解密
        ///// 如果没有EncryptedDynamickey值，则直接返回Security profile中key的值
        ///// </summary>
        ///// <param name="info"></param>
        ///// <returns></returns>
        //[OperationContract]
        //List<DataEncryptionInfoWrapper> UnWrapperInfosForRestoreJobs(List<DataEncryptionInfo> info);




        #endregion

    }
}
