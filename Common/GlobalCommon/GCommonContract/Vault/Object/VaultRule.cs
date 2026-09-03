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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultRule
    {
        /// <summary>
        /// Manager端数据库Id，Agent端不使用
        /// </summary>
        [XmlIgnore]
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Manager端使用，Agent端不使用
        /// </summary>
        [XmlIgnore]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Boolean IsCompression { set; get; }

        [DataMember]
        public Boolean IsEncryption { set; get; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public StoragePolicyDto StoragePolicyDto { get; set; }

        /// <summary>
        /// GUI页面选择的Storage Policy
        /// </summary>
        [DataMember]
        public string StoragePolicyId { get; set; }

        /// <summary>
        /// for GUI display
        /// </summary>
        [DataMember]
        public string StoragePolicyName { get; set; }

        [DataMember]
        public bool GenerateFullTextIndex { set; get; }

        [DataMember]
        public CompressionType CompressionType { get; set; }

        [DataMember]
        public DataSecurity DataSecurity { get; set; }

        [DataMember]
        public EncryptionMethods EncryptionMethods { get; set; }

        /// <summary>
        /// 用于表示enabled状态
        /// </summary>
        private RuleStatus _ruleStatus = RuleStatus.None;

        [DataMember]
        public RuleStatus RuleStatus
        {
            get
            {
                return this._ruleStatus;
            }
            set
            {
                if (value != this._ruleStatus)
                {
                    this._ruleStatus = value;
                    NotifyPropertyChanged("RuleStatus");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }

        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Enabled = 1,
        [EnumMember]
        Disabled = 2
    }
}
