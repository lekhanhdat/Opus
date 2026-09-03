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

using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.Wrapper.QueryService
{
    [QueryCommandString(SPDatabaseVersion.SharePoint2016TAP1, QueryCommandType.Insert)]
    internal static class SP2016InsertQueryString
    { 
        #region 存储过程
        public const string RegisterBlob_INSERT_mssqlrbs_rbs_sp_register_blob = @"[mssqlrbs].[rbs_sp_register_blob]";
        public const string AddPool_INSERT_mssqlrbs_rbs_sp_add_pool = @"[mssqlrbs].[rbs_sp_add_pool]";
        public const string AddWebpart_INSERT_proc_AddNonListViewFormWebPartForUrl = @"proc_AddNonListViewFormWebPartForUrl";

        #endregion

        public const string InsertToDocStreams_INSERT_DocStreams = @"if not exists (select DocId from DocStreams where SiteId= @SiteId and DocId= @DocId and Partition = @Partition and BSN = @BSN)
                            begin
           INSERT INTO [dbo].[DocStreams]
           ([DocId]
           ,[SiteId]
           ,[Partition]
           ,[BSN]
           ,[Size]
           ,[Content]
           ,[RbsId]
           ,[Type]
           ,[ExpirationUTC])
     VALUES
           (@DocId
           ,@SiteId
           ,@Partition
           ,@BSN
           ,@Size
           ,null
           ,@RbsId
           ,@Type
           ,null)
            End";

        public const string InsertToDocsToStreams_INSERT_DocsToStreams = @"INSERT INTO [dbo].[DocsToStreams] ([SiteId],[DocId],[HistVersion],[Level],[Partition],[BSN],[StreamId]) VALUES
           (@SiteId,@DocId,@HistVersion,@Level,@Partition,@BSN,@StreamId)";


        public static string InsertIntoAllUserDataJuncations_INSERT_AllUserDataJuncations(AveQueryWorker worker,Guid fieldId, Guid sourceListId, int id, int ordinal)
        {
            string cmdText = @"select tp_SiteId,tp_DeleteTransactionId,tp_IsCurrentVersion,tp_ParentId,
                    tp_DocId,tp_CalculatedVersion,tp_Level,tp_UIVersion from AllUserData WITH(NOLOCK) where tp_ID=@rowId and tp_DocId=@docId and tp_UIVersion=@UIVersion";
            worker.Command.CommandText = cmdText;

            var manager = new AveQueryColumnInfoManager("AllUserDataJunctions");
            manager.LoadColumnsInfo(null, worker.Command);
            manager.ResetColumnValue("tp_FieldId", fieldId);
            manager.ResetColumnValue("tp_SourceListId", sourceListId);
            manager.ResetColumnValue("tp_Id", id);
            manager.ResetColumnValue("tp_Ordinal", ordinal);
            manager.MakeInsertCommand(worker.Command);
            return worker.Command.CommandText;
        }

        #region proc

        public const string InsertWebMoveAndMoveEventReceiver_Insert_proc_InsertEventReceiver = "EXEC proc_InsertEventReceiver @NewId,N'',@SiteId,@WebId,@WebId,1,NULL,NULL,NULL,0,@Type,10000,NULL,@AssemblyFullName,@EventHandlerClassNames,NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL";

        #endregion

        public const string RecycleSite_Insert_SiteDeletion = @"INSERT INTO [SiteDeletion] ([SiteId],[InDeletion],[Restorable],[DeleteIsForMigration]) VALUES (@SiteId,0,0,1)";


    }
}
