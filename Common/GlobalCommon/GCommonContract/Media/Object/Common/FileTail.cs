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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileTail
    {
        [DataMember]
        public List<String> Attributes { get; set; }

        [DataMember]
        public String DetailInfoAttributes { get; set; }

        [DataMember]
        public Nullable<Int64> Crc32 { get; set; }

        [DataMember]
        public String ExtraInfo { get; set; }

        [DataMember]
        public Int64 Length { get; set; }

        [DataMember]
        public Boolean IsFailed { get; set; }

        [DataMember]
        public Boolean IsSystemFile { get; set; }

        [DataMember]
        public String CreatedBy { get; set; }

        [DataMember]
        public Int64 CreatedTime { get; set; }

        [DataMember]
        public String ModifiedBy { get; set; }

        [DataMember]
        public Int64 ContentSize { get; set; }

        [DataMember]
        public String Crc64 { get; set; }

        public FileTail()
        {
            IsFailed = false;
            IsSystemFile = false;
        }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("File Tail: ");
            stringBuilder.AppendFormat("Detail Info Attributes: {0}, ", this.DetailInfoAttributes);
            stringBuilder.AppendFormat("Extra Info: {0}, ", this.ExtraInfo);
            stringBuilder.AppendFormat("Is Failed: {0}, ", this.IsFailed);
            stringBuilder.AppendFormat("Is System File: {0}", this.IsSystemFile);
            return stringBuilder.ToString();
        }
    }
}
