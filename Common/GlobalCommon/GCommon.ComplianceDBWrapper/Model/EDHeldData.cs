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
using AvePoint.GCommon.ComplianceDBWrapper.Model;

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDHeldData
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        #region - UniqueID 是指在同源情况下的唯一ID,包含Version ,以及lastModifyTime -

        private string _uniqueID;

        public string UniqueID 
        { 
            get
            {
                switch (DataSource)
                {
                        case DataSource.SharePoint:
                            //对于SharePoint Data的UniqueID生成规则不同.
                            _uniqueID = IsCurrent ? (""+FarmID + SPGuid).ToMD5() : (""+FarmID + SPGuid + Version).ToMD5();
                            break;
                        case DataSource.Archive:
                            _uniqueID = (FarmID + DisplayURL + SubJobID).ToMD5();
                            break;
                }
                return _uniqueID;
            }
            set { _uniqueID = value; }
        }

        #endregion

        public string DisplayURL { get; set; }

        public string FileURL { get; set; }

        public string MetaDataURL { get; set; }

        public DataSource DataSource { get; set; }

        public DataType DataType { get; set; }

        public long Size { get; set; }

        public string CreateBy { get; set; }

        public MarkState MarkState { get; set; }

        public bool IsCurrent { get; set; }

        public string Version { get; set; }

        public DateTime ModifiedTime { get; set; }

        public EDStorageInfo ContentStorageInfo { get; set; }

        public EDStorageInfo MetadataStorageInfo { get; set; }

        public string DeviceID { get; set; }

        public Guid SPGuid { get; set; }

        public string FarmID { get; set; }

        public Guid WebAppID { get; set; }

        public Guid SiteID { get; set; }

        public Guid WebID { get; set; }

        public Guid ListID { get; set; }

        [Obsolete("已经废弃不要用啊")]
        public string FarmName { get; set; }

        [Obsolete("已经废弃不要用啊")]
        public string JobID { get; set; }

        public string SubJobID { get; set; }

        public string SiteURL { get; set; }

        public string PathMD5 { get; set; }

    }

    public enum DataSource
    {
        SharePoint = 0,
        Archive = 1
    }

    public enum DataType
    {
        Document = 0,
        Item = 1,
        Attachment=2,
        HeldData=3
    }
}
