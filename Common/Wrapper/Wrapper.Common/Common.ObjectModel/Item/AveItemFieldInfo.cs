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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveItemFieldInfo
    {       
        private string mDisplayName;
        private string mType;
        private string mStaticName;
        private object mValue;

        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
            set
            {
                mDisplayName = value;
            }
        }

        public string Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }

        public string StaticName
        {
            get
            {
                return mStaticName;
            }
            set
            {
                mStaticName = value;
            }
        }

        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        public AveItemFieldInfo(string displayName, string type, object value)
        {
            this.mDisplayName = displayName;
            this.mType = type;
            this.mValue = value;
        }

        public AveItemFieldInfo(string staticName, object value)        {
            this.mStaticName = staticName;            
            this.mValue = value;            
        }

        public AveItemFieldInfo(string displayName, string staticName, string type, object value) : this(displayName, type, value)
        {        
            this.mStaticName = staticName;         
        }

        public AveItemFieldInfo() { }
    }

    public class AveItemFieldCollectionInfo
    {
        public List<AveItemFieldInfo> ItemFields = new List<AveItemFieldInfo>();
        public AveListItemType ItemType;
        public string SrcUrl;
        public string ServerRelativeUrl;
        public string Version;
        public string CheckinComment; //only for migration to restore checkin comment.
        public string OriginalName;
        public int VersionMode;
        public long Size;
        public string UniqueId;
        public bool CreateIfNoneExist;
        public bool IsDestFolder;

        public AveItemFieldInfo GetUniqueItemFieldInfoByDisplayName(string displayName)
        {
            IList<AveItemFieldInfo> itemFieldInfoList = GetSpecialMembersFromList(ItemFields, "DisplayName", displayName);
            if (itemFieldInfoList != null && itemFieldInfoList.Count > 0)
            {
                return itemFieldInfoList[0];
            }
            else
            {
                return null;
            }
        }
        
        public AveItemFieldInfo GetUniqueItemFieldInfoByStaticName(string staticName)
        {
            IList<AveItemFieldInfo> itemFieldInfoList = GetSpecialMembersFromList(ItemFields, "StaticName", staticName);
            if (itemFieldInfoList != null && itemFieldInfoList.Count > 0)
            {
                return itemFieldInfoList[0];
            }
            else
            {
                return null;
            }
        }


        private IList<AveItemFieldInfo> GetSpecialMembersFromList(List<AveItemFieldInfo> sourceList, string keyword, string keyvalue)
        {
            List<AveItemFieldInfo> retList = new List<AveItemFieldInfo>();

            foreach (AveItemFieldInfo itemFieldInfo in sourceList)
            {
                switch (keyword)
                {
                    case "DisplayName":
                        if (itemFieldInfo.DisplayName.Equals(keyvalue))
                        {
                            retList.Add(itemFieldInfo);
                        }
                        break;
                    case "StaticName":
                        if (itemFieldInfo.StaticName.Equals(keyvalue))
                        {
                            retList.Add(itemFieldInfo);
                        }
                        break;
                }
            }

            return retList;
        }
    }
}
