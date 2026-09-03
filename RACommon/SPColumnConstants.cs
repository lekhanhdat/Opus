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

namespace AvePoint.RA.Common
{
    public class SPColumnConstants
    {
        public const string SP_Title = "Title";
        public const string SP_URL = "URL";
        public const string SP_NAME = "Name";
        public const string CONTENT_TYPE_DOCUMENT_NAME = "Document";
        public const string CONTENT_TYPE_DOCUMENT_SET = "Document Set";
        public const string FileLeafRef = "FileLeafRef";
        public const string ID_FOR_TERM = "IdForTerm";
        public const string SP_ID = "ID";
        public const string SP_ComplianceTag = "_ComplianceTag";
        public const string SP_Created = "Created";
        public const string File_Size = "File_x0020_Size";
        public const string Author = "Author";
        public const string Editor = "Editor";
        public const string Modified = "Modified";
        public const string DocumentIdUrl = "_dlc_DocIdUrl";
        public const string DocumentId = "_dlc_DocId";
        public const string SP_Sensitive_Name = "MSIP_Label_{0}_Name";
        public const string Sensitive_Label_Id = "_IpLabelId";
        public const string Sensitive_Label_Display_Name = "_DisplayName";
        public const string Sensitive_Label_Full_Name = "Sensitivity";
        public const string SP_ContentType = "ContentTypeId";
    }

    public class RcordsBuiltInColumn
    {
        public const string CONTAINER_BCS_NAME = "RevIM";
        public const string ITEM_BCS_NAME = "RevIMBCS";
        public const string UNIQUEID_NAME = "RevIMUniqueID";
        public const string RecordsRelated = "RecordsRelated";
        public const string RELATED_TENANTID = "RelatedId";
    }
}
