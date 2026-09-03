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

    #endregion using directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextTailInfo
    {
        [DataMember]
        public String VersionComment { get; set; }

        [DataMember]
        public String CreatedByDisplayName { get; set; }

        [DataMember]
        public String CreatedByLoginName { get; set; }

        [DataMember]
        public String ModifiedByDisplayName { get; set; }

        [DataMember]
        public String ModifiedByLoginName { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public String ArchiveBy { get; set; }

        [DataMember]
        public DateTime ArchiveTime { get; set; }

        [DataMember]
        public Int32 Size { get; set; }

        [DataMember]
        public String ContentTypeName { get; set; }

        [DataMember]
        public String TimeZoneInfoId { get; set; }

        [DataMember]
        public List<String> Attachments { get; set; }

        [DataMember]
        public Dictionary<String, Object> ColumnValues { get; set; }

        [DataMember]
        public SerializableDictionary FullTextIndexColumnValues { get; set; }

        public static String ToString(Dictionary<String, Object> ColumnValues)
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append("{");
            foreach (KeyValuePair<string, Object> entry in ColumnValues)
            {
                string name = entry.Key;
                buffer.Append("name=").Append(name).Append(",");
                buffer.Append("value=").Append(entry.Value).Append(";");
            }
            buffer.Append("}").Append('\n');
            return buffer.ToString();
        }

        public override String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("CreatedByDisplayName: ").Append(CreatedByDisplayName).Append('\n');
            buf.Append("CreatedByLoginName: ").Append(CreatedByLoginName).Append('\n');
            buf.Append("ModifiedByDisplayName: ").Append(ModifiedByDisplayName).Append('\n');
            buf.Append("ModifiedByLoginName: ").Append(ModifiedByLoginName).Append('\n');
            buf.Append("Created: ").Append(Created.ToString()).Append('\n');
            buf.Append("Modified: ").Append(Modified.ToString()).Append('\n');
            buf.Append("ArchiveBy: ").Append(ArchiveBy).Append('\n');
            buf.Append("ArchiveTime: ").Append(ArchiveTime.ToString()).Append('\n');
            buf.Append("TimeZoneInfoId: ").Append(TimeZoneInfoId).Append('\n');
            buf.Append("Size: ").Append(Size).Append('\n');
            buf.Append("ContentTypeName: ").Append(ContentTypeName).Append('\n');
            if (FullTextIndexColumnValues != null)
            {
                buf.Append("FullTextIndexColumnValues: ").Append(ToString(FullTextIndexColumnValues));
            }
            if (Attachments != null)
            {
                buf.Append("Attachments: ").Append(Arrays.ToString(Attachments.ToArray()));
            }
            return buf.ToString();
        }
    }

    [Serializable]
    public class SerializableDictionary : Dictionary<String, Object>
    {
        public SerializableDictionary() { }

        public static SerializableDictionary ConvertToSerializableDictionary(Dictionary<String, Object> dic)
        {
            SerializableDictionary myDic = new SerializableDictionary();
            foreach (KeyValuePair<String, Object> pair in dic)
            {
                myDic[pair.Key] = pair.Value;
            }
            return myDic;
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}