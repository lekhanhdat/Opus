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




namespace AvePoint.GCommon.Contract.GranularBackup.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    #endregion

    /// <summary> Index Job的扩展类 </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRootAttribute(ElementName = "SiteMasterIndexExtension")]
    public class SiteMasterIndexExtension
    {
        /// <summary>
        /// backup module workflow definition.
        ///  
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public Boolean WorkflowDefinition { get; set; }

        /// <summary>
        ///  backup module workflow Instance.
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public Boolean WorkflowInstance { get; set; }

        /// <summary>
        /// Backup与SO结合.
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public BackupSODataType BackupSODataType { get; set; }

        /// <summary>
        /// Backup user profile setting
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public Boolean IncludeUserProfile { get; set; }

        [DataMember]
        [XmlAttribute]
        public Boolean IncludeListView { get; set; }

        [DataMember]
        [XmlAttribute]
        public Boolean IncludeVersion { get; set; }

        [DataMember]
        [XmlAttribute]
        public String CustomActionFilePath { get; set; }

        [DataMember]
        [XmlAttribute]
        public String CustomActionDescription { get; set; }

        /// <summary>
        /// 存放LogicalDevice路径
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public List<String> CustomActionArguments { get; set; }

        public override String ToString()
        {
            return String.Format("Backup SO Data Type: {0}, Custom Action File Path: {1}",
                this.BackupSODataType.ToString(),
                this.CustomActionFilePath);
        }
    }

    /// <summary>Media数据扩展设置.</summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRootAttribute(ElementName = "DataVersionContent")]
    public class DataVersionContentDto
    {
        [DataMember]
        [XmlAttribute("PlatformType")]
        public PlatformType Type { get; set; }

        [DataMember]
        [XmlAttribute("ProductVersion")]
        public ProductVersion Version { get; set; }

        /// <summary>
        /// Reresent last upgrading imported backup data time.
        /// </summary>
        [DataMember]
        public Int64 LastImportedTime { get; set; }

        public override String ToString()
        {
            return String.Format("Type: {0}, Version: {1}",
                this.Type.ToString(),
                this.Version.ToString());
        }
    }
}
