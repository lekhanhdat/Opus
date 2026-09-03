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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASOExportLocationDto : ExportLocationDto
    {
        /// <summary>
        /// 0 unc
        /// 1 sharepoint
        /// </summary>
        [DataMember]
        public int CASOLocationType { get; set; }

        [DataMember]
        public CASOSPDocumentLibraryLocation SPLocation { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Name:" + Name + "\n");
            builder.Append("Location Type" + CASOLocationType + "\n");
            if (CASOLocationType == 0)
            {
                builder.Append("Username" + UserName + "\n");
                builder.Append("Path" + Path + "\n");
            }
            else
            {
                builder.Append(SPLocation.ToString());
            }
            //ADO-131868
            return this.Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASOSPDocumentLibraryLocation
    {
        /// <summary>
        /// 0 specify
        /// 1 each site
        /// </summary>
        [DataMember]
        public int SPLibraryURLType { get; set; }

        [DataMember]
        public SPTreeNodeDto FarmNode { get; set; }

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
        /// 0 Library
        /// 1 Folder
        /// </summary>
        [DataMember]
        public int DestinationType { get; set; }

        [DataMember]
        public Guid FolderId { get; set; }

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
            if (SPLibraryURLType == 0)
            {
                builder.Append("Farm Name:" + FarmNode.Name + "\n");
                builder.Append("Document Library:" + string.Format("{0}/{1}", WebURL, DocumentLibraryName) + "\n");
            }
            else
            {
                builder.Append("Document Library Name:" + DocumentLibraryName + "\n");
            }
            builder.Append("Rules:" + Rules + "\n");
            builder.Append("File Name Type:" + FileNamingType + "\n");
            builder.Append("File Name:" + FileName + "\n");
            return builder.ToString();
        }
    }

    public enum CASOLocationType
    {
        UNC = 0,
        SharePointDocument = 1
    }

    public enum CASOSPLibraryURLType
    {
        Specify = 0,
        Default = 1
    }

    public enum CASORules
    {
        Overwrite = 0,
        CreateNew = 1
    }

    public enum CASOFileNamingType
    {
        Default = 0,
        Specify = 1
    }
}
