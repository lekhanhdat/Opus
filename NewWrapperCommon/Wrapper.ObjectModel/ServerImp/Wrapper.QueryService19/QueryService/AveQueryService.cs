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
namespace AvePoint.Wrapper.QueryService
{
    extern alias QueryService16;

    using AvePoint.Wrapper.Common;
    using System;
    using System.Data.SqlClient;
    using static QueryService16.AvePoint.Wrapper.QueryService.SP2016SelectQueryString;
    using static QueryService16.AvePoint.Wrapper.QueryService.SP2016UpdateQueryString;
    using static QueryService16.AvePoint.Wrapper.QueryService.SP2016DeleteQueryString;
    using static QueryService16.AvePoint.Wrapper.QueryService.SP2016InsertQueryString;
    internal class AveQueryService: QueryService16.AvePoint.Wrapper.QueryService.AveQueryService
    {
        protected override AveRBSStubInfo GenerateStubinfo(byte[] tem_blobId, byte[] tem_poolId, long dataLen)
        {
            return new AveRBSStubInfo(tem_blobId, tem_poolId, AveRBSCommon.RBS_PROVIDER_NAME_SP2019, dataLen);
        }

        protected override void AddProviderNameParam()
        {
            mQueryWorker.AddParameter("@ProviderName", AveRBSCommon.RBS_PROVIDER_NAME_SP2019);
        }
        
        public override void CheckConflictInfoForListItem(Guid siteId, Guid listId, RestoringDto restoringDto)
        {
            ResetConflictType(restoringDto);
            this.ExceptionHandlingScope(() =>
            {
                var checker = new ConflictChecker(mQueryWorker);
                restoringDto.ConflictType |= checker.ConflictWithDocumentForListItem(siteId, listId, restoringDto);
                restoringDto.ConflictType |= checker.ConflictWithRecyclebinForListItem(siteId, listId, restoringDto);
            });
        }
        
        public override void CheckConflictInfoForListItem(Guid siteId, Guid listId, string title, RestoringDto restoringDto)
        {
            ResetConflictType(restoringDto);
            this.ExceptionHandlingScope(() =>
            {
                var checker = new ConflictChecker(mQueryWorker);
                restoringDto.ConflictType |= checker.ConflictWithDocumentForListItem(siteId, listId, title, restoringDto);
                restoringDto.ConflictType |= checker.ConflictWithRecyclebinForListItem(siteId, listId, title);
            });
        }
        
        public override void CheckConflictInfo(Guid siteId, Guid parentId, RestoringDto restoringDto)
        {
            ResetConflictType(restoringDto);
            this.ExceptionHandlingScope(() =>
            {
                var checker = new ConflictChecker(mQueryWorker);
                restoringDto.ConflictType |= checker.ConflictWithDocument(siteId, parentId, restoringDto);
                restoringDto.ConflictType |= checker.ConflictWithRecyclebin(siteId, parentId, restoringDto);
            });
        }
        
        public override void CheckConflictInfo(Guid siteId, Guid listId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto)
        {
            ResetConflictType(restoringDto);
            this.ExceptionHandlingScope(() =>
            {
                var checker = new ConflictChecker(mQueryWorker);
                restoringDto.ConflictType |= checker.ConflictWithDocument(siteId, listId, parentId, tp_Guid, restoringDto);
                restoringDto.ConflictType |= checker.ConflictWithRecyclebin(siteId, listId, parentId, tp_Guid);
            });
        }

        public override void CheckConflictInfoBySpecialColumn(Guid siteId, Guid parentId, object columnValue, string fieldColumn, RestoringDto restoringDto)
        {
            ResetConflictType(restoringDto);
            this.ExceptionHandlingScope(() =>
            {
                var checker = new ConflictChecker(mQueryWorker);
                restoringDto.ConflictType |= checker.ConflictWithRecyclebinBySpecialColumn(siteId, parentId, columnValue, fieldColumn, restoringDto);
                restoringDto.ConflictType |= checker.ConflictWithDocumentBySpecialColumn(siteId, parentId, columnValue, fieldColumn, restoringDto);
            });
        }


        class ConflictChecker
        {
            private AveQueryWorker queryWorker;
            public ConflictChecker(AveQueryWorker worker)
            {
                this.queryWorker = worker;
            }

