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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
    using DataEncryptionInfoWrapper = GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/3/21",
    "dwxue@avepoint.com",
    "ycyang@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    public class EncryptionInfoManager
        : IEncryptionInfoManager
    {
        public Dictionary<String, DataEncryptionInfo> PutEncryptionInfos(List<RestoreSecurityInfoWrapper> restoreSecurityInfos)
        {
            var result = new Dictionary<String, DataEncryptionInfo>();
            if (restoreSecurityInfos != null)
            {
                restoreSecurityInfos.ForEach(infoWrapper =>
                {
                    DataEncryptionInfoManager.PutEncryptionInfo(infoWrapper.SecurityInfo.EncryptionInfo, infoWrapper.SecurityInfo.DynamicKey);
                    if (result.ContainsKey(infoWrapper.BackupJobId) == false) result.Add(infoWrapper.BackupJobId, infoWrapper.SecurityInfo.EncryptionInfo);
                });
            }
            return result;
        }

        public String PutEncryptionInfo(DataEncryptionInfoWrapper dataEncryptionInfoWrapper)
        {
            var result = default(String);
            if (dataEncryptionInfoWrapper != null)
            {
                DataEncryptionInfoManager.PutEncryptionInfo(dataEncryptionInfoWrapper.EncryptionInfo, dataEncryptionInfoWrapper.DynamicKey);
                result = SerializerHelper.SerializeToBase64StringByDataContractSerializer(dataEncryptionInfoWrapper.EncryptionInfo);
            }
            return result;
        }

        public DataEncryptionInfo GetEncryptionInfo(String dataEncryptionInfo)
        {
            DataEncryptionInfo result = new DataEncryptionInfo();
            if (dataEncryptionInfo != null)
            {
                result = SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(dataEncryptionInfo, typeof(DataEncryptionInfo)) as DataEncryptionInfo;
            }
            return result;
        }
    }
}