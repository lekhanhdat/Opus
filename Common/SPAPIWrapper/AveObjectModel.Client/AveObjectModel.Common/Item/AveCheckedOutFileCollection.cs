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
namespace AvePoint.ObjectModel.Common
{
    using AvePoint.Wrapper.Common;
    using System;
    using System.Collections.Generic;
    class AveCheckedOutFileCollection:AveAbstractCommonCollection<IAveCheckedOutFileCSOM>,IAveCheckedOutFileCollection
    {
        private AveList mParentList;
        public AveCheckedOutFileCollection(AveList parentList,IDictionary<string,object> items)
        {
            mParentList = parentList;
            DataCache.AddPropertyies(items);
            var children = DataCache.GetChildren();
            mListData = new List<IAveCheckedOutFileCSOM>();
            foreach (var properties in children)
            {
                mListData.Add(new AveCheckedOutFileCSOM(parentList, properties));
            }
        }
        public IAveCheckedOutFileCSOM GetByPath(string serverRelativeUrl)
        {
            foreach (var file in mListData)
            {
                if (string.Equals(file.ServerRelativePath, serverRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return default(IAveCheckedOutFileCSOM);
        }
    }
}
