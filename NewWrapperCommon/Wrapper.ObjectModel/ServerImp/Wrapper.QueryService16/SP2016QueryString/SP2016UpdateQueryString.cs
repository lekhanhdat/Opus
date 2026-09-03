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
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace AvePoint.Wrapper.QueryService
{
    [QueryCommandString(SPDatabaseVersion.SharePoint2016TAP1, QueryCommandType.Update)]
    internal static class SP2016UpdateQueryString
    {
        public const string SetNextId_UPDATE_dbo_proc_SetNextId = @"[dbo].[proc_SetNextId]";

        public const string UpdateSiteUsage_UPDATE_proc_QMChangeSiteDiskUsedAndContentTimestamp = "proc_QMChangeSiteDiskUsedAndContentTimestamp";

        public const string SetInternalPool_UPDATE_mssqlrbs_resources_rbs_internal_pools = @"UPDATE [mssqlrbs_resources].[rbs_internal_pools] 
                SET [can_store_new_blobs]=@CanStoreNewBlobs,[close_time]=@CloseTime 
                WHERE [blob_store_id]=@BlobStoreId AND [store_pool_id]=@StorePoolId AND [pool_id]=@PoolId";

        public const string SetUserSettings_UPDATE_UserInfo = @"UPDATE UserInfo SET tp_SystemId=@SystemId,tp_Login=@LoginName,tp_Title=@Title,tp_Email=@Email WHERE tp_SiteId=@SiteId AND tp_Id=@Id";

        [QueryCommandArgument(Arguments =new object[] { "$displayField", "$nameField", "$emailField" })]
        public static string SetUserSettings_UPDATE_AllUserData(string displayField, string nameField, string emailField)
        {
            return string.Format("UPDATE AllUserData SET {0}=@Title, {1}=@LoginName, {2}=@Email WHERE tp_ListId=@UserListId AND tp_Id=@Id AND tp_RowOrdinal=0",
                displayField, nameField, emailField);
        }

        public const string SetItemAECMAndAppAE_UPDATE_AllUserData = @"UPDATE AllUserData  SET tp_AppEditor=@AppEditor, tp_AppAuthor=@APPAuthor,tp_Editor=@Editor, tp_Author=@Author,tp_Created=@Created,tp_Modified=@Modified 
                    WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                            AND tp_UIVersion=@Version AND tp_CalculatedVersion=0 AND tp_Level=@Level";

        public const string SetItemAECM_UPDATE_AllUserData = @"UPDATE AllUserData SET tp_Editor=@Editor, tp_Author=@Author,tp_Created=@Created,tp_Modified=@Modified 
                    WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                                AND tp_UIVersion=@Version AND tp_CalculatedVersion=0 AND tp_Level=@Level";

        public const string SetItemCMById_UPDATE_AllDocs = @"Update AllDocs Set TimeCreated=@TimeCreated, TimeLastModified = @TimeLastModified Where SiteId =@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And UIVersion=@UIVersion And Level=@Level";

        public const string SetItemAECM2_UPDATE_AllUserData = @"Update AllUserData Set tp_Modified=@tp_Modified,tp_Created=@tp_Created,tp_Author=@tp_Author,tp_Editor=@tp_Editor 
                Where tp_SiteId =@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1)  And tp_ParentId=@ParentId And tp_DocId=@Id And tp_UIVersion=@UIVersion And tp_Level=@Level";

        public const string SetWebAuthor_UPDATE_AllWebs = @"update AllWebs set Author =@UserID where Id=@WebID and SiteId=@SiteId";

        private const string SetColValueWithRowOrdinalFormat_UPDATE_AllUserData = @"update AllUserData set {0}=@colValue where tp_SiteId=@siteId and tp_DeleteTransactionId=0x and tp_DocId=@docId and tp_UIVersion=@UIVersion and tp_RowOrdinal =@rowOrdinal";

        [QueryCommandArgument(Arguments =new object[] { "$colName" })]
        public static string SetColValueWithRowOrdinal_UPDATE_AllUserData(string colName)
        {
            return string.Format(SetColValueWithRowOrdinalFormat_UPDATE_AllUserData, colName);
        }

        private const string SetColValueFormat_UPDATE_AllUserData = @"update AllUserData set {0}=@colValue where tp_SiteId=@siteId and tp_DeleteTransactionId=0x and tp_DocId=@docId and tp_UIVersion=@UIVersion";

        [QueryCommandArgument(Arguments =new object[] { "$colName" })]
        public static string SetColValue_UPDATE_AllUserData(string colName)
        {
            return string.Format(SetColValueFormat_UPDATE_AllUserData, colName);
        }

        #region ChangeItemId1_UPDATE_AllDocs_AllUserData_AllListsAux_NameValuePair
        public const string ChangeItemId1_UPDATE_AllDocs_AllUserData_AllListsAux_NameValuePair = @"
        SET NOCOUNT ON
        DECLARE @FromId INT
        DECLARE @NextId INT
        DECLARE @WebId uniqueidentifier
        DECLARE @ListId uniqueidentifier
        DECLARE @FromDirName NVARCHAR(256)
        DECLARE @FromLeafName NVARCHAR(128)
        DECLARE @FromParentId uniqueidentifier
        DECLARE @HasAttachment BIT
        DECLARE @FromItemOrder INT
        BEGIN TRAN
        
        SELECT @WebId=WebId,@ListId=ListId,@FromId=DoclibRowId,@FromDirName=DirName,@FromLeafName=LeafName,@FromParentId=ParentId
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND Id=@Id
        
        IF @@ROWCOUNT=0
        BEGIN
          COMMIT TRAN
          SELECT -100
          RETURN
        END
        
        IF @FromId IS NULL
        BEGIN
          COMMIT TRAN
          SELECT -101
          RETURN
        END
        
        IF @FromId=@ToId
        BEGIN
          COMMIT TRAN
          SELECT 0
          RETURN
        END
        
        IF EXISTS(SELECT TOP 1 tp_ID 
        FROM AllUserData WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId 
        AND tp_ListId=@ListId 
        AND tp_ID=@ToId 
        AND tp_IsCurrentVersion=1
        )
        BEGIN
          COMMIT TRAN
          SELECT -1
          RETURN
        END
        
        SELECT @NextId=NextAvailableId 
        FROM AllListsAux WITH(UPDLOCK)
        WHERE ListID=@ListId AND SiteId=@SiteId
        
        IF @@ROWCOUNT <> 1
        BEGIN
          ROLLBACK TRAN
          SELECT -102
          RETURN
        END
        
        IF @ToId>@FromId AND @ToId>=@NextId
        BEGIN
          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
          WHERE ListID=@ListId AND SiteId=@SiteId
          
          IF @@ROWCOUNT <> 1
          BEGIN
            ROLLBACK TRAN
            SELECT -103
            RETURN
          END
        END
        
        -- Do we need to handle version?
        
        SET @HasAttachment = 0
        
        SELECT @HasAttachment=tp_HasAttachment,@FromItemOrder=tp_ItemOrder
        FROM AllUserData WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -104
          RETURN
        END
        
        DECLARE @ToLeafName NVARCHAR(128)
        IF @ItemType <> 1
        BEGIN
          SET @ToLeafName=@FromLeafName
        END
        ELSE
        BEGIN
          SET @ToLeafName=CAST(@ToId AS NVARCHAR(128))+N'_.000'
        END
        
        IF @FromItemOrder IS NULL
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId 
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        END
        ELSE
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId,tp_ItemOrder=(@ToId*100) 
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        END
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -105
          RETURN
        END
        
        UPDATE AllDocs SET LeafName=@ToLeafName,DoclibRowId=@ToId WHERE SiteId=@SiteId AND ParentId=@FromParentId AND Id=@Id AND DeleteTransactionId=0x
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END

        UPDATE NameValuePair SET ItemId=@ToId WHERE ListId=@ListId AND ItemId=@FromId            

        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END
        
        IF @HasAttachment=1
        BEGIN
          DECLARE @AttachmentId uniqueidentifier
          SELECT @AttachmentId=Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@RootFolderId AND LeafName='Attachments' AND DeleteTransactionId=0x
          
          IF @@ROWCOUNT <> 0
          BEGIN
            SELECT @AttachmentId=Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@AttachmentId AND LeafName=CAST(@FromId AS NVARCHAR(128)) AND DeleteTransactionId=0x
            
            IF @@ROWCOUNT <> 0
            BEGIN
              DECLARE @ToIdString NVARCHAR(128)
              DECLARE @ToAttachmentDirName NVARCHAR(256)
              SET @ToIdString=CAST(@ToId AS NVARCHAR(128))
              SET @ToAttachmentDirName=@FromDirName+'/Attachments/'+@ToIdString
              
              UPDATE AllDocs SET LeafName=@ToIdString WHERE SiteId=@SiteId AND Id=@AttachmentId AND DeleteTransactionId=0x
              UPDATE AllDocs SET DirName=@ToAttachmentDirName WHERE SiteId=@SiteId AND ParentId=@AttachmentId AND DeleteTransactionId=0x
              
              IF @@ERROR <> 0
              BEGIN
                ROLLBACK TRAN
                SELECT -107
                RETURN
              END
            END
          END
        END
        
        COMMIT TRAN
        SELECT 0
        RETURN";
        #endregion

        #region ChangeItemId2_UPDATE_AllDocs_AllUserData_AllListsAux_NameValuePair
        public const string ChangeItemId2_UPDATE_AllDocs_AllUserData_AllListsAux_NameValuePair = @"
        SET NOCOUNT ON
        DECLARE @NextId INT
        BEGIN TRAN
        
        
        IF EXISTS(SELECT TOP 1 0 
        FROM AllUserData 
        WHERE tp_ListId=@ListId 
        AND tp_IsCurrentVersion=1 
        AND tp_ID=@ToId
        AND tp_CalculatedVersion=0
        AND tp_SiteId=@SiteId
        )
        BEGIN
          COMMIT TRAN
          SELECT -1
          RETURN
        END
        
        SELECT @NextId=NextAvailableId 
        FROM AllListsAux WITH(UPDLOCK)
        WHERE ListID=@ListId AND SiteId=@SiteId
        
        IF @@ROWCOUNT <> 1
        BEGIN
          ROLLBACK TRAN
          SELECT -102
          RETURN
        END
        
        IF @ToId>@FromId AND @ToId>=@NextId
        BEGIN
          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
          WHERE ListID=@ListId AND SiteId=@SiteId
          
          IF @@ROWCOUNT <> 1
          BEGIN
            ROLLBACK TRAN
            SELECT -103
            RETURN
          END
        END
        
        -- Do we need to handle version?
        
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId,tp_ItemOrder=(@ToId*100) 
        WHERE tp_SiteId=@SiteId 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        AND tp_ParentId=@ParentId 
        AND tp_DocId=@Id 
        END
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -105
          RETURN
        END
        
        UPDATE AllDocs SET DoclibRowId=@ToId WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id 
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END

        UPDATE NameValuePair SET ItemId=@ToId WHERE ListId=@ListId AND ItemId=@FromId            

        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END
                
        COMMIT TRAN
        SELECT 0
        RETURN";
        #endregion

        private static string SetUserDataFormat_UPDATE_AllUserData = @"UPDATE AllUserData SET {0} WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocId=@ID AND tp_CalculatedVersion=0 AND tp_Level=@Level";

        [QueryCommandArgument(Arguments = new object[] { "$colName1=@colValue1,$colName2=@colValue2" })]
        public static string SetUserData_UPDATE_AllUserData(string setColumnCommand)
        {
            return string.Format(SetUserDataFormat_UPDATE_AllUserData, setColumnCommand);
        }

        public const string SetUserDataJunctions_UPDATE_AllUserDataJuncations = @"UPDATE AllUserDataJunctions SET tp_ParentId=@tp_ParentId 
                        WHERE tp_SiteId=@tp_SiteId AND tp_DeleteTransactionId=0x AND tp_DocId=@tp_DocId";

        public const string SetUserData2_UPDATE_AllUserData = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_Guid=@tp_Guid
                        WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x";

        public const string SetAllDocs1_UPDATE_AllDocs = @"UPDATE AllDocs SET DirName=@DirName, LeafName=@LeafName, ParentId=@ParentId
                        WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x";

        private const string SetUserData3Format_UPDATE_AllUserData = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_GUID=@tp_GUID,{0}={1}
                        WHERE tp_SiteId=@SiteId AND tp_Listid=@ListId And tp_DeleteTransactionId=0x And tp_ID=@ID";

        [QueryCommandArgument(Arguments = new object[] { "$colName","@colValue" })]
        public static string SetUserData3_UPDATE_AllUserData(string colName, string colValue)
        {
            return string.Format(SetUserData3Format_UPDATE_AllUserData, colName, colValue);
        }

        public const string SetAllDocs2_UPDATE_AllDocs = @"UPDATE AllDocs SET DirName=@DirName, ParentId=@ParentId
                        WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x";

        public const string SetUIVersion_UPDATE_AllDocs_AllUserData = @"
            Begin Tran
               Update AllDocs Set UIVersion = @originalVersion where SiteId = @SiteId And DeleteTransactionId = 0x And Id = @Id And UIVersion = @Version And ParentId = @ParentId
               Update AllUserData Set tp_UIVersion = @originalVersion where tp_SiteId = @SiteId And tp_DeleteTransactionId = 0x And(tp_IsCurrentVersion = 1 or tp_IsCurrentVersion = 0) And tp_Id = @RowId And tp_UIVersion = @Version And tp_ParentId = @ParentId And tp_DocId = @Id
            IF @@ERROR!= 0
              Begin
                Rollback Tran
              End
            Else
              Begin
                Commit Tran
              End";

        public static string SetDocData_UPDATE_AllDocs(Dictionary<string, object> docdata)
        {
            var stringBuilder = new StringBuilder();
            foreach (var listColumn in docdata)
            {
                stringBuilder.AppendFormat(",{0}=@{0}", listColumn.Key);
            }
            return string.Format(@"UPDATE AllDocs SET {0} WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND Level=@Level", stringBuilder.ToString().TrimStart(','));
        }

        public const string SetUserDeleteFalse_UPDATE_UserInfo = @"update UserInfo set tp_Deleted=0 where tp_SiteId=@SiteId and tp_SystemId=@SystemId";

        public const string SetItemLevel_UPDATE_AllDocs = @"update AllDocs set Level=@Level,DraftOwnerId=@DraftOwnerId where SiteID=@SiteID and DeleteTransactionId=0x and ParentID=@ParentID and Id=@ID and Level=@OldLevel";
        public const string SetItemLevel_UPDATE_AllUserData = @"UPDATE AllUserData SET tp_Level=@Level,tp_DraftOwnerId=@DraftOwnerId WHERE tp_SiteID=@SiteID AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocID=@ID AND tp_CalculatedVersion=0 AND tp_Level=@OldLevel";

        [QueryCommandArgument(Arguments = new object[] { true, true })]
        public static string SetWebpartProperties_UPDATE_AllWebParts(bool allUserPropertiesNull, bool perUserPropertiesNull)
        {
            return string.Format(@"UPDATE AllWebParts SET tp_AllUsersProperties={0},tp_PerUserProperties={1} WHERE tp_SiteId=@SiteID AND (tp_IsCurrentVersion=1 OR tp_IsCurrentVersion=0) AND tp_PageUrlID=@PageID AND tp_ID=@ID",
                    !allUserPropertiesNull ? "@AllUsersProperties" : "NULL",
                    !perUserPropertiesNull ? "@PerUserProperties" : "NULL");
        }

        [QueryCommandArgument(Arguments = new object[] { true,false, true, false })]
        public static string SetWebpartInfo_UPDATE_AllWebParts(bool baseViewIdNull, bool viewNull, bool updateContentType, bool updateDisplayName)
        {
            return string.Format(@"UPDATE AllWebParts SET tp_BaseViewID={0},tp_View={1}{2}{3}  
                             where tp_SiteId=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1)  AND tp_PageUrlID=@PageID AND tp_ID=@ID",
                !baseViewIdNull ? "@BaseViewID" : "NULL",
                !viewNull ? "@View" : "NULL",
                updateContentType ? ", tp_ContentTypeId=@ContentType" : string.Empty,
                updateDisplayName ? ", tp_DisplayName=@DisplayName" : string.Empty);
        }
        public static string SetWebpartVersionInfo_UPDATE_AllWebParts(bool IsNewLevel, bool IsPageVersion)
        {
            return string.Format(@"UPDATE AllWebParts SET {0}{1}tp_IsCurrentVersion = @IsCurrentVersion 
                       where tp_SiteId = @SiteID AND tp_IsCurrentVersion = 1 AND tp_PageUrlID = @PageID AND tp_PageVersion = 0  AND tp_Level = @Level AND tp_ID = @ID",
             IsNewLevel ? "tp_Level = @NewLevel," : string.Empty,
             IsPageVersion ? "tp_PageVersion = @PageVersion," : string.Empty);
        }

        public const string SetWebpartLevel_UPDATE_WebPartLists = @"UPDATE WebPartLists Set tp_Level=@SourceLevel where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@CurPageLevel";

        public const string SetWebpartUserID_UPDATE_AllWebParts = @"UPDATE AllWebParts SET tp_userId=@UserID WHERE tp_siteid=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_PageUrlID=@PageID  AND tp_id=@ID";

        public const string SetWebpartUserId_UPDATE_WebPartLists = @"UPDATE WebPartLists SET tp_userId=@UserID WHERE  tp_webpartid=(select top(1) tp_ID from AllWebParts where tp_siteid=@SiteID AND tp_id=@ID)";

        public const string SetWebpartUserId_UPDATE_Personalization = @"UPDATE Personalization SET tp_UserID=@UserID WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@CurrentUserID";

        public const string SetWebpartProperties_UPDATE_Personalization = @"UPDATE Personalization SET tp_PerUserProperties=@PerUserProperties where tp_SiteId=@SiteId AND tp_WebPartID=@ID AND tp_UserId=@UserId";

        public const string SetTPGUID_UPDATE_AllUserData = @"Update AllUserData SET tp_GUID=@tp_GUID 
                                WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_ParentId=@ParentId AND tp_DocId=@ID  
                                AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

        public const string SetItemDocInfoIndexIDLevel_UPDATE_AllDocs = @"UPDATE AllDocs SET TimeCreated=@TimeCreated,TimeLastModified=@TimeLastModified,UnVersionedMetaInfo=@UnVersionedMetaInfo,UnVersionedMetaInfoVersion=@UnVersionedMetaInfoVersion,UnVersionedMetaInfoSize=@UnVersionedMetaInfoSize 
                WHERE SiteId=@SiteId AND Id=@ID AND DeleteTransactionId=0x AND Level=@Level";

        public const string SetItemDocInfoIndexParentId_UPDATE_AllDocs = @"UPDATE AllDocs SET TimeCreated=@TimeCreated,TimeLastModified=@TimeLastModified,UnVersionedMetaInfo=@UnVersionedMetaInfo,UnVersionedMetaInfoVersion=@UnVersionedMetaInfoVersion,UnVersionedMetaInfoSize=@UnVersionedMetaInfoSize 