            #region Recyclebin
            internal ConflictType ConflictWithRecyclebinForListItem(Guid siteId, Guid listId, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                byte[] deleteId = new byte[0];
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ListId", listId);
                this.queryWorker.AddParameter("@TP_ID", restoringDto.NameMapping.Substring(0, restoringDto.NameMapping.IndexOf("_", StringComparison.OrdinalIgnoreCase)));
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(SP2019SelectQueryString.GetItemInRecyclebinById_SELECT_AllUserData))
                {
                    if (dr.Read())
                    {
                        deleteId = (byte[])dr[1];
                    }
                }
                conflictType = VerifyInRecycleBin(siteId, deleteId);
                return conflictType;
            }
            
            internal ConflictType ConflictWithRecyclebin(Guid siteId, Guid parentId, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                byte[] deleteId = new byte[0];
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ParentId", parentId);
                this.queryWorker.AddParameter("@LeafName", restoringDto.NameMapping);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(GetItemInRecyclebinByName_SELECT_AllDocs))
                {
                    if (dr.Read())
                    {
                        deleteId = (byte[])dr[0];
                    }
                }
                conflictType = VerifyInRecycleBin(siteId, deleteId);
                return conflictType;
            }

            internal ConflictType ConflictWithRecyclebin(Guid siteId, Guid listId, Guid parentId, Guid tp_Guid)
            {
                var conflictType = ConflictType.None;
                byte[] deleteId = new byte[0];
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@tp_SiteId", siteId);
                this.queryWorker.AddParameter("@tp_ListId", listId);
                this.queryWorker.AddParameter("@tp_ParentId", parentId);
                this.queryWorker.AddParameter("@tp_Guid", tp_Guid);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(GetItemInRecyclebinByTPGUID_SELECT_AllUserData))
                {
                    while (dr.Read())
                    {
                        deleteId = (byte[])dr[0];
                        break;
                    }
                }
                conflictType = VerifyInRecycleBin(siteId, deleteId);
                return conflictType;
            }

            internal ConflictType ConflictWithRecyclebinBySpecialColumn(Guid siteId, Guid parentId, object columnValue, string fieldColumn, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                byte[] deleteId = new byte[0];
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@tp_SiteId", siteId);
                this.queryWorker.AddParameter("@tp_ParentId", parentId);
                this.queryWorker.AddParameter("@ColumnValue", columnValue);
                int rowId = -1;
                int level = -1;
                int uiVersion = 0;
                string cmdText = GetItemInRecyclebinByColumn_SELECT_AllUserData(fieldColumn);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        deleteId = (byte[])dr[0];
                        if (!dr.IsDBNull(1) && !dr.IsDBNull(2))
                        {
                            rowId = dr.GetInt32(1);
                            level = dr.GetByte(2);
                        }
                        uiVersion = dr.GetInt32(3);
                    }
                }
                conflictType = VerifyInRecycleBin(siteId, deleteId);
                if (conflictType == ConflictType.RecycleBin)
                {
                    SetConflictInfo(restoringDto, rowId, level, uiVersion);
                }
                return conflictType;
            }

            internal ConflictType ConflictWithRecyclebinForListItem(Guid siteId, Guid listId, string title)
            {
                var conflictType = ConflictType.None;
                byte[] deleteId = new byte[0];
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ListId", listId);
                this.queryWorker.AddParameter("@title", title);
                // Will handle current look as normal ones, so delete "and nvarchar1!='Current'"
                using (var dr = this.queryWorker.ExecuteReader(SP2019SelectQueryString.GetItemInRecyclebinByTitle_SELECT_AllUserData))
                {
                    while (dr.Read())
                    {
                        deleteId = (byte[])dr[1];
                    }
                }
                conflictType = VerifyInRecycleBin(siteId, deleteId);
                return conflictType;
            }


            private ConflictType VerifyInRecycleBin(Guid siteId, byte[] deleteId)
            {
                ConflictType conflictType = ConflictType.None;
                if (deleteId.Length > 0)
                {
                    this.queryWorker.ClearParameters();
                    this.queryWorker.AddParameter("@SiteId", siteId);
                    this.queryWorker.AddParameter("@DeleteTransactionId", deleteId);
                    using (SqlDataReader dr = this.queryWorker.ExecuteReader(SP2019SelectQueryString.GetItemInRecyclebinByDeleteId_SELECT_RecycleBin))
                    {
                        if (dr.Read())
                        {
                            conflictType = ConflictType.RecycleBin; //conflict with RecycleBin
                        }
                    }
                }

                return conflictType;
            }
            #endregion

            #region Document
            internal ConflictType ConflictWithDocumentForListItem(Guid siteId, Guid listId, string title, RestoringDto restoringDto)
            {
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ListId", listId);
                this.queryWorker.AddParameter("@title", title);

                // Will handle current look as normal ones, so delete "and nvarchar1!='Current'"
                using (var dr = this.queryWorker.ExecuteReader(GetItemByTitle_SELECT_AllUserData))
                {
                    while (dr.Read())
                    {  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
                        return ConflictType.Document;
                    }
                }

                return ConflictType.None;
            }

            internal ConflictType ConflictWithDocumentForListItem(Guid siteId, Guid listId, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ListId", listId);
                this.queryWorker.AddParameter("@TP_ID", restoringDto.NameMapping.Substring(0, restoringDto.NameMapping.IndexOf("_", StringComparison.OrdinalIgnoreCase)));
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(GetItemById_SELECT_AllUserData))
                {
                    while (dr.Read())
                    {
                        conflictType = ConflictType.Document;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
                    }
                }
                return conflictType;
            }

            internal ConflictType ConflictWithDocument(Guid siteId, Guid parentId, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@SiteId", siteId);
                this.queryWorker.AddParameter("@ParentId", parentId);
                this.queryWorker.AddParameter("@LeafName", restoringDto.NameMapping);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(GetItemByName_SELECT_AllDocs))
                {
                    while (dr.Read())
                    {
                        conflictType = ConflictType.Document;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
                    }
                }
                return conflictType;
            }

            internal ConflictType ConflictWithDocument(Guid siteId, Guid listId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@tp_SiteId", siteId);
                this.queryWorker.AddParameter("@tp_ListId", listId);
                this.queryWorker.AddParameter("@tp_ParentId", parentId);
                this.queryWorker.AddParameter("@tp_Guid", tp_Guid);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(GetItemByTPGUID_SELECT_AllUserData))
                {
                    while (dr.Read())
                    {
                        conflictType = ConflictType.Document;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
                    }
                }
                return conflictType;
            }

            internal ConflictType ConflictWithDocumentBySpecialColumn(Guid siteId, Guid parentId, object columnValue, string fieldColumn, RestoringDto restoringDto)
            {
                var conflictType = ConflictType.None;
                this.queryWorker.ClearParameters();
                this.queryWorker.AddParameter("@tp_SiteId", siteId);
                this.queryWorker.AddParameter("@tp_ParentId", parentId);
                this.queryWorker.AddParameter("@ColumnValue", columnValue);

                string cmdText2 = GetItemByColumn_SELECT_AllUserData(fieldColumn);
                using (SqlDataReader dr = this.queryWorker.ExecuteReader(cmdText2))
                {
                    while (dr.Read())
                    {
                        conflictType = ConflictType.Document;
                        SetConflictInfo(restoringDto, dr);
                    }
                }
                return conflictType;
            }
            #endregion

            private void SetConflictInfo(RestoringDto restoringDto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(1) && !dr.IsDBNull(2))
                {
                    int rowId = dr.GetInt32(1);
                    int level = dr.GetByte(2);
                    int uiVersion = dr.GetInt32(3);
                    if (level == 1)
                    {
                        restoringDto.PublishingUIVersion = uiVersion;
                    }
                    else if (level == 2)
                    {
                        restoringDto.DraftUIVersion = uiVersion;
                    }
                    restoringDto.ConflictRowId = rowId;
                }
                else
                {
                    restoringDto.PublishingUIVersion = dr.GetInt32(3);
                }
            }

            private void SetConflictInfo(RestoringDto restoringDto, int rowId, int level, int uiVersion)
            {
                if (rowId != -1 && level != -1)
                {
                    if (level == 1)
                    {
                        restoringDto.PublishingUIVersion = uiVersion;
                    }
                    else if (level == 2)
                    {
                        restoringDto.DraftUIVersion = uiVersion;
                    }
                    restoringDto.ConflictRowId = rowId;
                }
                else
                {
                    restoringDto.PublishingUIVersion = uiVersion;
                }
            }
        }
    }

    public class AveQueryServiceProvider
    {
        public static T Instance<T>(object arg) where T : IAveQueryService
        {
            return (T)CreateQueryService(arg);
        }
        internal static IAveQueryService CreateQueryService(object arg)
        {
            var queryService = new AveQueryService();
            queryService.InitQuerySession(arg);
            return queryService;
        }
    }
}
