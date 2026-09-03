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
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Object
{
    public enum RecordFieldName
    {
        ContainerId,
        TemplateId,
        TermId,
        TermName,
        ScopePermissionId,
        LoanedBy,
        //custom column
        RecordsId, 
        RecordsId_Array,
        RecordStatus,
        ExtensionForFile,
        AveSiteId,
        CreatedBy,
        CreatedBy_Array,
        DirPath,
        FolderId,
        FullPath,
        ItemId,
        ItemRowId,
        LeafName,
        LeafName_Array,
        ListId,
        NodeId,
        NodeType,
        ScopeId,
        SourceFlag,
        TimeCreated,
        TimeModified,
        ParentId,
        SortTicks,
        MetaInfo,
        CollectTime,
    }

    public class RecordFiled2PathUtil
    {
        
        private static Dictionary<RecordFieldName, string> fieldName2PathDic = new Dictionary<RecordFieldName, string>
        {
            { RecordFieldName.ContainerId, CosmosFieldPath.ContainerId},
            { RecordFieldName.TemplateId, CosmosFieldPath.TemplateId},
            { RecordFieldName.TermId, CosmosFieldPath.TermId},
            { RecordFieldName.ScopePermissionId, CosmosFieldPath.ScopePermissionId},
            { RecordFieldName.LeafName, CosmosFieldPath.seprator + "leafName"}, 
            { RecordFieldName.LeafName_Array, CosmosFieldPath.seprator + "leafName_Array"}, 
            { RecordFieldName.RecordsId, CosmosFieldPath.seprator + "recordsId"}, 
            { RecordFieldName.RecordsId_Array, CosmosFieldPath.seprator + "recordsId_Array"}, 
            { RecordFieldName.TermName, CosmosFieldPath.seprator + "termName"}, 
            { RecordFieldName.RecordStatus, CosmosFieldPath.seprator + "recordStatus"},
            { RecordFieldName.ExtensionForFile, CosmosFieldPath.seprator + "extensionForFile"},
            { RecordFieldName.AveSiteId, CosmosFieldPath.seprator + "aveSiteId"},
            { RecordFieldName.CreatedBy, CosmosFieldPath.seprator + "createdBy"},
            { RecordFieldName.CreatedBy_Array, CosmosFieldPath.seprator + "createdBy_Array"},
            { RecordFieldName.DirPath, CosmosFieldPath.seprator + "dirPath"},
            { RecordFieldName.FullPath, CosmosFieldPath.seprator + "fullPath"},
            { RecordFieldName.ItemId, CosmosFieldPath.seprator + "itemId"},
            { RecordFieldName.ItemRowId, CosmosFieldPath.seprator + "itemRowId"},
            { RecordFieldName.ListId, CosmosFieldPath.seprator + "listId"},
            { RecordFieldName.NodeType, CosmosFieldPath.seprator + "nodeType"},
            { RecordFieldName.ScopeId, CosmosFieldPath.seprator + "scopeId"},
            { RecordFieldName.SourceFlag, CosmosFieldPath.seprator + "sourceFlag"},
            { RecordFieldName.TimeCreated, CosmosFieldPath.seprator + "timeCreated"},
            { RecordFieldName.TimeModified, CosmosFieldPath.seprator + "timeModified"},
            { RecordFieldName.ParentId, CosmosFieldPath.seprator + "parentId"},
            { RecordFieldName.SortTicks, CosmosFieldPath.seprator + "sortTicks"},
            { RecordFieldName.MetaInfo, CosmosFieldPath.seprator + "metaInfo"},
            { RecordFieldName.CollectTime, CosmosFieldPath.seprator + "collectTime"},
            //custom column below
            { RecordFieldName.LoanedBy, CosmosFieldPath.LoanedBy},


        };
        public static string GetPath(RecordFieldName fieldName)
        {
            if (!fieldName2PathDic.ContainsKey(fieldName)) throw new ArgumentException("Not supported field name", nameof(fieldName));
            return fieldName2PathDic[fieldName];
        }
    }

    public class CosmosFieldName
    {
        public const string ContainerId = "containerId";
        public const string TemplateId = "templateId";
        public const string TermId = "termId";
        public const string ScopePermissionId = "scopePermissionId";
        public const string CustomColumnDic = "customColumnDic";
    }

    public class CosmosFieldPath
    {
        public const string seprator = "/";
        private const string customColumnSeprator = seprator + CosmosFieldName.CustomColumnDic + seprator;
        public const string ContainerId = seprator+ CosmosFieldName.ContainerId;
        public const string TemplateId = seprator + CosmosFieldName.TemplateId;
        public const string TermId = seprator + CosmosFieldName.TermId;
        public const string ScopePermissionId = seprator + CosmosFieldName.ScopePermissionId;
        //custom column
        public const string LoanedBy = customColumnSeprator + DefaultColumnIDs.LoanedBy;
        //public const string LoanedBy = customColumnSeprator + DefaultColumnIDs.LoanedBy + seprator + "Users";


    }


}