WHERE SiteId=@SiteId AND Id=@ID AND DeleteTransactionId=0x AND ParentId=@ParentId AND UIVersion = @UIVersion";

        [QueryCommandArgument(Arguments = new object[] {"$statusField" })]
        public static string SetWFStatus_UPDATE_AllUserData(string statusField)
        {
            return $"UPDATE AllUserData Set {statusField}=@StatusFieldValue WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_ListId=@ListId AND tp_GUID=@tpGuid AND tp_ID=@tpId AND tp_RowOrdinal=@rowOrdinal";
        }

        public const string SetWFStatusName_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET StatusFieldName=@StatusField WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetWFConfiguration_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET Configuration=@Configuration WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetWFNameById_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET Name=@Name WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetWFCreated_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET Created=@Created WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetWFAuthor_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET Author=@userId WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetWFModified_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET Modified=@Modified WHERE SiteId = @SiteId AND Id=@Id";

        public const string SetListModified_UPDATE_AllListsAux = @"UPDATE AllListsAux SET Modified=@Modified WHERE SiteId=@SiteId AND ListID=@ListID";

        public const string SetWFTemplateDocFlag_UPDATE_AllDocs = @"Update Alldocs Set DocFlags=DocFlags|0x00080000 Where SiteId=@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And Level=@Level";

        public const string SetWFNameValuePair_UPDATE_NameValuePair = @"UPDATE NameValuePair SET Value = @tp_WorkflowInstanceId WHERE SiteId=@tp_SiteId AND ListId=@tp_ListId AND FieldId=@WorkflowInstanceFieldId AND ItemId=@tp_Id AND Level=@tp_Level";

        public const string SetWFInstanceCount_UPDATE_WorkflowAssociation = @"UPDATE WorkflowAssociation SET InstanceCount=@Count WHERE Id=@Id";

        public const string SetItemCheckoutUserId1_UPDATE_AllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND DeleteTransactionId=0x AND ParentId=@ParentId AND  ID=@ID";

        public const string SetItemCheckoutUserId1_UPDATE_AllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocID=@ID";

        public const string SetItemCheckoutUserId2_UPDATE_AllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND ID=@ID AND DeleteTransactionId=0x";

        public const string SetItemCheckoutUserId2_UPDATE_AllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_DocID=@ID";

        public const string SetItemCheckoutUserId3_UPDATE_AllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID WHERE SiteId=@SiteID AND DeleteTransactionId=0x AND ParentId=@ParentID AND ID=@ID";

        public const string SetItemCheckoutUserId3_UPDATE_AllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocID=@ID";

        [QueryCommandArgument(Arguments = new object[] { "$colName1=@colValue1,$colName2=@colValue2" })]
        public static string SetListInfos_UPDATE_AllLists(string setCommand)
        {
            return $"update AllLists set {setCommand} where tp_SiteId=@SiteId and tp_WebId=@WebId and tp_ID=@ListId";
        }

        public static string SetVersionInfo_UPDATE_AllDocVersions(AveQueryWorker worker, Dictionary<string, object> allDocVersions, int uiVersion, bool resetValue)
        {
            return new SetAllDocVersionsCommandBuilder(allDocVersions, uiVersion, resetValue).Build(worker);
        }
        public static string SetVersionInfo_UPDATE_AllDocs(AveQueryWorker worker, Dictionary<string, object> allDocsData)
        {
            return new SetAllDocsCommandBuilder(allDocsData).Build(worker);
        }
        public static string SetVersionInfo_UPDATE_AllUserData(AveQueryWorker worker, Dictionary<string, object> userData)
        {
            return new SetAllUserDataCommandBuilder(userData).Build(worker);
        }

        public const string SetItemCMByName_UPDATE_AllDocs = "Update AllDocs set TimeCreated=@TimeCreated, TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ";

        public const string UpdateSiteDeletedStatus_Update_AllSites = @"UPDATE [AllSites] SET [Deleted] = 1 WHERE Id=@SiteId";

        public const string UpdateContentNative_Update_DocStreams = @"UPDATE ADS 
