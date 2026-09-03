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
namespace Office365GroupRestore
{
    #region
    using System;
    using System.Collections.Generic;
    #endregion

    public class RestorePropertyTagList
    {
        public static List<TagPropertyDefinition> PropTagList = new List<TagPropertyDefinition>()
         {
            #region Sender
               //PR_SenderSimpleDispName_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x4030001F",DataBlockContent = "3040",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENDER_NAME_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0C1A001F",DataBlockContent = "1A0C",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENDER_ENTRYID
               new TagPropertyDefinition() { PropertyTagKey  = "0x0C190102",DataBlockContent = "190C",PropertyTagValueType = PropertyTagValueType.ByteArray},
               //PR_SENDER_SEARCH_KEY
               new TagPropertyDefinition() { PropertyTagKey  = "0x0C1D0102",DataBlockContent = "1D0C",PropertyTagValueType = PropertyTagValueType.ByteArray},
               //PR_SENDER_EMAIL_ADDRESS_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0C1F001F",DataBlockContent = "1F0C",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENDER_ADDRTYPE_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0C1E001F",DataBlockContent = "1E0C",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SentRepresentingSimpleDispName_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x4031001F",DataBlockContent = "3140",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENT_REPRESENTING_ADDRTYPE_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0064001F",DataBlockContent = "6400",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENT_REPRESENTING_EMAIL_ADDRESS_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0065001F",DataBlockContent = "6500",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENT_REPRESENTING_ENTRYID
               new TagPropertyDefinition() { PropertyTagKey  = "0x00410102",DataBlockContent = "4100",PropertyTagValueType = PropertyTagValueType.ByteArray},
               //PR_SENT_REPRESENTING_NAME_W
               new TagPropertyDefinition() { PropertyTagKey  = "0x0042001F",DataBlockContent = "4200",PropertyTagValueType = PropertyTagValueType.String},
               //PR_SENT_REPRESENTING_SEARCH_KEY
                new TagPropertyDefinition() { PropertyTagKey  = "0x003B0102",DataBlockContent = "3B00",PropertyTagValueType = PropertyTagValueType.ByteArray},
                //PR_SENDER_SID
                new TagPropertyDefinition() { PropertyTagKey  = "0x0E4D0102",DataBlockContent = "4D0E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                //PR_SEND_RICH_INFO
                new TagPropertyDefinition() { PropertyTagKey  = "0x3A40000B",DataBlockContent = "403A",PropertyTagValueType = PropertyTagValueType.Boolean},
                //PR_SENT_REPRESENTING_SID
                new TagPropertyDefinition() { PropertyTagKey  = "0x0E4E0102",DataBlockContent = "4E0E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                 #endregion

            #region Received By
                //PR_RECEIVED_BY_ADDRTYPE_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0075001F",DataBlockContent = "7500",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RECEIVED_BY_EMAIL_ADDRESS_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0076001F",DataBlockContent = "7600",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RECEIVED_BY_ENTRYID
                new TagPropertyDefinition() { PropertyTagKey  = "0x003F0102",DataBlockContent = "3F00",PropertyTagValueType = PropertyTagValueType.ByteArray},
                //PR_RECEIVED_BY_NAME_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0040001F",DataBlockContent = "4000",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RECEIVED_BY_SEARCH_KEY
                new TagPropertyDefinition() { PropertyTagKey  = "0x00510102",DataBlockContent = "5100",PropertyTagValueType = PropertyTagValueType.ByteArray},
                //PR_RcvdBySimpleDispName_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x4034001F",DataBlockContent = "3440",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RcvdRepresentingSimpleDispName_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x4035001F",DataBlockContent = "3540",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RCVD_REPRESENTING_ADDRTYPE_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0077001F",DataBlockContent = "7700",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RCVD_REPRESENTING_EMAIL_ADDRESS_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0078001F",DataBlockContent = "7800",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RCVD_REPRESENTING_ENTRYID
                new TagPropertyDefinition() { PropertyTagKey  = "0x00430102",DataBlockContent = "4300",PropertyTagValueType = PropertyTagValueType.ByteArray},
                //PR_RCVD_REPRESENTING_NAME_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x0044001F",DataBlockContent = "4400",PropertyTagValueType = PropertyTagValueType.String},
                //PR_RCVD_REPRESENTING_SEARCH_KEY
                new TagPropertyDefinition() { PropertyTagKey  = "0x00520102",DataBlockContent = "5200",PropertyTagValueType = PropertyTagValueType.ByteArray},
                #endregion

            #region Conversation
                //PR_CONVERSATION_INDEX
                new TagPropertyDefinition() { PropertyTagKey  = string.Empty,DataBlockContent = "7100",PropertyTagValueType = PropertyTagValueType.String,IsSpecialTagKey = true},
                //PR_CONVERSATION_INDEX_TRACKING
                new TagPropertyDefinition() { PropertyTagKey  = string.Empty,DataBlockContent = "7000",PropertyTagValueType = PropertyTagValueType.String,IsSpecialTagKey = true},
                //PR_CONVERSATION_INDEX_TRACKING
                new TagPropertyDefinition() { PropertyTagKey  = "0x3016000B",DataBlockContent = "1630",PropertyTagValueType = PropertyTagValueType.Boolean},
              #endregion

            #region DataTime
                //PR_CLIENT_SUBMIT_TIME
                new TagPropertyDefinition() { PropertyTagKey  = "0x00390040",DataBlockContent = "3900",PropertyTagValueType = PropertyTagValueType.DateTime},
                //PR_LAST_MODIFICATION_TIME
                new TagPropertyDefinition() { PropertyTagKey  = "0x30080040",DataBlockContent = "0830",PropertyTagValueType = PropertyTagValueType.DateTime},
                //PR_MESSAGE_DELIVERY_TIME
                new TagPropertyDefinition() { PropertyTagKey  = "0x0E060040",DataBlockContent = "060E",PropertyTagValueType = PropertyTagValueType.DateTime},
              #endregion

            #region Creator
                //PR_CreatorAddrType_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x4022001F",DataBlockContent = "2240",PropertyTagValueType = PropertyTagValueType.String},
                //PR_CreatorEmailAddr_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x4023001F",DataBlockContent = "2340",PropertyTagValueType = PropertyTagValueType.String},
                //PR_CreatorFlags
                new TagPropertyDefinition() { PropertyTagKey  = "0x40590003",DataBlockContent = "5940",PropertyTagValueType = PropertyTagValueType.Int64},
                //PR_CreatorSimpleDispName_W
                new TagPropertyDefinition() { PropertyTagKey  = "0x4038001F",DataBlockContent = "3840",PropertyTagValueType = PropertyTagValueType.String},
                 //PR_CreatorGuid
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E4B0102",DataBlockContent = "4B0E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                 //PR_CREATOR_SID
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E580102",DataBlockContent = "580E",PropertyTagValueType = PropertyTagValueType.ByteArray},
              #endregion

            #region TO CC BCC
                 //PR_DISPLAY_BCC_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E02001F",DataBlockContent = "020E",PropertyTagValueType = PropertyTagValueType.String},
                 //PR_DISPLAY_CC_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E03001F",DataBlockContent = "030E",PropertyTagValueType = PropertyTagValueType.String},
                 //PR_DISPLAY_TO_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E04001F",DataBlockContent = "040E",PropertyTagValueType = PropertyTagValueType.String},
                  #endregion

            #region Modifier
                 //PR_LastModifierAddrType_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x4024001F",DataBlockContent = "2440",PropertyTagValueType = PropertyTagValueType.String},
                 //PR_LastModifierEmailAddr_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x4025001F",DataBlockContent = "2540",PropertyTagValueType = PropertyTagValueType.String},
                 //PR_LastModifierFlags
                 new TagPropertyDefinition() { PropertyTagKey  = "0x405A0003",DataBlockContent = "5A40",PropertyTagValueType = PropertyTagValueType.Int64},
                 //PR_LastModifierGuid
                 new TagPropertyDefinition() { PropertyTagKey  = "0x0E4C0102",DataBlockContent = "4C0E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                 //PR_LastModifierSimpleDispName_W
                 new TagPropertyDefinition() { PropertyTagKey  = "0x4039001F",DataBlockContent = "3940",PropertyTagValueType = PropertyTagValueType.String},
                  //PR_LAST_MODIFIER_ENTRYID
                  new TagPropertyDefinition() { PropertyTagKey  = "0x3FFB0102",DataBlockContent = "FB3F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                  //PR_LAST_MODIFIER_NAME_W
                  new TagPropertyDefinition() { PropertyTagKey  = "0x3FFA001F",DataBlockContent = "FA3F",PropertyTagValueType = PropertyTagValueType.String},
                  //PR_LAST_MODIFIER_SID
                  new TagPropertyDefinition() { PropertyTagKey  = "0x0E590102",DataBlockContent = "590E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                 #endregion

            #region Normal
                  //PR_CHANGE_KEY
                  new TagPropertyDefinition() { PropertyTagKey  = "0x65E20102",DataBlockContent = "E265",PropertyTagValueType = PropertyTagValueType.ByteArray},
                  //PR_ENTRYID
                  new TagPropertyDefinition() { PropertyTagKey  = "0x0FFF0102",DataBlockContent = "FF0F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                  //PR_HASATTACH
                  new TagPropertyDefinition() { PropertyTagKey  = "0x0E1B000B",DataBlockContent = "1B0E",PropertyTagValueType = PropertyTagValueType.Boolean},
                  //PR_IMPORTANCE
                  new TagPropertyDefinition() { PropertyTagKey  = string.Empty,DataBlockContent = "1700",PropertyTagValueType = PropertyTagValueType.Int64,IsSpecialTagKey = true},
                  //PR_INTERNET_ARTICLE_NUMBER
                  new TagPropertyDefinition() { PropertyTagKey  = "0x0E230003",DataBlockContent = "230E",PropertyTagValueType = PropertyTagValueType.Int64},
                  //PR_INTERNET_CPID
                  //if null set default value 65001
                   new TagPropertyDefinition() { PropertyTagKey  = "0x3FDE0003",DataBlockContent = "DE3F",PropertyTagValueType = PropertyTagValueType.Int64,DefaultValue =65001 },
                   //PR_INTERNET_MESSAGE_ID_W
                   new TagPropertyDefinition() { PropertyTagKey  = "0x1035001F",DataBlockContent = "3510",PropertyTagValueType = PropertyTagValueType.String},
                   //PR_MAPPING_SIGNATURE
                   new TagPropertyDefinition() { PropertyTagKey  = "0x0FF80102",DataBlockContent = "F80F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                   //PR_MDB_PROVIDER
                   new TagPropertyDefinition() { PropertyTagKey  = "0x34140102",DataBlockContent = "1434",PropertyTagValueType = PropertyTagValueType.ByteArray},
                   //PR_MESSAGE_CC_ME
                   new TagPropertyDefinition() { PropertyTagKey  = "0x0058000B",DataBlockContent = "5800",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_MESSAGE_CLASS_W
                    new TagPropertyDefinition() { PropertyTagKey  = "0x001A001F",DataBlockContent = "1A00",PropertyTagValueType = PropertyTagValueType.String,IsSpecialTagKey = true},
                    //PR_MESSAGE_CODEPAGE
                    new TagPropertyDefinition() { PropertyTagKey  = "0x3FFD0003",DataBlockContent = "FD3F",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_MESSAGE_FLAGS
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E070003",DataBlockContent = "070E",PropertyTagValueType = PropertyTagValueType.Int64,IsSpecialTagKey = true},
                    //PR_MESSAGE_LOCALE_ID
                    new TagPropertyDefinition() { PropertyTagKey  = "0x3FF10003",DataBlockContent = "F13F",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_MESSAGE_TO_ME
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0057000B",DataBlockContent = "5700",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_NON_RECEIPT_NOTIFICATION_REQUESTED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0C06000B",DataBlockContent = "060C",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_OBJECT_TYPE
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0FFE0003",DataBlockContent = "FE0F",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_ORIGINATOR_DELIVERY_REPORT_REQUESTED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0023000B",DataBlockContent = "2300",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_PARENT_DISPLAY_W
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E05001F",DataBlockContent = "050E",PropertyTagValueType = PropertyTagValueType.String},
                    //PR_PARENT_ENTRYID
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E090102",DataBlockContent = "090E",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_PREDECESSOR_CHANGE_LIST
                    new TagPropertyDefinition() { PropertyTagKey  = "0x65E30102",DataBlockContent = "E365",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_PRIORITY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x00260003",DataBlockContent = "2600",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_READ_RECEIPT_REQUESTED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0029000B",DataBlockContent = "2900",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_RECORD_KEY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0FF90102",DataBlockContent = "F90F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_RTF_IN_SYNC
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E1F000B",DataBlockContent = "1F0E",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_SEARCH_KEY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x300B0102",DataBlockContent = "0B30",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_SENSITIVITY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x00360003",DataBlockContent = "3600",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_STORE_ENTRYID
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0FFB0102",DataBlockContent = "FB0F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_STORE_RECORD_KEY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0FFA0102",DataBlockContent = "FA0F",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_STORE_SUPPORT_MASK
                    new TagPropertyDefinition() { PropertyTagKey  = "0x340D0003",DataBlockContent = "0D34",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_STORE_UNICODE_MASK
                    new TagPropertyDefinition() { PropertyTagKey  = "0x340F0003",DataBlockContent = "0F34",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_SUBJECT_PREFIX_W
                    new TagPropertyDefinition() { PropertyTagKey  = "0x003D001F",DataBlockContent = "3D00",PropertyTagValueType = PropertyTagValueType.String},
                    //PR_ICON_INDEX
                    new TagPropertyDefinition() { PropertyTagKey  = "0x10800003",DataBlockContent = "8010",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_ALTERNATE_RECIPIENT_ALLOWED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0002000B",DataBlockContent = "0200",PropertyTagValueType = PropertyTagValueType.Boolean},
                  #endregion

            #region Others
                    //PR_START_DATE
                    new TagPropertyDefinition() { PropertyTagKey  = "0x00600040",DataBlockContent = "6000",PropertyTagValueType = PropertyTagValueType.DateTime},
                    //PR_END_DATE
                    new TagPropertyDefinition() { PropertyTagKey  = "0x00610040",DataBlockContent = "6100",PropertyTagValueType = PropertyTagValueType.DateTime},
                    //PR_MSG_EDITOR_FORMAT
                    new TagPropertyDefinition() { PropertyTagKey  = "0x59090003",DataBlockContent = "0959",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_DELETE_AFTER_SUBMIT
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E01000B",DataBlockContent = "010E",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_INETMAIL_OVERRIDE_FORMAT
                    new TagPropertyDefinition() { PropertyTagKey  = "0x59020003",DataBlockContent = "0259",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_NATIVE_BODY_INFO
                    new TagPropertyDefinition() { PropertyTagKey  = "0x59090003",DataBlockContent = "0959",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_ORIGINAL_SENSITIVITY
                    new TagPropertyDefinition() { PropertyTagKey  = "0x002E0003",DataBlockContent = "2E00",PropertyTagValueType = PropertyTagValueType.Int64},
                    //PR_Preview_W
                    new TagPropertyDefinition() { PropertyTagKey  = "0x3FD9001F",DataBlockContent = "D93F",PropertyTagValueType = PropertyTagValueType.String},
                    //PR_READ
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0E69000B",DataBlockContent = "690E",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_RECIPIENT_REASSIGNMENT_PROHIBITED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x002B000B",DataBlockContent = "2B00",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_REPLY_REQUESTED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0C17000B",DataBlockContent = "170C",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_RESPONSE_REQUESTED
                    new TagPropertyDefinition() { PropertyTagKey  = "0x0063000B",DataBlockContent = "6300",PropertyTagValueType = PropertyTagValueType.Boolean},
                    //PR_TARGET_ENTRYID
                    new TagPropertyDefinition() { PropertyTagKey  = "0x30100102",DataBlockContent = "1030",PropertyTagValueType = PropertyTagValueType.ByteArray},
                    //PR_ORIGINAL_AUTHOR_NAME_W
                    new TagPropertyDefinition() { PropertyTagKey  = "0x004D001F",DataBlockContent = "4D00",PropertyTagValueType = PropertyTagValueType.String},
                    #endregion
         };
    }

    public class TagPropertyDefinition
    {
        private String propertyTagKey;
        public String PropertyTagKey
        {
            get { return propertyTagKey; }
            set { propertyTagKey = value; }
        }
        private String dataBlockContent;
        public String DataBlockContent
        {
            get { return dataBlockContent; }
            set { dataBlockContent = value; }
        }

        private object defaultValue;
        public object DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        public bool IsSpecialTagKey { get; set; } = false;

        private PropertyTagValueType propertyTagValueType;
        public PropertyTagValueType PropertyTagValueType
        {
            get { return propertyTagValueType; }
            set { propertyTagValueType = value; }
        }
    }
    public enum PropertyTagValueType
    {
        String,
        ByteArray,
        Boolean,
        DateTime,
        Int64,
        Others
    }
}