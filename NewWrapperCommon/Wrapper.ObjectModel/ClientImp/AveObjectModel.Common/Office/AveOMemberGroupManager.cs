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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOMemberGroupManager : AveAbstractCommonCollection<IAveOMemberGroup>, IAveOMemberGroupManager
    {
        private IAveRequest mRequest;

        public AveOMemberGroupManager(IAveRequest request, Dictionary<string, object> memberGroupsProp)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(memberGroupsProp);
            InitMemberGroupManager();
        }

        internal void InitMemberGroupManager()
        {
            List<Dictionary<string, object>> memberGroupList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveOMemberGroup>(memberGroupList.Count);
            foreach (Dictionary<string, object> memberGroupProp in memberGroupList)
            {
                AveOMemberGroup memberGroup = new AveOMemberGroup(memberGroupProp);
                mListData.Add(memberGroup);
            }
        }

        public IAveOMemberGroup CreateMemberGroup(Guid source, string displayName, string mailNickname, string description, string url, string sourceReference)
        {
            throw new NotImplementedException();
        }

        public new long Count
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOMemberGroup this[long id]
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOMemberGroup GetMemberGroupBySourceAndSourceReference(Guid source, string sourceReference)
        {
            throw new NotImplementedException();
        }
    }
}
