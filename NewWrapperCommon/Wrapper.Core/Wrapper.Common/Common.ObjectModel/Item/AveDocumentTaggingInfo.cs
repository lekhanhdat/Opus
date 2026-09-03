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
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveSocialTagInfo
    {
        public string ProfileManagerUrl;

        public string Url;
        public string Title;
        public string Owner;
        public bool IsPrivate;
        public DateTime LastModifiedTime;

        public TagExtention tagExtention;

        //for term
        public AveTermInfo Term;

        public AveSocialTagInfo()
        {
        }
    }

    public class AveDocumentTaggingInfo
    {
        public string Url;
        public string Title;
        public string Owner;
        public bool IsPrivate;
        public string TermOwner;
        //for term
        public AveTermInfo Term;

        public AveDocumentTaggingInfo()
        {
        }
    }

    public class TagExtention
    {
        [DataMember]
        public string TermStoreName;
        [DataMember]
        public Guid TermStoreId;
        [DataMember]
        public string TermGroupName;
        [DataMember]
        public Guid TermGroupId;
        [DataMember]
        public string ParentTermSetName;
    }

    public class AveSocialCommentInfo
    {
        public string ProfileManagerUrl;

        public string Url;
        public string Comment;
        public string Owner;
        public bool IsHighPriority;
        public string Title;
        public DateTime LastModifiedTime;

        public AveSocialCommentInfo()
        {
        }
    }
}