SET Content.write(@streamBuffer,NULL,NULL)
FROM DocStreams AS ADS WITH (INDEX(DocStreams_CI)) 
WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition and BSN=@BSN";

        public const string SetContentNullNative_Update_DocStreams = @"UPDATE ADS 
SET Content=0x,RbsId=NULL 
FROM DocStreams AS ADS WITH (INDEX(DocStreams_CI)) 
WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition and BSN=@BSN";

        public const string UpdateDocumentSize_Update_AllDocs = @"Update AllDocs Set Size = @Size where SiteId=@SiteId AND ( DeleteTransactionId = 0x or DeleteTransactionId <> 0x ) AND ParentId=@ParentId AND Id=@Id AND InternalVersion=@InternalVersion ;
                            UPDATE AllDocVersions SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (InternalVersion = @InternalVersion);";

        public const string UpdateContentFiletoStub_Update_DocStreams = @"Update DocStreams Set Content=null, RbsId=@RbsId where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs With(nolock) WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);";

        public const string UpdateStubFileToContent_Update_DocStreams = @"Update DocStreams Set Content=@Content, RbsId=null where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM  AllDocs With(nolock)WHERE (SiteId = @SiteId) AND (Id = @Id) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);";

        public const string UpdateStubDocumentSize_Update_AllDocs = @"Update AllDocs Set Size = @Size,NextBSN= @nextBSN where SiteId=@SiteId AND ( DeleteTransactionId = 0x or DeleteTransactionId <> 0x ) AND ParentId=@ParentId AND Id=@Id AND Level=@Level";

        public const string UpdateStubDocumentContent_Update_DocStreams = @"Update DocStreams Set Content=@Content where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs With(nolock) WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));";

        public const string UpdateStubDocumentAndVersionSize_Update_AllDocs_AllDocVersions = @"UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);
                        UPDATE AllDocVersions SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)";

        public const string UPDATEDeleteTransactionId_UPDATE_AllDocVersions = @"UPDATE AllDocVersions Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";

        public const string UPDATEDeleteTransactionId_UPDATE_AllDocs = @"UPDATE AllDocs Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";

        public const string UPDATEDeleteTransactionId_UPDATE_AllUserData = @"UPDATE AllUserData Set tp_DeleteTransactionId=0x WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId<>0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";

        public const string SetItemEditor_UPDATE_AllUserData = @"UPDATE AllUserData SET tp_Editor=@Editor WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ListId=@ListId AND tp_ID=@ID";

        #region Connector

        public const string UpdateContentInDbForLargeFileV1_Update_DocsToStreams_DocStreams_AllDocs_AllDocVersions = @"
DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND DocsToStreams.Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 1);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 2);
ELSE
BEGIN
    UPDATE DocStreams SET Content = 0x0, [RbsId]=NULL, TYPE=11 WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
    set @row = @@ROWCOUNT
    IF @row=0 RAISERROR ('Cannot find the stream.', 16, 3);
END
IF @histVersion =0
  UPDATE AllDocs SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND Level=@Level
ELSE
  UPDATE AllDocVersions SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@histVersion
";

        public const string UpdateContentInDbForLargeFileV2_Update_DocStreams = @"
UPDATE DocStreams 
SET Content = 0x0 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";

        public const string UpdateContentInDbForLargeFileV3_Update_DocStreams_DocsToStreams = @"
DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 1);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 2) ;
ELSE
BEGIN
    UPDATE DocStreams SET Content.WRITE(@tempbuffer,0,null) WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
    set @row = @@ROWCOUNT
    IF @row=0 RAISERROR ('Cannot find the stream.', 16, 3);
END";

        public const string UpdateContentInDbForLargeFileV4_Update_DocStreams_DocsToStreams = @"DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 4);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 5);
ELSE
BEGIN
    UPDATE DocStreams SET Content.WRITE(@tempbuffer,null,null) WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
    set @row = @@ROWCOUNT
    IF @row=0 RAISERROR ('Cannot find the stream.', 16, 6);
