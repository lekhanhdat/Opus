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
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOTreeNode : SPTreeNodeDto
    {
        [DataMember]
        public string StubId { set; get; }

        [DataMember]
        public string StubName { set; get; }

        [DataMember]
        public StubType StubType { set; get; }

        [DataMember]
        public string StubSize { set; get; }

        [DataMember]
        public bool IsChecked { set; get; }
        /// <summary>
        /// 用于页面显示和提示判断, 不存到DB
        /// </summary>
        [DataMember]
        public bool HasNecessaryConfig { set; get; }

        #region End user archiver setting
        [DataMember]
        public EndUserArchiverSetting EndUserArchiverSetting { set; get; }
        //不需要EncryptionType， 加密信息也在DataSecurity中
        #endregion

    }

    [DataContract]
    public class EndUserArchiverSetting
    {
        /// <summary>
        /// SharePoint end user archiver feature status
        /// </summary>
        [DataMember]
        public EndUserFeatureStatus EndUserFeatureStatus { get; set; }
        /// <summary>
        /// SharePoint end user archiver solution status
        /// </summary>
        [DataMember]
        public EndUserSolutionStatus EndUserSolutionStatus { get; set; }
        /// <summary>
        /// 用于GUI页面判断是否显示警告的图标使用
        /// </summary>
        [DataMember]
        public bool IsSolutionDeployed { get; set; }
        /// <summary>
        /// Storage policy for display on UI
        /// </summary>
        [DataMember]
        public string StoragePolicyName { get; set; }
        /// <summary>
        /// Index device for display on UI
        /// </summary>
        [XmlIgnoreAttribute]
        [DataMember]
        public string IndexDeviceName { get; set; }
        /// <summary>
        /// Storage policy for data in xml
        /// </summary>
        [DataMember]
        public string StoragePolicyId { set; get; }
        /// <summary>
        /// Compreesion level
        /// </summary>
        [DataMember]
        public CompressionType ArchiverCompressionType { get; set; }
        /// <summary>
        /// Compression method
        /// </summary>
        [DataMember]
        public DataSecurity ArchiverDataSecurity { get; set; }

        [DataMember]
        public String DataEncryptionProfileId { get; set; }

        /// <summary>
        /// End User Archiver Tag
        /// </summary>
        [DataMember]
        public List<TagMaping> ArchiverTagMapings { get; set; }

        /// <summary>
        /// Workflow Status
        /// </summary>
        [DataMember]
        public BackupRestoreWorkflow WorkFlowStatus { get; set; }

        [DataMember]
        public bool UseSnapLock { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PermissionInfo PermissionInfo { get; set; }

        /// <summary>
        /// 重写toString, 方便打印log
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("EndUserFeatureStatus : ").Append(this.EndUserFeatureStatus).Append("\t");
            sb.Append("ArchiverCompressionType : ").Append(this.ArchiverCompressionType).Append("\t");
            sb.Append("ArchiverDataSecurity : ").Append(this.ArchiverDataSecurity).Append("\t");
            sb.Append("StoragePolicy : ").Append(StoragePolicyName);
            return sb.ToString();
        }
    }

    [DataContract]
    public class TagMaping
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public TestResult TestResult { get; set; }

        [DataMember]
        public TestFailedType TestFailedType { get; set; }
    }

    [DataContract]
    public enum TestFailedType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ColumnNotExist = 1,
        [EnumMember]
        ListNotExist = 2,
        [EnumMember]
        UserNoPermission = 3
    }

    [DataContract]
    public enum TestResult
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Successful = 1,

        [EnumMember]
        Error = 2
    }

    [DataContract]
    public enum StubType
    {
        [EnumMember]
        Realtime = 0,

        [EnumMember]
        Scheduled = 1,

        [EnumMember]
        Connector = 2,

        [EnumMember]
        Unknown = 3,

        [EnumMember]
        ThirdParty = 4,

        [EnumMember]
        LimitedFile = 5,

        [EnumMember]
        ConnectorLimitedFile = 6
    }


    [DataContract]
    public enum EndUserFeatureStatus
    {
        [EnumMember]
        Deactive = 0,
        [EnumMember]
        Active = 1
    }

    [DataContract]
    public enum EndUserSolutionStatus
    {
        [EnumMember]
        Undeployed = 0,
        [EnumMember]
        Deployed = 1
    }
}
