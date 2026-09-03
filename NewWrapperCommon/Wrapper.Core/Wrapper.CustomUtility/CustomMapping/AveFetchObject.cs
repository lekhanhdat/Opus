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

using System.Collections.Generic;
using System;
using System.Text;

namespace AvePoint.Wrapper.CustomUtility
{
    public static class AveFetchObject
    {
        private const string mContextPropertyName = "CurrentMetadata";
        private const string mContextContentTypeName = "CurrentContentType";
        private const string mFolderStruct = "FolderStruct";

        private const string mContentTypeIdBytes = "#tp_ContentTypeId";

        public static bool TryGetMetadata(string propertyName, out object value)
        {
            value = null;
            var metadataDic = AppDomain.CurrentDomain.GetData(mContextPropertyName) as Dictionary<string, object>;
            if (metadataDic != null && metadataDic.ContainsKey(propertyName))
            {
                value = metadataDic[propertyName];
            }                
                                                    
            return value != null;
        }

        public static string GetContentType()
        {
            string value = string.Empty;
            var metadataDic = AppDomain.CurrentDomain.GetData(mContextPropertyName) as Dictionary<string, object>;
            var contentTypeDic = AppDomain.CurrentDomain.GetData(mContextContentTypeName) as Dictionary<string, string>;
            if (metadataDic != null && metadataDic.ContainsKey(mContentTypeIdBytes))
            {
                string contentTypeId = HexStringFromBytes((byte[])metadataDic[mContentTypeIdBytes]);
                if (contentTypeDic != null && contentTypeDic.ContainsKey(contentTypeId))
                {
                    value = contentTypeDic[contentTypeId];
                }
            }
            return value;
        }

        public static string GetCallapseFolderStruct()
        {
            var folderStruct = AppDomain.CurrentDomain.GetData(mFolderStruct) as string;
            return folderStruct;
        }

        private static string HexStringFromBytes(byte[] rgb)
        {
            StringBuilder sb = new StringBuilder("0x", 2 + ((rgb != null) ? (rgb.Length * 2) : 0));
            if (rgb != null)
            {
                foreach (byte num in rgb)
                {
                    CharsOfByte(num, sb);
                }
            }
            return sb.ToString();
        }

        private static void CharsOfByte(byte b, StringBuilder sb)
        {
            char[] s_mphex2ch = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            sb.Append(s_mphex2ch[b >> 4]);
            sb.Append(s_mphex2ch[b & 15]);
        }
    }       
}
