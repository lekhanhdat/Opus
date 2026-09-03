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


using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server13
{
    class AveTaxonomyGroupCollection : AveAbstractCommonCollection<IAveTaxonomyGroup>, IAveTaxonomyGroupCollection
    {
        private GroupCollection mGroups;
        public AveTaxonomyGroupCollection(GroupCollection groups)
            : base(groups)
        {
            mGroups = groups;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveTaxonomyGroup(t as Group);
        }

        public override int Count
        {
            get
            {
                return mGroups.Count;
            }
        }

        #region IAveTaxonomyGroupCollection Members

        public override IAveTaxonomyGroup this[int index]
        {
            get
            {
                Group group = mGroups[index];
                if (group == null)
                {
                    return null;
                }
                return new AveTaxonomyGroup(group);
            }
        }

        public IAveTaxonomyGroup this[string groupName]
        {
            get
            {
                return new AveTaxonomyGroup(mGroups[groupName]);
            }
        }

        public IAveTaxonomyGroup this[Guid groupId]
        {
            get
            {
                return new AveTaxonomyGroup(mGroups[groupId]);
            }
        }

        #endregion
    }
}