END";

        public const string ClearEbsInfo_Update_DocStreams = @"
UPDATE DocStreams 
SET Content = 0x0 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";

        public const string UpdateFileContentSize_Update_AllDocs_AllDocVersions_DocsToStreams_DocStreams = @"
--DECLARE @SiteId uniqueidentifier, @ParentId uniqueidentifier, @DocId uniqueidentifier, @Level tinyint, @UIVersion int, @Size int, @Partition tinyint
DECLARE @bsn bigint, @histVersion int, @rowc int

UPDATE AllDocs SET Size=@Size,SizeWrite=@Size WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@DocId AND Level=@Level AND UIVersion=@UIVersion
SELECT @rowc=@@ROWCOUNT
IF @rowc>0
    SELECT @histVersion=0
ELSE 
BEGIN
    SELECT @histVersion=@UIVersion
    UPDATE AllDocVersions SET Size=@Size,SizeWrite=@Size WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@UIVersion
    SELECT @rowc=@@ROWCOUNT
END

IF @rowc>0
BEGIN
    SELECT @bsn=BSN FROM DocsToStreams WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
    SELECT @rowc=@@ROWCOUNT
    IF @rowc>1 RAISERROR ('More than one stream map.', 16, 1);
    IF @rowc=0 RAISERROR ('Cannot find stream maps.', 16, 2);
    ELSE
    BEGIN
        UPDATE DocStreams SET Size=@Size WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@bsn
        IF @@ROWCOUNT=0 RAISERROR ('Cannot find the stream.', 16, 3);
    END
