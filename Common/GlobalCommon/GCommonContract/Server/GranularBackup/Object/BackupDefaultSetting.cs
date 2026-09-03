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



namespace AvePoint.GCommon.Contract.Server.GranularBackup.Object
{
    #region using directive
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    #endregion

    [XmlRoot(ElementName = "BackupDefaultSetting")]
    public class BackupDefaultSetting
    {
        [XmlAttribute]
        public int CompressionType { get; set; }

        [XmlAttribute]
        public int DataSecurity { get; set; }

        [XmlAttribute]
        public bool FullTextIndex { get; set; }

        [XmlElement]
        public NotificationDto Notification { get; set; }

        [XmlElement]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        [XmlAttribute]
        public string StoragePolicyId { get; set; }

        /// <summary> Represent user choose a filter policy</summary>
        [XmlAttribute]
        public string FilterPolicyId { get; set; }

        [XmlAttribute]
        public PlanCategory Category { get; set; }

        /// <summary> User profile setting </summary>
        [XmlAttribute]
        public bool IncludeUserProfile { get; set; }

        [XmlAttribute]
        public string NotificationProfileId { get; set; }

        [XmlAttribute]
        public string SecurityProfileGuid { get; set; }

        [XmlAttribute]
        public bool IncludeVersions { get; set; }

        [XmlAttribute]
        public bool IncludeListView { get; set; }

        [XmlAttribute]
        public bool EnableMultiThreadInVersionLevel { get; set; }

        [XmlAttribute]
        public bool IsCloseIRMSetting { get; set; }

        [XmlAttribute]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [XmlAttribute]
        public BackupMMSSetting BackupMMSSetting { get; set; }

        [XmlAttribute]
        public bool UseBackupMMSSettingProperty { get; set; }
    }
}
