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
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    //[Flags]
    public enum SPWFProcessorErrorCode
    { 
        Successful=0,
        CannotCreateUniqueIdField=1,
        AssociationNotImplementSQL=101,
        AssociationParentIsNull=102,
        AssociationParentTypeNotSupported=103,
        AssociationSetUnitException = 104,
        AssociationCannotGetDefinitionFiles=105,
        AssociationSubListNotSupported=106,
        AssociationSubListError=107,
        AssociationCustomDataBackupError=108,
        AssociationCustomDataRestoreError = 109,
        AssociationSerializationError=110,
        AssociationDeserializationError=111,
        AssociationBaseIdConflict=112,
        AssociationHandleConflictError=113,
        AssociationDefinitionRestoreError=114,
        AssociationCannotGetWorkflowTemplate=115,
        CannotCreateListField=121,
        FieldProcessorInitializeError=122,
        ContentTypeRestoreError=131,
        SoapServerException=132,
        ValidatingWorkflowException=133,
        AssociatingWorkflowException=134,
        WebServiceOperationNotSupported=135,
        CreateStatusFieldException=136,
        SetCustomProcessorException=137,
        LoadCustomProcessorException=138,
        AssociationRenameError=141,
        AssociationUnknownError=999,





        InstanceNotImplementAPI=1001,
        InstanceParentItemIsNull=1002,
        WebLevelFieldProcessorIsNull=1003,
        GetWorkflowInstanceError=1011,
        InstanceBackupSelfError=1012,
        InstanceBackupTaskError=1013,
        InstanceBackupSubscriptionError=1014,
        InstanceBackupHistoryError=1015,
        InstanceCustomDataBackupError=1016,
        DBFieldToSPFieldError=1017,
        SetPropsFromDataReaderError=1018,
        GetHistoryFieldsError=1019,

        ParentAssociationCannotBeFound=1101,
        GetParentAssociationError=1102,
        InitializeFixupParamError=1103,
        HandleInstaceError=1104,
        InstanceConflict=1105,
        InstanceUnitIsNull=1106,
        InstanceRestoreTaskError=1107,
        InstanceRestoreSubscriptionError=1108,
        InstanceRestoreSelfError=1109,
        InstanceRestoreCustomDataError=1110,
        InstanceDataReplaceError = 1111,
        InstanceDataReplacerInternalError = 1112,
        StatusFieldIsNull = 1113,
        CreateSPListItemError = 1114,
        CreateSPListItemUnknowError = 1115,
        InstanceRestoreHistoryError=1116,
        CannotGetSPEventManagerType=1117,
        CannotGetEventFiringDisabledStatus=1118,
        CannotSetEventFiringDisabledStatus = 1119,
        UpdateItemMetaDataError=1120,
        ReloadParentItemException=1121,
        CreateInstanceArgumentNullException=1122,
        CreateInstanceUnknownException=1123,
        ServiceHandlerNotImplement=1124,

        InstanceUnknownError = 1999,



        PermissionScopeNotSupportedException=2001,
        PermissionParentIsNullException=2002,
        PermissionParentInvalidException=2003,
        PermissionBackupUnknownWarning=2004,
        permissionBackupUnknownException=2005,
        PermissionRestorePrincipalIsNullWarning,
        PermissionRestoreRoleDefinitionIsNullWarning,
        PermissionRestoreCannotGrantLimitedAccessWarning,
        PermissionRestoreUnknownWarning = 2006,
        permissionRestoreUnknownException = 2007,

        PermissionUnitBackupException=2008,
        PermissionUnitRestoreException=2009,


        PutIntoPostAction=9999,
    }


    [Serializable]
    public class SPWFProcessorException:Exception
    {
        public Exception ProcInnerException
        {
            get { return InnerException; }
        }
        
        private int mErrorCode = 0;
        public int ErrorCode
        {
            get { return mErrorCode; }
        }

        private string mErrorCodeString = SPWFProcessorErrorCode.Successful.ToString();
        public string ErrorCodeString
        {
            get { return mErrorCodeString; }
        }

        private Guid mFailedObjectId;
        public Guid FailedObjectId
        {
            get{return mFailedObjectId;}
        }

        private string mInnerMessage;
        public string InnerMessage
        {
            get { return mInnerMessage; }
        }



        public SPWFProcessorException(string message)
            : base(message)
        { }

        public SPWFProcessorException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public SPWFProcessorException(SPWFProcessorErrorCode errorCode)
        {
            mErrorCode = (int)errorCode;
            mErrorCodeString = errorCode.ToString();
        }

        public SPWFProcessorException(SPWFProcessorErrorCode errorCode, Exception innerException)
            : base(errorCode.ToString(), innerException)
        {
            mErrorCode = (int)errorCode;
            mErrorCodeString = errorCode.ToString();
        }

        public SPWFProcessorException(SPWFProcessorErrorCode errorCode,Exception innerException,Guid failedObjectId)
            : base(errorCode.ToString(),innerException)
        {
            mErrorCode = (int)errorCode;
            mErrorCodeString = errorCode.ToString();
            mFailedObjectId = failedObjectId;
        }

        public SPWFProcessorException(SPWFProcessorErrorCode errorCode, Exception innerException, string message)
            : base(message, innerException)
        {
            mErrorCode = (int)errorCode;
            mErrorCodeString = errorCode.ToString();
            mInnerMessage = message;
        }
    }

    public class Logs
    {
        public const string MonitorScope = "MonitorScope";
        public const string MonitorScopeLeave = "MonitorScopeLeave";
        public const string ResourceItemMissing = "ResourceItemMissing";
        

        public const string NoCustomAssociationProc = "NoCustomAssociationProc";
        public const string NoCustomInstanceProc = "NoCustomInstanceProc";
        public const string NoCustomData = "NoCustomData";

        public const string AssociationCustomDataBackupException="AssociationCustomDataBackupException";
        public const string AssociationCustomDataRestoreException="AssociationCustomDataRestoreException";
        public const string InstanceCustomDataBackupException="InstanceCustomDataBackupException";
        public const string InstanceCustomDataRestoreException="InstanceCustomDataRestoreException";

        public const string CT_RestoreUnknownException = "CT_RestoreUnknownException";
        public const string CT_MissingContentTypes = "CT_MissingContentTypes";

        public const string FLD_CollectionInitializeException = "FLD_CollectionInitializeException";
        public const string FLD_FieldAttribute = "FLD_FieldAttribute";
        public const string FLD_CreateFieldException = "FLD_CreateFieldException";
        public const string FLD_ConvertDBFieldException = "FLD_ConvertDBFieldException";
        public const string FLD_ConvertPropsException = "FLD_ConvertPropsException";
        public const string FLD_Property = "FLD_Property";
        public const string FLD_PropertyAttributes = "FLD_PropertyAttributes";

        public const string NintexWorkflow_ConfigDBConnString = "NintexWorkflow_ConfigDBConnString";
        public const string NintexWorkflow_GetConfigDBException = "NintexWorkflow_GetConfigDBException";
        public const string NintexWorkflow_ContentDBConnString = "NintexWorkflow_ContentDBConnString";
        public const string NintexWorkflow_GetContentDBException = "NintexWorkflow_GetContentDBException";
        public const string NintexWorkflow_TemplateLibraryMissing = "NintexWorkflow_TemplateLibraryMissing";
        public const string NintexWorkflow_TemplateFileMissing = "NintexWorkflow_TemplateFileMissing";
        public const string NintexWorkflow_BackupUnknownException = "NintexWorkflow_BackupUnknownException";
        public const string NintexWorkflow_NoBackupData = "NintexWorkflow_NoBackupData";
        public const string NintexWorkflow_SPFileMissing = "NintexWorkflow_SPFileMissing";
        public const string NintexWorkflow_ActionParams = "NintexWorkflow_ActionParams";
        public const string NintexWorkflow_GetActionException = "NintexWorkflow_GetActionException";
        public const string NintexWorkflow_RestoreUnknownException = "NintexWorkflow_RestoreUnknownException";
        public const string NintexWorkflow_IsInstalled = "NintexWorkflow_IsInstalled";
        public const string NintexWorkflow_DBFieldValue = "NintexWorkflow_DBFieldValue";
        public const string NintexWorkflow_NativeBackupException = "NintexWorkflow_NativeBackupException";
        public const string NintexWorkflow_NativeRestoreException = "NintexWorkflow_NativeRestoreException";
        public const string NintexWorkflow_HandleTableException = "NintexWorkflow_HandleTableException";
        public const string NintexWorkflow_UpdateTempFilePropsException = "NintexWorkflow_UpdTempFilePropsException";
        public const string NintexWorkflow_UpdateTempItemPropsException = "NintexWorkflow_UpdTempItemPropsException";
        public const string NintexWorkflow_UpdateTempFilePropsUnknownException = "NintexWorkflow_UpdTempFilePropsUnknownException";

        public const string AP_SetIssueTrackingFieldException = "AP_SetIssueTrackingFieldException";
        public const string AP_StatusFieldName = "AP_StatusFieldName";
        public const string AP_StatusFieldSchema = "AP_StatusFieldSchema";
        public const string AP_GetStatusFieldSchemaException = "AP_GetStatusFieldSchemaException";
        public const string AP_SetAssociationUintPropertiesException = "AP_SetAssociationUintPropertiesException";
        public const string AP_GetAssociationUintPropertiesException = "AP_GetAssociationUintPropertiesException";
        public const string AP_AssociationProperty = "AP_AssociationProperty";
        public const string AP_BackupBegin = "AP_BackupBegin";
        public const string AP_BackupFinish = "AP_BackupFinish";
        public const string AP_BackupSubListFinish = "AP_BackupSubListFinish";
        public const string AP_RestoreBegin = "AP_RestoreBegin";
        public const string AP_RestoreFinish = "AP_RestoreFinish";
        public const string AP_RestoreSubListFinish = "AP_RestoreSubListFinish";
        public const string AP_ConflictStatu = "AP_ConflictStatu";
        public const string AP_CreateStatusFieldException = "AP_CreateStatusFieldException";
        public const string AP_UnitSaveException = "AP_UnitSaveException";
        public const string AP_UnitLoadException = "AP_UnitLoadException";
        public const string AP_GetXomlAndRulesVersionLabelException = "AP_GetXomlAndRulesVersionLabelException";

        public const string IP_GetHistoryFieldException = "IP_GetHistoryFieldException";
        public const string IP_GetItemInstanceException = "IP_GetItemInstanceException";
        public const string IP_InstanceCount = "IP_InstanceCount";
        public const string IP_BackupInstanceException = "IP_BackupInstanceException";
        public const string IP_BackupInstanceSelfException = "IP_BackupInstanceException";
        public const string IP_BackupTaskItemsException = "IP_BackupTaskItemsException";
        public const string IP_BackupEventReceiversException = "IP_BackupEventReceiversException";
        public const string IP_BackupHistoriesException = "IP_BackupHistoriesException";
        public const string IP_BackupCustomUnitsException = "IP_BackupCustomUnitsException";
        public const string IP_BackupPermissionsException = "IP_BackupPermissionsException";
        public const string IP_RestoreParentNotFoundMessage = "IP_RestoreParentNotFoundMessage";
        public const string IP_RestoreParentNotFoundException = "IP_RestoreParentNotFoundException";
        public const string IP_RestoreConflictStatus = "IP_RestoreConflictStatus";
        public const string IP_RestoreInstanceException = "IP_RestoreInstanceException";
        public const string IP_EmptyStatusFieldName = "IP_EmptyStatusFieldName";
        public const string IP_StatusFieldName = "IP_StatusFieldName";
        public const string IP_CreateInstanceException = "IP_CreateInstanceException";
        public const string IP_RestoreInstanceSelfException = "IP_RestoreInstanceException";
        public const string IP_RestoreTaskItemsException = "IP_RestoreTaskItemsException";
        public const string IP_RestoreEventReceiversException = "IP_RestoreEventReceiversException";
        public const string IP_RestoreHistoriesException = "IP_RestoreHistoriesException";
        public const string IP_RestoreCustomUnitsException = "IP_RestoreCustomUnitsException";
        public const string IP_HasInstanceData = "IP_HasInstanceData";
        public const string IP_ReplaceInstanceDataException = "IP_ReplaceInstanceDataException";
        public const string IP_CreateSPItemException = "IP_CreateSPItemException";
        public const string IP_MissingPermissions = "IP_MissingPermissions";
        public const string IP_RestorePermissionsException = "IP_RestorePermissionsException";
        public const string IP_UpdateException = "IP_UpdateException";
        public const string IP_InsertException = "IP_InsertException";
        public const string IP_GetPropertiesFromReaderException = "IP_GetPropertiesFromReaderException";
        public const string IP_ResetStatusFieldException = "IP_ResetStatusFieldException";

        public const string Markup_ProcessTemplateFilesException = "Markup_ProcessTemplateFilesException";
        public const string Markup_APIResultException = "Markup_APIResultException";
        public const string Markup_FoundListByTitle = "Markup_FoundListByTitle";
        public const string Markup_MissingList = "Markup_MissingList";
        public const string Markup_CannotHandleGUID = "Markup_CannotHandleGUID";
        public const string Markup_CannotHandleGUIDUnknown = "Markup_CannotHandleGUIDUnknown";

        public const string FileContentProc_CustomProcAssemblyName = "FileContentProc_CustomProcAssemblyName";
        public const string FileContentProc_FileCharsetName = "FileContentProc_FileCharsetName";


        public const string Common_SPFileCheckInException = "Common_SPFileCheckInException";
        public const string Common_SPFileCheckOutException = "Common_SPFileCheckOutException";

        public const string Common_XmlFileHandleException = "Common_XmlFileHandleException";
    }
}