END
ELSE RAISERROR ('Cannot find the document.', 16, 4);
";

        public const string SetRbsIdToNull_Update_DocStreams = @"
UPDATE DocStreams 
SET 
RbsId = NULL 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";

        public const string UpdateStreamSchema_Update_AllDocs_TVF_DocsToStreams_SiteDocHistVerLvlPart_TVF_DocStreams_CI_TVF_DocsToStreams_CI = @"
declare @Partition tinyint,
        @Rows int,
        @HistVersion int,   
        @RbsId varbinary(64),
        @BSN int

set @HistVersion=0
set @Partition=0

SELECT @RbsId= DS.RbsId,@BSN= DS.BSN FROM 
        TVF_DocsToStreams_SiteDocHistVerLvlPart(@SiteId,@DocId,@HistVersion,@Level,@Partition) as DTS
CROSS APPLY 
        TVF_DocStreams_CI(DTS.SiteId,DTS.DocId,DTS.Partition,DTS.BSN) as DS
ORDER BY DS.BSN DESC

SET @Rows=@@ROWCOUNT

IF (@Rows=0)
   SET @Ret=2
ELSE IF(@RbsId IS NOT NULL)
   IF (@Rows=1)
       BEGIN
           UPDATE AllDocs  SET StreamSchema=@StreamSchema  WHERE Id=@DocId AND SiteId=@SiteId and Level=@Level and ParentId=@ParentId AND  DeleteTransactionId=0x
           SET @Ret=0
       END
   ELSE
       BEGIN
            BEGIN TRAN
                DELETE 
                  DTS
                FROM 
                  TVF_DocsToStreams_CI(@SiteId,@DocId,@HistVersion,@Level,@Partition,@BSN) as DTS
                DELETE
                    DS
                FROM 
                    TVF_DocStreams_CI(@SiteId,@DocId,@Partition,@BSN) as DS
            IF @@ERROR <> 0
               BEGIN
                  ROLLBACK TRAN
                  SET @Ret=2
               END
            ELSE
               BEGIN
                  COMMIT TRAN
                  SET @Ret=1
               END
        END
