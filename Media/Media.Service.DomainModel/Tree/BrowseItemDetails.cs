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




namespace AvePoint.Media.Service.DomainModel
{
    #region directives

    using System;
    using System.Collections.Generic;

    #endregion directives

    public class BrowseItemDetails
    {
        public String Id { get; set; }

        public String FileName { get; set; }

        public String FileAttribute { get; set; }

        public Int32 HoldStatus { get; set; }

        public Int64 LastModifiedTime { get; set; }

        public String Path { get; set; }

        public String Title { get; set; }

        public String Author { get; set; }

        public Int64 CreatedTime { get; set; }

        public Int64 ModifiedTime { get; set; }

        public String Version { get; set; }

        public BrowseFileType FileType { get; set; }

        public String Body { get; set; }

        public Int64 Expires { get; set; }

        public String SharepointType { get; set; }

        public String Permission { get; set; }

        public Int64 VaultedTime { get; set; }

        public Int32 CheckIn { get; set; }

        public String ContentType { get; set; }

        public String FullPath { get; set; }

        public String Attachment { get; set; }

        public Dictionary<String, String> CustomizedColumes { get; set; }

        public override string ToString()
        {
            return string.Format("BrowseItemDetails : Path : {0}, FileName: {1}",
                Path, FileName);
        }
    }
}