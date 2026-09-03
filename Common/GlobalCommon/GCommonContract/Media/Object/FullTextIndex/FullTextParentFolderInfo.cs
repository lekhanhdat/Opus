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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextParentFolderInfo
    {
        [DataMember]
        public Int64 ArchiveTime { get; set; }

        [DataMember]
        public String Type { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public Int64 DataFileNumber { get; set; }

        [DataMember]
        public Int64 DataFileOffset { get; set; }

        [DataMember]
        public String Attributes { get; set; }

        [DataMember]
        public String ParentMD5 { get; set; }

        [DataMember]
        public String Permission { get; set; }

        [DataMember]
        public String PathMD5 { get; set; }

        [DataMember]
        public Boolean IsHit { get; set; }

        [DataMember]
        public String IsSystemFolder { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Full Text Parent Folder Info: ");
            stringBuilder.AppendFormat("Type: {0}, ", this.Type);
            stringBuilder.AppendFormat("Name: {0}, ", this.Name);
            stringBuilder.AppendFormat("Permission: {0}, ", this.Permission);
            stringBuilder.AppendFormat("IsHit: {0}", this.IsHit);
            return stringBuilder.ToString();
        }
    }
}
