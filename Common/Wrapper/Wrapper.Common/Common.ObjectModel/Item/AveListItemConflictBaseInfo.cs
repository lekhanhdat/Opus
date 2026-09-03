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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    public class AveListItemConflictBaseInfo
    {
        private string _webServerRelativeUrl;
        protected IAveRequest mRequest;
        private AveFileLevel _level;
        private DateTime _modified;
        private DateTime _created;
        private int _author;
        private int _editor;
        private string _serverRelativeUrl;
        private Guid _uniqueId;
        private Guid _guid;
        private AveFileSystemObjectType _type;
        private Guid _parentId;
        private int _id;

        public AveListItemConflictBaseInfo(IAveRequest request, string webServerRelativeUrl, Dictionary<string, object> itemProperties)
        {
            _webServerRelativeUrl = webServerRelativeUrl;
            mRequest = request;
            if (itemProperties != null)
            {
                if (itemProperties.ContainsKey("LEVEL"))
                {
                    _level = (AveFileLevel)itemProperties["LEVEL"];
                }
                if(itemProperties.ContainsKey("TimeLastModified"))
                {
                    _modified = (DateTime)itemProperties["TimeLastModified"];
                }
                if (itemProperties.ContainsKey("ServerRelativeUrl"))
                {
                    _serverRelativeUrl = (string)itemProperties["ServerRelativeUrl"];
                }
                if (itemProperties.ContainsKey("UniqueId"))
                {
                    _uniqueId = (Guid)itemProperties["UniqueId"];
                }
                if(itemProperties.ContainsKey("FileSystemObjectType"))
                {
                    _type = (AveFileSystemObjectType)Enum.Parse(typeof(AveFileSystemObjectType),Convert.ToString(itemProperties["FileSystemObjectType"]));
                }
                if(itemProperties.ContainsKey("ParentUniqueId"))
                {
                    Guid.TryParse(itemProperties["ParentUniqueId"].ToString(), out _parentId);
                }
                if (itemProperties.ContainsKey("ID"))
                {
                    Int32.TryParse(itemProperties["ID"].ToString(), out _id);
                }
                if(itemProperties.ContainsKey("GUID"))
                {
                    Guid.TryParse(itemProperties["GUID"].ToString(), out _guid);
                }
                if (itemProperties.ContainsKey("TimeCreated"))
                {
                    _created = (DateTime)itemProperties["TimeCreated"];
                }
                if (itemProperties.ContainsKey("Author"))
                {
                    Int32.TryParse(itemProperties["Author"].ToString(), out _author);
                }
                if (itemProperties.ContainsKey("Editor"))
                {
                    Int32.TryParse(itemProperties["Editor"].ToString(), out _editor);
                }
            }
        }

        //public AveListItemConflictBaseInfo(GraphListItem listItem)
        //{
        //    _level = (AveFileLevel)Enum.Parse(typeof(AveFileLevel), listItem.Fields.AdditionalData["_Level"].ToString());
        //    _type = (AveFileSystemObjectType)Enum.Parse(typeof(AveFileSystemObjectType), listItem.Fields.AdditionalData["FSObjType"].ToString());
        //    _modified = listItem.LastModifiedDateTime.Value.UtcDateTime;
        //    _created = listItem.CreatedDateTime.Value.UtcDateTime;
        //    Guid.TryParse(listItem.Fields.AdditionalData["UniqueId"].ToString(), out _uniqueId);
        //    Int32.TryParse(listItem.Id, out _id);
        //    Guid.TryParse(listItem.Fields.AdditionalData["GUID"].ToString(), out _guid);
        //    Int32.TryParse(listItem.Fields.AdditionalData["AuthorLookupId"].ToString(), out _author);
        //    Int32.TryParse(listItem.Fields.AdditionalData["EditorLookupId"].ToString(), out _editor);
        //    Guid.TryParse(listItem.Fields.AdditionalData["ParentUniqueId"].ToString(), out _parentId);
        //    _serverRelativeUrl = (string)listItem.Fields.AdditionalData["FileRef"];
        //}

        public int ID
        {
            get
            {
                return _id;
            }
        }
        public Guid ParentUniqueId
        {
            get
            {
                return _parentId;
            }
        }
        public AveFileSystemObjectType ObjectType
        {
            get
            {
                return _type;
            }
        }
        public AveFileLevel Level
        {
            get
            {
                return _level;
            }
        }

        public DateTime Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                _modified = value;
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return _serverRelativeUrl;
            }
        }

        public string WebServerRelativeUrl
        {
            get
            {
                return _webServerRelativeUrl;
            }
        }

        public Guid UniqueId
        {
            get
            {
                return _uniqueId;
            }
        }
        public Guid GUID
        {
            get 
            {
                return _guid; 
            }
        }
        public IAveRequest AveRequest
        {
            get
            {
                return mRequest;
            }
        }

        public DateTime TimeCreated
        {
            get
            {
                return _created;
            }
            set
            {
                _created = value;
            }
        }

        public int Author
        {
            get
            {
                return _author;
            }
        }

        public int Editor
        {
            get
            {
                return _editor;
            }
        }

        public void CheckIn(string comment)
        {
            this.CheckIn(comment, AveCheckinType.MinorCheckIn);
        }

        public void CheckIn(string comment, AveCheckinType checkinType)
        {
            this.mRequest.CheckIn(_webServerRelativeUrl, this.ServerRelativeUrl, comment, (int)checkinType);
        }
    }
}
