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
using AvePoint.Wrapper.Common;
using System.Collections;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMemberGroup : AveAbstractCommonCollection, IAveOMemberGroup
    {
        private MemberGroup mMemberGroup;
        private AveOMembershipManager mMembershipManager;

        public AveOMemberGroup(AveOMembershipManager membershipManager, MemberGroup memberGroup)
            : base(memberGroup)
        {
            mMembershipManager = membershipManager;
            mMemberGroup = memberGroup;
        }

        internal MemberGroup MemberGroup
        {
            get
            {
                return mMemberGroup;
            }
        }

        #region IAveMemberGroup Members

        public long Count
        {
            get
            {
                return mMemberGroup.Count;
            }
        }

        public string Description
        {
            get
            {
                return mMemberGroup.Description;
            }
            set
            {
                mMemberGroup.Description = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return mMemberGroup.DisplayName;
            }
            set
            {
                mMemberGroup.DisplayName = value;
            }
        }

        public long Id
        {
            get
            {
                return mMemberGroup.Id;
            }
        }

        public DateTime LastUpdate
        {
            get
            {
                return mMemberGroup.LastUpdate;
            }
        }

        public string MailNickName
        {
            get
            {
                return mMemberGroup.MailNickName;
            }
            set
            {
                mMemberGroup.MailNickName = value;
            }
        }

        public Uri PublicUrl
        {
            get
            {
                return mMemberGroup.PublicUrl;
            }
        }

        public AveMembershipSource Source
        {
            get
            {
                return (AveMembershipSource)mMemberGroup.Source;
            }
        }

        public Guid SourceInternal
        {
            get
            {
                return mMemberGroup.SourceInternal;
            }
        }

        public string SourceReference
        {
            get
            {
                return mMemberGroup.SourceReference;
            }
            set
            {
                mMemberGroup.SourceReference = value;
            }
        }

        public string Url
        {
            get
            {
                return mMemberGroup.Url;
            }
            set
            {
                mMemberGroup.Url = value;
            }
        }

        #endregion

        internal override object CreatElementInstance(object obj)
        {
            return new AveOMembership(mMembershipManager, (Membership)obj);
        }
    }
}
