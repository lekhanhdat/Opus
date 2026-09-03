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
    using System.Collections.Generic;
    class AveCheckedOutFileCSOM :AveClientObject,IAveCheckedOutFileCSOM
    {
        private AveList mParentList;
        public AveCheckedOutFileCSOM(AveList parentList,IDictionary<string,object> properties)
        {
            mParentList = parentList;
            DataCache.AddPropertyies(properties);
        }

        public int CheckedOutById
        {
            get { return base.DataCache.GetProperty<int>("CheckedOutById"); }
        }

        public string DirName
        {
            get
            {
                EnsureDirNameAndLeafName();
                return base.DataCache.GetProperty<string>("DirName");
            }
        }

        private void EnsureDirNameAndLeafName()
        {
            if (DataCache.IsPropertyAvailable("DirName") && DataCache.IsPropertyAvailable("LeafName"))
            {
                return;
            }
            string path = ServerRelativePath;
            if (!string.IsNullOrEmpty(path))
            {
                string fileName = path.Substring(path.LastIndexOf('/') + 1);
                string dirName = path.Substring(0, path.Length - fileName.Length - 1);
                DataCache.AddProperty("DirName", dirName);
                DataCache.AddProperty("LeafName", fileName);
            }
        }

        public string LeafName
        {
            get
            {
                EnsureDirNameAndLeafName();
                return base.DataCache.GetProperty<string>("LeafName");
            }
        }

        public string ServerRelativePath
        {
            get { return base.DataCache.GetProperty<string>("ServerRelativePath"); }
        }

        public IAveUser CheckedOutBy//attention!
        {
            get
            {
                return mParentList.ParentWeb.SiteUsers.GetByID(CheckedOutById);
            }
        }

        public void TakeOverCheckOut()
        {
            var props=(mParentList.Request as IAveRequest).TakeOverCheckOut(mParentList.ParentWeb.ServerRelativeUrl,mParentList.ID,ServerRelativePath);
            DataCache.AddPropertyies(props);
        }
    }
}
