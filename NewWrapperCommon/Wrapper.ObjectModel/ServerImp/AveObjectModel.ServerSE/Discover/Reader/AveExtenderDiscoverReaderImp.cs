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
using System.Data.SqlClient;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    extern alias QueryService16;
    using QueryService16.AvePoint.Wrapper.QueryService;
    class AveExtenderDiscoverReaderImp : AveExtenderDiscoverReader
    {
        private static AveExtenderDiscoverReaderImp reader;
        private static object lockObject = new object();

        protected AveExtenderDiscoverReaderImp() { }

        public static AveExtenderDiscoverReaderImp GetInstance()
        {
            if (reader == null)
            {
                lock (lockObject)
                {
                    if (reader == null)
                    {
                        reader = new AveExtenderDiscoverReaderImp();
                    }
                }
            }
            return reader;
        }

        public override string GetAttachmentsQueryString()
        {

            return AveQueryString16.Sp16AllAttachmentsForExternder;
        }

        public override string GetSingleItemAttachmentsQueryString()
        {
            return AveQueryString16.Sp16SingleAttachmentForExternder;
        }

        public override string GetAttachmentsWithRecycleBinQueryString()
        {
            return AveQueryString16.Sp16AllAttachmentsForExternderWithRecycleBin;
        }
        
        public override string GetAllItemsInAllDocQueryString()
        {
            return AveQueryString16.Sp16AllItemsInDocsForExtender;
        }

        public override string GetAllVersionsQueryString(bool includeRecyclebin = false)
        {
            return includeRecyclebin ? AveQueryString16.Sp16AllItemsAndVersionsForExtenderWithRecycleBin : AveQueryString16.Sp16AllItemsAndVersionsForExtender;
        }

        public override string GetAllItemAndVersionsStubInfoQueryString()
        {
            return AveQueryString16.Sp16AllItemAndVersionsStubInfo;
        }

        public override string GetAllAttachmentsStubInfoQueryString()
        {
            return AveQueryString16.Sp16AllAttachmentsStubInfoForExtender;
        }

        public override string GetDocInfoForIBQueryString()
        {
            return AveQueryString16.Sp16AllDocValueForEventCache_Extender;
        }

        //只有extender 10使用
        public override string GetAttachmentStubContentForIB()
        {
            return string.Empty;
        }


        public override string GetItemColumns()
        {
            return " doc.Id,doc.LeafName,doc.DoclibRowId,doc.Type,doc.UIVersion as UIVersion,doc.DocFlags,doc.HasStream,doc.Level, 2 AS QueryType, NULL AS Content,doc.Size ";
        }

        public override void ReadAttachmentContent(AveItemObject obj, SqlDataReader sr)
        {
            obj.DocID = (Guid)sr["Id"];
            obj.DirName = (string)sr["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)sr["LeafName"];
            obj.ParentID = (Guid)sr["ParentId"];
            obj.Level = (byte)sr["Level"];
            //if (!(sr["Content"] is DBNull))
            //{
            //    obj.Content = (byte[])sr["Content"];//Stub
            //}
            if (!(sr["DocFlags"] is DBNull))
            {
                obj.DocFlags = (int)sr["DocFlags"];
            }
            if (!(sr["Size"] is DBNull))
            {
                obj.Size = long.Parse(sr["Size"].ToString());
            }
            //if (!(sr["RbsId"] is DBNull))
            //{
            //    obj.RbsId = (byte[])sr["RbsId"];
            //}
            obj.DeleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
        }
    }
}
