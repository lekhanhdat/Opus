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
namespace AvePoint.Wrapper.Common
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    public interface IAveDiscoverReader
    {
        /// <summary>
        /// Get query string for one attachment.
        /// </summary>
        /// <returns></returns>
        string GetSingleItemAttachmentsQueryString();

        /// <summary>
        /// Get query string for all attachment.
        /// </summary>
        /// <returns></returns>
        string GetAttachmentsQueryString();
        /// <summary>
        /// Get query string for all attachment with recycle bin.
        /// </summary>
        /// <returns></returns>
        string GetAttachmentsWithRecycleBinQueryString();
        /// <summary>
        /// Get query string in AllUserdata table.  Note: Extender is special.  Empty.
        /// </summary>
        /// <returns></returns>
        string GetAllVersionsQueryString(bool includeRecyclebin = false);
        
        /// <summary>
        /// Only from Extender 10. Get Content from AlldocStreams table.
        /// </summary>
        /// <returns></returns>
        string GetAttachmentStubContentForIB();
                
        /// <summary>
        /// Get query string in AllUserData table for SP 07.
        /// </summary>
        /// <returns></returns>
        string GetAllVersionsQueryStringFor07();
        /// <summary>
        /// Get query string in Alldocs table.
        /// </summary>
        /// <returns></returns>
        string GetAllItemsInAllDocQueryString();
        /// <summary>
        /// Get stub data's content
        /// </summary>
        /// <returns></returns>
        string GetAllItemAndVersionsStubInfoQueryString();
        
        /// <summary>
        /// Only for extender.
        /// </summary>
        /// <returns></returns>
        string GetAllAttachmentsStubInfoQueryString();
        /// <summary>
        /// Get Item query string in AllUserData table.
        /// </summary>
        /// <returns></returns>
        string GetAllItemsInUserDataQueryString();
        /// <summary>
        /// Get item's base column in alldocs table.
        /// </summary>
        /// <returns></returns>
        string GetItemColumns();
        /// <summary>
        /// Get Attachmetn's properties from reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr);
        /// <summary>
        /// Get attachmet's properties from DocObject
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tempDoc"></param>
        void ReadAttachmentContent(AveItemObject obj, DocObject tempDoc);
        /// <summary>
        /// Get Items's special properties from reader.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="item"></param>
        void OverriteProperties(SqlDataReader sr, AveItemObject item);

        /// <summary>
        /// Get stub item's properties from reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadStubItemContent(AveItemObject obj, SqlDataReader sr);
        /// <summary>
        /// Get item's properties form reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadItemContent(AveItemObject obj, IAveQueryDataReader sr);
        /// <summary>
        ///  Get item's properties form reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadItemContent(AveItemObject obj, SqlDataReader sr);
        /// <summary>
        /// Get item's properties form reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadItemContentForIB(AveItemObject obj, SqlDataReader sr);
        /// <summary>
        /// Get item's properties form DocObject.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="doc"></param>
        void ReadItemContentForIB(AveItemObject obj, DocObject doc);
        /// <summary>
        /// Get version's properties form reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadVersionContent(AveVersionObject obj, SqlDataReader sr);
        /// <summary>
        /// Get version's properties form reader.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="obj"></param>
        void ReadVersionStubInfo(SqlDataReader sr, AveVersionObject obj);
        /// <summary>
        /// Get attachment's properties form reader.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="obj"></param>
        void ReadAttachmentStubInfo(SqlDataReader sr, AveItemObject obj);
        /// <summary>
        /// Get version's properties with "DeleteTransactionId"  form reader 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadVersionContentWithDeleteState(AveVersionObject obj, SqlDataReader sr);
        /// <summary>
        /// Get stub version's properties from reader.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sr"></param>
        void ReadStubVersionContent(AveVersionObject obj, SqlDataReader sr);
        /// <summary>
        /// get UIversion and TimeLastModified from container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="obj"></param>
        void GenerateVersionObject(Dictionary<string, object> container, AveVersionObject obj);
        /// <summary>
        /// determine a folder if is a unused folder.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="noList"></param>
        /// <returns></returns>
        bool IsUnusedFolder(string url, bool noList);

        
        /// <summary>
        /// Only For extender
        /// </summary>
        /// <param name="item"></param>
        /// <param name="attachment"></param>
        /// <param name="sr"></param>
        void AddExtentionAttachment(AveItemObject item, AveItemObject attachment, SqlDataReader sr);

        /// <summary>
        /// Only extender return true
        /// </summary>
        /// <returns></returns>
        bool NeedGetItemStubInfo();
    }
}