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
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using global::Media.Common;

    #endregion using directives

    [Serializable]
    public class BackupExtensionInfo
    {
        public Boolean WorkflowDefinition { get; set; }

        public Boolean WorkflowInstance { get; set; }

        public Int32 BackupSODataType { get; set; }

        public Boolean IncludeUserProfile { get; set; }

        public Boolean IncludeListView { get; set; }

        public Boolean IncludeVersion { get; set; }

        public BackupExtensionInfo() { }

        public BackupExtensionInfo(SiteMasterIndexExtension siteMasterIndexExtension)
        {
            this.WorkflowDefinition = siteMasterIndexExtension.WorkflowDefinition;
            this.WorkflowInstance = siteMasterIndexExtension.WorkflowInstance;
            this.BackupSODataType = (Int32)siteMasterIndexExtension.BackupSODataType;
            this.IncludeUserProfile = siteMasterIndexExtension.IncludeUserProfile;
            this.IncludeListView = siteMasterIndexExtension.IncludeListView;
            this.IncludeVersion = siteMasterIndexExtension.IncludeVersion;
        }

        public SiteMasterIndexExtension ToIndexExtention()
        {
            return new SiteMasterIndexExtension
            {
                WorkflowDefinition = this.WorkflowDefinition,
                WorkflowInstance = this.WorkflowInstance,
                BackupSODataType = EnumConverter.ToEnum<BackupSODataType>(this.BackupSODataType.ToString()),
                IncludeUserProfile = this.IncludeUserProfile,
                IncludeListView = this.IncludeListView,
                IncludeVersion = this.IncludeVersion,
            };
        }

        public override string ToString()
        {
            return string.Format("BackupExtensionInfo: {0} WorkflowDefinition : {1} {0}WorkflowInstance : "
                + "{2} {0}BackupSODataType : {3} {0} IncludeUserProfile : {4}{0} IncludeVersion : {5}{0}", Environment.NewLine, this.WorkflowDefinition.ToString(),
                this.WorkflowInstance.ToString(), this.BackupSODataType.ToString(), this.IncludeUserProfile.ToString(), this.IncludeVersion.ToString());
        }
    }
}