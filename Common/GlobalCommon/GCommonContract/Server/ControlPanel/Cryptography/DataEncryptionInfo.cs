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



namespace AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataEncryptionInfo
    {
        //动态生成的加密密钥
        [DataMember]
        public Byte[] EncryptedDynamicKey { get; set; }
        //加密算法
        [DataMember]
        public Int32 EncryptionType { get; set; }
        //动态生成的加密密钥的SHA-1值
        [DataMember]
        public Byte[] Checksum { get; set; }
        //加密Profile(Data Encryption Key）的GUID
        [DataMember]
        public String ProfileGuid { get; set; }
        //加密Profile(Data Encryption Key）的Name，用于丢失Profile后给用户的提示
        [DataMember]
        public String ProfileName { get; set; }
        //具体保护信息的GUID，因为一个Profile可能有多个历史的修改记录。
        [DataMember]
        public String ProtectionGuid { get; set; }
        //具体的加密保护类型，是客户输入的密码，还是证书等
        //[DataMember]
        //public ProtectionAlgorithmType ProtectionAlgorithmType { get; set; }
        //提示信息，用于丢失Profile后的提示
        [DataMember]
        public String PromptMessage { get; set; }
        ////通过全局保护密钥加密后的动态密钥，现在预留。
        //[DataMember]
        //public Byte[] SystemEncryptedKey { get; set; }

        public override String ToString()
        {
            return String.Format("Encryption Type: {0}, Profile Guid: {1}, Profile Name: {2}, Protection Guid: {3}",
                this.EncryptionType,
                this.ProfileGuid,
                this.ProfileName,
                this.ProtectionGuid);
        }
    }
}
