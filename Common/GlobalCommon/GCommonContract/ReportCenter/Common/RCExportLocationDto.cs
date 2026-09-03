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



using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCExportLocationDto : ExportLocationDto
    {
        /// <summary>
        /// 0 unc
        /// 1 sharepoint
        /// </summary>
        [DataMember]
        public int LocationType { get; set; }

        [DataMember]
        public SPDocumentLibraryLocation SPLocation { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Name:" + Name + "\n");
            builder.Append(SPLocation.ToString());
            return builder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPDocumentLibraryLocation
    {
        /// <summary>
        /// 0 specify
        /// 1 default
        /// </summary>
        [DataMember]
        public int SPLibraryURLType { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }

        public string DocumentLibraryURL
        {
            get
            {
                if (SPLibraryURLType == 0)
                {
                    return string.Format("{0}/{1}", WebURL, DocumentLibraryName);
                }
                return DocumentLibraryName;
            }
        }

        /// <summary>
        /// 如果是Each Site模式,该值为空
        /// </summary>
        [DataMember]
        public string WebURL { get; set; }

        [DataMember]
        public string DocumentLibraryName { get; set; }

        /// <summary>
        /// 0 Overwrite
        /// 1 Create New
        /// </summary>
        [DataMember]
        public int Rules { get; set; }

        /// <summary>
        /// 0 default
        /// 1 specify
        /// </summary>
        [DataMember]
        public int FileNamingType { get; set; }

        [DataMember]
        public string FileName { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Document Library Type:" + SPLibraryURLType + "\n");
            return builder.ToString();
        }

        /// <summary>
        /// 包含了选择了导出的Library的Tree
        /// </summary>
        [DataMember]
        public SPTreeNodeDto LibraryTree { get; set; }
    }

    public enum LocationType
    {
        UNC = 0,
        SharePointDocument = 1
    }

    public enum SPLibraryURLType
    {
        Specify = 0,
        Default = 1
    }

    public enum Rules
    {
        Overwrite = 0,
        CreateNew = 1
    }

    public enum FileNamingType
    {
        Default = 0,
        Specify = 1
    }
}
