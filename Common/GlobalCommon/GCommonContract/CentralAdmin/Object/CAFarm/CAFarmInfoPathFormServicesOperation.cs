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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmInfoPathFormServicesOperation : CAOperation
    {
        [DataMember]
        public InfoPathFormServiceActionGetDetail ActionGetDetail { get; set; }

        [DataMember]
        public InfoPathFormServiceActionSetDetail ActionSetDetail { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceActionGetDetail
    {
        [DataMember]
        public InfoPathFormServiceActionGetDetailType ActionGetDetailType { get; set; }

        [DataMember]
        public List<SPFormTemplate> FormTemplates { get; set; }

        [DataMember]
        public Dictionary<String, List<String>> AllSiteCollections { get; set; }

        [DataMember]
        public List<SPDataConnectionFile> DataConnectionFiles { get; set; }

        [DataMember]
        public InfoPathFormServiceConfig FormServiceConfig { get; set; }

        [DataMember]
        public List<InfoPathFormServiceWebServiceProxyConfig> WebServiceProxyConfigDetails { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceActionSetDetail
    {
        [DataMember]
        public InfoPathFormServiceActionSetDetailType ActionSetDetailType { get; set; }

        [DataMember]
        public InfoPathFormServiceActionManageForm ActionManageFormDetail { get; set; }

        [DataMember]
        public InfoPathFormServiceManageDataConnectionFile ManageDataConnectionFileDetail { get; set; }

        [DataMember]
        public InfoPathFormServiceConfig ConfigDetail { get; set; }

        [DataMember]
        public InfoPathFormServiceWebServiceProxyConfig WebServiceProxyConfigDetail { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceManageDataConnectionFile
    {
        [DataMember]
        public ManageDataConnectionFileDetailType ManageDataConnectionFileDetailType { get; set; }

        [DataMember]
        public DataConnectionFileInfo UploadDataConnectionFileInfo { get; set; }

        [DataMember]
        public SPDataConnectionFile EditDataConnectionFileInfo { get; set; }

        [DataMember]
        public SPDataConnectionFile RemoveDataConnectionFileInfo { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceWebServiceProxyConfig
    {
        [DataMember]
        public String WebApplicationUrl { get; set; }

        [DataMember]
        public Boolean IsAllowUserFormWebServiceProxy { get; set; }

        [DataMember]
        public Boolean IsAllowWebServiceProxy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceConfig
    {
        [DataMember]
        public Boolean IsAllowUserFormBrowserEnabling { get; set; }

        [DataMember]
        public Boolean IsAllowUserFormBrowserRendering { get; set; }

        [DataMember]
        public Int32 DefaultDataConnectionTimeout { get; set; }

        [DataMember]
        public Int32 MaxDataConnectionTimeout { get; set; }

        [DataMember]
        public Boolean IsRequireSslForDataConnections { get; set; }

        [DataMember]
        public Int32 MaxDataConnectionResponseSize { get; set; }

        [DataMember]
        public Boolean IsAllowEmbeddedSqlForDataConnections { get; set; }

        [DataMember]
        public Boolean IsAllowUdcAuthenticationForDataConnections { get; set; }

        [DataMember]
        public Boolean IsAllowUserFormCrossDomainDataConnections { get; set; }

        [DataMember]
        public Int32 MaxPostbacksPerSession { get; set; }

        [DataMember]
        public Int32 MaxUserActionsPerPostback { get; set; }

        [DataMember]
        public Int32 ActiveSessionsTimeout { get; set; }

        [DataMember]
        public Int32 MaxSizeOfFormSessionState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormServiceActionManageForm
    {
        [DataMember]
        public ManageFormDetailType ManageFormType { get; set; }

        [DataMember]
        public InfoPathFormInfo UploadFormInfo { get; set; }

        [DataMember]
        public InfoPathEditTemplateInfo EditPropertyTemplateInfo { get; set; }

        [DataMember]
        public InfoPathActivateTemplateInfo ActivateTemplateInfo { get; set; }

        [DataMember]
        public InfoPathDeactivateTemplateInfo DeactivateTemplateInfo { get; set; }

        [DataMember]
        public InfoPathStartQuiesceInfo StartQuiesceInfo { get; set; }

        [DataMember]
        public InfoPathStoptQuiesceInfo StopQuiesceInfo { get; set; }

        [DataMember]
        public InfoPathRemoveTemplateInfo RemoveTemplateInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathRemoveTemplateInfo : InfoPathEditTemplateInfo
    { }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathStartQuiesceInfo
    {

        [DataMember]
        public String FormId { get; set; }

        [DataMember]
        public String TemplateName { get; set; }

        [DataMember]
        public Int32 QuiesceTimeSpanInMinutes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathStoptQuiesceInfo : InfoPathStartQuiesceInfo
    { }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathDeactivateTemplateInfo : InfoPathActivateTemplateInfo
    { }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathActivateTemplateInfo
    {
        [DataMember]
        public String SiteCollectionUrl { get; set; }

        [DataMember]
        public String FormId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathEditTemplateInfo
    {
        [DataMember]
        public String Category { get; set; }

        [DataMember]
        public String Id { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataConnectionFileInfo
    {
        [DataMember]
        public Byte[] DataConnectionFileMetaData { get; set; }

        [DataMember]
        public String FileName { get; set; }

        [DataMember]
        public String Category { get; set; }

        [DataMember]
        public Boolean IsOverwirteFile { get; set; }

        [DataMember]
        public Boolean IsAllowHttpAccess { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InfoPathFormInfo
    {
        [DataMember]
        public Byte[] FormTemplateMetaData { get; set; }

        [DataMember]
        public String FileName { get; set; }

        [DataMember]
        public Boolean IsVerify { get; set; }

        [DataMember]
        public Boolean IsUpgradeExistingTemplate { get; set; }

        [DataMember]
        public Boolean IsDropCurrentSessions { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InfoPathFormServiceActionGetDetailType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        FormTemplates,

        [EnumMember]
        DataConnectionFiles,

        [EnumMember]
        ConfigInfoPathService,

        [EnumMember]
        ConfigInfoPathFormServiceWebServiceProxy
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InfoPathFormServiceActionSetDetailType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        ManageForm,

        [EnumMember]
        ManageDataConnectionFile,

        [EnumMember]
        ConfigInfoPathFormService,

        [EnumMember]
        ConfigInfoPathFormServiceWebServiceProxy,
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ManageFormDetailType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        UploadForm,

        [EnumMember]
        EditProperty,

        [EnumMember]
        Activate,

        [EnumMember]
        Deactivate,

        [EnumMember]
        StartQuiesce,

        [EnumMember]
        StopQuiesce,

        [EnumMember]
        Remove,
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ManageDataConnectionFileDetailType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        UploadUDCFile,

        [EnumMember]
        EditProperty,

        [EnumMember]
        Remove,
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPDataConnectionFile
    {
        [DataMember]
        public String Category { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public String DisplayName { get; set; }

        [DataMember]
        public String Farm { get; set; }

        [DataMember]
        public Boolean HasDependants { get; set; }

        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public SharePointObjectStatus Status { get; set; }

        [DataMember]
        public String TypeName { get; set; }

        [DataMember]
        public Int64 Version { get; set; }

        [DataMember]
        public Boolean WebAccessible { get; set; }

        [DataMember]
        public String Xml { get; set; }

        [DataMember]
        public List<String> ReferenceFormTemplateNames { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPFormTemplate
    {
        [DataMember]
        public String Category { get; set; }

        [DataMember]
        public DateTime CreatedTimeUtc { get; set; }

        [DataMember]
        public List<String> DataConnectionFileReferences { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public String DisplayName { get; set; }

        [DataMember]
        public String FormName { get; set; }

        [DataMember]
        public String Farm { get; set; }

        [DataMember]
        public String FeatureId { get; set; }

        [DataMember]
        public String FormId { get; set; }

        [DataMember]
        public FormTemplateStatus FormTemplateStatus { get; set; }

        [DataMember]
        public bool FullTrust { get; set; }

        [DataMember]
        public DateTime ModifiedTimeUtc { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public QuiesceMode QuiesceStatus { get; set; }

        [DataMember]
        public Boolean Signed { get; set; }

        [DataMember]
        public String SolutionId { get; set; }

        [DataMember]
        public SharePointObjectStatus Status { get; set; }

        [DataMember]
        public String TypeName { get; set; }

        [DataMember]
        public Int64 Version { get; set; }

        [DataMember]
        public Boolean WasCreated { get; set; }

        [DataMember]
        public Boolean WorkflowEnabled { get; set; }

        [DataMember]
        public String PhysicalFileName { get; set; }

        [DataMember]
        public String FormTemplateVersion { get; set; }

        [DataMember]
        public String FormTitle { get; set; }

        [DataMember]
        public DateTime QuiesceEndTimeUtc { get; set; }

        [DataMember]
        public String Id { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FormTemplateStatus
    {
        [EnumMember]
        Converting,

        [EnumMember]
        Error,

        [EnumMember]
        Normal,

        [EnumMember]
        PendingConversion,

        [EnumMember]
        Quiesced,

        [EnumMember]
        Quiescing,

        [EnumMember]
        Removing,

        [EnumMember]
        UploadFailed,

        [EnumMember]
        Uploading,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum QuiesceMode
    {
        [EnumMember]
        Normal,
        [EnumMember]
        Quiesced,
        [EnumMember]
        Quiescing,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointObjectStatus
    {
        [EnumMember]
        Online,
        [EnumMember]
        Disabled,
        [EnumMember]
        Offline,
        [EnumMember]
        Unprovisioning,
        [EnumMember]
        Provisioning,
        [EnumMember]
        Upgrading,
    }
}