ELSE
   SET @Ret=2";

        public const string UpdateStreamSchemaByDocId_Update_AllDocs_AllDocVersions = @"
IF @histVersion =0
  UPDATE AllDocs SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND Level=@Level
ELSE
  UPDATE AllDocVersions SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@histVersion
";

        public static string UpdateFileDocFlag_Update_AllDocs_AllDocVersions(bool isStub)
        {
            const string updateCommand = @"UPDATE AllDocs SET {0} WHERE Id=@Id AND Level=@Level AND UIVersion=@UIVersion AND DeleteTransactionId=0x AND {1};
                      UPDATE AllDocVersions SET {0} WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND {1};";
            return isStub 
                ? string.Format(updateCommand, "DocFlags=DocFlags|65536", "DocFlags&65536<>65536") 
                : string.Format(updateCommand, "DocFlags=DocFlags&(~65536)", "DocFlags&65536=65536");
        }

        public static string UpdateThumbnailForConnector_Update_AllUserData(bool snapShotIsNull,bool isVideo)
        {
            const string updateBaseCommand = @"Update AlluserData Set {0} Where tp_SiteId = @SiteId And tp_DeleteTransactionId = 0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocId=@ItemId AND tp_CalculatedVersion=0 AND tp_Level=@ItemLevel And tp_RowOrdinal=0";
            string text;
            if (isVideo)
            {
                text = snapShotIsNull ? "nvarchar13=@Player" : "ntext2=@snapShot,nvarchar13=@Player";
            }
            else
            {
                text = snapShotIsNull ? "nvarchar9=@Resolution,nvarchar13=@Player" : "ntext2=@snapShot,nvarchar9=@Resolution,nvarchar13=@Player";
            }
            return string.Format(updateBaseCommand, text);
        }

        public static string UpdateFileAuthorEditor_Update_AllUserData(Guid parentId)
        {
            const string updateCommand = @"
update AllUserData 
set tp_Author=@tp_Author,
tp_Editor=@tp_Editor 
where 
tp_SiteId=@SiteId AND 
tp_DeleteTransactionId=0x AND 
tp_IsCurrentVersion=1 AND 

tp_DocId=@Id AND 
tp_CalculatedVersion=0 AND 
tp_Level=@Level AND 
tp_rowordinal=0";
            if (parentId != Guid.Empty)
            {
                return updateCommand + " AND tp_ParentId=@ParentId ";
            }
            return updateCommand;
        }

        #endregion Connector
    }

    class SetAllDocVersionsCommandBuilder
    {
        private Dictionary<string, object> allDocVersions;
        private int uiVersion;
        private bool resetValue;
        public SetAllDocVersionsCommandBuilder(Dictionary<string, object> allDocVersions, int uiVersion, bool resetValue)
        {
            this.allDocVersions = allDocVersions;
            this.uiVersion = uiVersion;
            this.resetValue = resetValue;
        }
        internal string Build(AveQueryWorker worker)
        {
            var manager = new AveQueryColumnInfoManager("AllDocVersions");
            var needUpdateDocData = AssemblySetParametrs(this.allDocVersions, this.uiVersion, this.resetValue);
            string whereCondition = AssemblyWhereCondition(needUpdateDocData.Count > 0);
            manager.MakeUpdateCommand(worker.Command, needUpdateDocData, (List<string>)null, whereCondition);
            return worker.Command.CommandText;
        }

        private static string AssemblyWhereCondition(bool includeOtherColumn)
        {
            return includeOtherColumn ? 
                ",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version" : 
                "DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
        }

        private static Dictionary<string, object> AssemblySetParametrs(Dictionary<string, object> allDocVersions, int version, bool resetValue)
        {
            var needUpdateColums = new List<string>()
                {
                     "TimeCreated",
                     "Size",
                     "CheckinComment",
                     "Level",
                     "VirusVendorID",
                     "VirusStatus",
                     "VirusInfo",
                };
            if (resetValue)
            {
                needUpdateColums.Add("DraftOwnerId");
            }

            var needUpdateDocData = needUpdateColums.Where(colName => allDocVersions.ContainsKey(colName)).ToDictionary(colName => colName, colName => allDocVersions[colName]);
            if (resetValue)
            {
                needUpdateDocData["Level"] = version % 512 == 0 ? 1 : 2;
            }

            return needUpdateDocData;
        }
    }

    class SetAllDocsCommandBuilder
    {
        Dictionary<string, object> allDocsData;

        public SetAllDocsCommandBuilder(Dictionary<string, object> allDocsData)
        {
            this.allDocsData = allDocsData;
        }

        internal string Build(AveQueryWorker worker)
        {
            var manager = new AveQueryColumnInfoManager("AllDocs");
            var needUpdateDocData = AssemblySetParametrs(this.allDocsData);
            string whereClause = AssemblyWhereCondition(needUpdateDocData.Count>0);
            manager.MakeUpdateCommand(worker.Command, needUpdateDocData, (List<string>)null, whereClause);
            return worker.Command.CommandText;
        }

        private static Dictionary<string, object> AssemblySetParametrs(Dictionary<string, object> allDocsData)
        {
            var needUpdateColums = new List<string>(10)
            {
                 "LeafName",
                 "Size",
                 "TimeCreated",
                 "TimeLastModified",
                 "NextToLastTimeModified",
                 "MetaInfoTimeLastModified",
                 "TimeLastWritten",
                 "CheckoutDate",
                 "Level",
                 "CheckinComment",
             };
            return needUpdateColums.Where(colName => allDocsData.ContainsKey(colName)).ToDictionary(colName => colName, colName => allDocsData[colName]);
        }

        private static string AssemblyWhereCondition(bool includeOtherColumn)
        {
            return includeOtherColumn ?
                ",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version" :
                "DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";
        }

    }

    class SetAllUserDataCommandBuilder
    {
        Dictionary<string, object> userData;

        public SetAllUserDataCommandBuilder(Dictionary<string, object> userData)
        {
            this.userData = userData;
        }
        internal string Build(AveQueryWorker worker)
        {
            var manager = new AveQueryColumnInfoManager("AllUserData");
            var unUpdateColumns = new List<string>()
            {
                    "tp_ID",
                    "tp_SiteId",
                    "tp_ListId",
                    "tp_RowOrdinal",
                    "tp_Version",
                // "tp_ItemOrder",注释这行代码是因为发现tp_itemOrder会影响ADO-14176中page页上TabwebPart上的顺序，所以还原这个属性。
                    "tp_ContentTypeId",
                    "tp_UIVersion",
                    "tp_CalculatedVersion",
                    "tp_UIVersionString",
                // "tp_CheckoutUserId",
                    "tp_DocId",
                    "tp_GUID",
            };
            string whereClause = "WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@UIVersion";
            manager.MakeUpdateCommand(worker.Command, this.userData, unUpdateColumns, whereClause);
            return worker.Command.CommandText;
        }


    }
}
