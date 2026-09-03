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
    [Common.QueryCommandString(Common.SPDatabaseVersion.SharePoint2016TAP1, Common.QueryCommandType.Delete)]
    internal static class SP2016DeleteQueryString
    {
        public const string RemoveWebPartByID_DELETE_AllWebParts = @"delete from AllWebParts where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_ID=@ID";

        public const string RemovePersonalWebPart_DELETE_AllWebParts = @"delete from AllWebParts where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_Level=@Level AND tp_UserID > 0 AND tp_ID in ({0})";

        public const string RemovePersonalWebPart_DELETE_Personalization = @"DELETE Personalization WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@UserID";

        public const string RemoveUserDataJuncation_DELETE_AllUserDataJunctions = @"Delete From AllUserDataJunctions Where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_UIVersion=@UIVersion And tp_FieldId=@FieldId And tp_DocId=@DocId And tp_SourceListId=@ListId";

        public const string RemoveEventReceivers_DELETE_EventReceivers = @"DELETE EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND Type=32767 AND ContextCollectionId=@ContextCollectionId AND ContextObjectId IS NULL AND ContextId IS NULL AND ContextType IS NUll AND ContextEventType IS NULL AND SequenceNumber=@SequenceNumber AND Assembly='' AND Class=''";

        public const string RemoveDocsToStreams_DELETE_DocsToStreams = @"DELETE from DocsToStreams where siteId = @SiteId and DocId = @DocId and HistVersion = @HistVersion And Level = @Level";

        public const string RemoveDocStreams_DELETE_DocStreams = @"Delete from DocStreams where siteId= @SiteId and DocId = @DocId";

    }
}
