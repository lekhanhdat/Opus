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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveListItemCollection : ICollection, IEnumerable<IAveListItem>, IEnumerable
    {
        IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName);
        IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName, int rowId);
        IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType);
        IAveListItem Add();
        void Delete(int index);
        IAveListItem GetById(int id);
        IAveListItem GetById(string id);

        IAveListItem this[int index] { get; }
        IAveListItem this[Guid id] { get; }
        IAveList List { get; }
        IAveListItemCollectionPosition ListItemCollectionPosition { get; }
    }

    public interface IAveListItemCollectionPosition
    {
        string PagingInfo { get; set; }
    }

    public interface IAveFieldStringValues
    {
        string[] FieldNames { get; }
        string GetFieldValue(string fieldName);
    }

    public class AveItemCreationInformation
    {
        private string mfolderUrl;
        private string mleafName;
        private AveFileSystemObjectType munderlyingObjectType;

        public string FolderUrl
        {
            get
            {
                return this.mfolderUrl;
            }
            set
            {
                this.mfolderUrl = value;
            }
        }

        public string LeafName
        {
            get
            {
                return this.mleafName;
            }
            set
            {
                this.mleafName = value;
            }
        }

        public AveFileSystemObjectType UnderlyingObjectType
        {
            get
            {
                return this.munderlyingObjectType;
            }
            set
            {
                this.munderlyingObjectType = value;
            }
        }

    }

    public enum AveFileSystemObjectType
    {
        File = 0,
        Folder = 1,
        Invalid = -1,
        Web = 2
    }
}
