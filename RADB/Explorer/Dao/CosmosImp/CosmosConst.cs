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
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    /// <summary>
    /// Notice: case sensitve!
    /// T: table
    /// A: Alias
    /// C: Column
    /// </summary>
    public class CosmosConst
    {
        /// <summary>
        /// Http response header returned from Cosmos used to check the index update progress
        /// </summary>
        public const string IndexTransformationProgressHeader = "x-ms-documentdb-collection-index-transformation-progress";
        public const string T_Records = "Record";

        public const string A_Records = "r";

        public const string C_LeafName = "leafName";
        public const string C_NodeType = "nodeType";
        public const string C_Id = "id";
        public const string C_DeclareAsRecord = "declareAsRecord";
        public const string C_LockedByRecordLabel = "lockedByRecordLabel";
        public const string C_RecordStatus = "recordStatus";
        public const string C_RecordId = "recordsId";
        public const string C_RecordIdArray = "recordsId_Array";

        public const string C_SourceFlag = "sourceFlag";
        public const string C_HoldBy = "holdBy";
        public const string C_HoldStatus = "holdStatus";
        public const string C_HoldByUsers = "holdByUsers";

        public const string C_TermId = "termId";
        public const string C_TermName = "termName";
        public const string C_ParentId = "parentId";
        public const string C_RuleId = "ruleId";
        public const string C_RecordOwner = "recordOwner";
        public const string C_RecordOwnerArray = "recordOwner_Array";
        public const string C_ManualReviewArray = "manual_reviewer_Array";


        public const string C_ContainerId = "containerId";
        public const string C_ScopePermissionId = "scopePermissionId";
        public const string C_DisposalDueDate = "disposalDueDate";
        public const string C_AveSiteId = "aveSiteId";
        public const string C_TeamsId = "teamsId";

        public const string C_LeafNameArray = "leafName_Array";
        public const string C_ModifiedBy = "modifiedBy";
        public const string C_ModifiedByLower = "modifiedBy_Lower";

        public const string C_ModifiedByArray = "modifiedBy_Array";
        public const string C_TimeModified = "timeModified";
        public const string C_TimeCreated = "timeCreated";
        public const string C_ExtensionForFile = "extensionForFile";

        public const string C_CreatedBy = "createdBy";
        public const string C_CreatedByLower = "createdBy_Lower";
        public const string C_CreatedByArray = "createdBy_Array";
        public const string C_CustomColumnsDic = "customColumnDic";
        public const string C_CustomColumnsValueArray = "Value_Array";
        public const string C_CustomColumnsValue = "Value";
        public const string C_CustomColumnsName = "Name";
        public const string C_CustomColumnsNumber = "Number";
        public const string C_CustomColumnsYesOrNo = "YesOrNo";

        public const string C_CustomColumnsMultiChoice = "MultiChoice";
        public const string C_CustomColumnsDate = "Date";
        public const string C_CustomColumnsUsers = "Users";
        public const string C_CustomColumnsDisplayName = "DisplayName";
        public const string C_CustomColumnsDisplayNameLower = "DisplayName_Lower";
        public const string C_CustomColumnsUPN = "UserPrincipalName";

        public const string C_LocationId = "locationId";
        public const string C_BoxId = "boxId";
        public const string C_FileId = "fileId";
        public const string C_TemplateId = "templateId";

        public const string C_WebId = "webId";
        public const string C_ListId = "listId";
        public const string C_ScopeId = "scopeId";
        public const string C_AncestorArray = "ancestor_Array";

        public const string C_MailboxAddress = "emailaddress";
        public const string C_CollectionDate = "collectTime";

        public const string C_DestryoedTime = "destroyedTime";
        public const string C_DirPath = "dirPath";

        public const string C_LoanPickStatus = "loanPickStatus";
        public const string C_DestructionPickStatus = "destructionPickStatus";
        public const string C_TrainingScope = "training_Scope";
        public const string C_TrainingTermId = "training_TermId";
        public const string C_TrainingAddType = "training_addType";
        public const string C_PredictTime = "ai_predictTime";
        public const string C_PredictTermId = "ai_predictTermId";
        public const string C_MLApprovalStatus = "ai_approvalStatus";

        public const string C_Hidden = "hidden";

    }
}
