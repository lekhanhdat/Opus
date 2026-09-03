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
    public interface IAveContentTypeCollection : IEnumerable<IAveContentType>, ICollection
    {
        IAveContentType this[string name] { get; }
        IAveContentType this[IAveContentTypeId contentTypeId] { get; }
        IAveContentType this[int index] { get; }
        IAveWeb Web { get; }
        IAveList List { get; }
        bool IsDirty { get; set; }

        IAveContentType Add(AveContentTypeCreationInformation contentTypeCreationInfo);
        IAveContentType Add(IAveContentType contentType);
        IAveContentType AddContentType(IAveContentType contentType, bool updateResourceFileProperty, bool checkName, bool setNextChildByte);
        IAveContentType AddExistingContentType(IAveContentType contentType);
        void AddSitePolicy(string policySchema, string siteUrl);
        IAveContentTypeId BestMatch(IAveContentTypeId contentTypeId);
        IAveContentType GetById(string contentTypeId);
        AveContentTypeCollectionInfo GetContentTypeInfos(bool backupParent);
        List<AveContentTypeFileInfo> GetResources(Guid siteId, string folderUrl);
        string GetContentTypeName(Guid siteId, byte[] contentTypeId);
        List<byte[]> GetParentContentTypeIdList(string id);

        AveContentTypeCollectionInfo GetContentTypeInfos(Guid listId, Guid webId, Guid siteId, string scope, bool backupParent);
        
        AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope, bool backupParent);

        bool CheckContentTypeExist(Guid siteId, string ctId);
        bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, string ctId);

        void Update();

        Hashtable DictId { get; }

        AveContentTypeCollectionInfo GetContentTypeInfos(List<string> names, Guid siteId, string scope, bool backupParent);
    }

    public interface IAveContentTypeId : IComparable
    {
        IAveContentTypeId Parent { get; }
        string TypeId { get; }
        bool IsChildOf(IAveContentTypeId id);
        IAveContentTypeId Empty{get;}
        int Length { get; }
        byte[] ToByteArray();
    }

    public class AveContentTypeCreationInformation
    {
        private string mdescription;
        private string mgroup;
        private string mname;
        private IAveContentType mparentContentType;

        public string Description
        {
            get
            {
                return this.mdescription;
            }
            set
            {
                this.mdescription = value;
            }
        }

        public string Group
        {
            get
            {
                return this.mgroup;
            }
            set
            {
                this.mgroup = value;
            }
        }

        public string Name
        {
            get
            {
                return this.mname;
            }
            set
            {
                this.mname = value;
            }
        }

        public IAveContentType ParentContentType
        {
            get
            {
                return this.mparentContentType;
            }
            set
            {
                this.mparentContentType = value;
            }
        }
    }
}
