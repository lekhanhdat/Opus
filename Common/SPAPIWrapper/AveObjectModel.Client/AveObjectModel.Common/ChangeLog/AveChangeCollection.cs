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
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveChangeCollection : AveAbstractCommonCollection<IAveChange>, IAveChangeCollection
    {
        public AveChangeCollection(Dictionary<string, object> changeCollectionProperties) 
        {
            this.DataCache.AddPropertyies(changeCollectionProperties);
            InitChildrenChange();
        }

        private void InitChildrenChange()
        {
            var ChangePropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveChange>(ChangePropertiesList.Count);
            foreach (var dic in ChangePropertiesList)
            {
                IAveChange tempChange = GenerateChangeObject(dic);
                mListData.Add(tempChange);
            }
        }
        private IAveChange GenerateChangeObject(IDictionary<string, object> changeProperties)
        {
            if (changeProperties.Count <= 0) 
            {
                return null;
            }
            string changeObjectType = changeProperties["ChangeObjectType"] as string;
            switch (changeObjectType)
            {
                case "Microsoft.SharePoint.Client.ChangeItem":
                    return new AveChangeItem(changeProperties);
                case "Microsoft.SharePoint.Client.ChangeFile":
                    return new AveChangeFile(changeProperties);
                case "Microsoft.SharePoint.Client.ChangeList":
                    return new AveChangeList(changeProperties);
                case "Microsoft.SharePoint.Client.ChangeWeb":
                    return new AveChangeWeb(changeProperties);
                default:
                    break;
            }
            return new AveChange(changeProperties);
        }
        public int Count
        {
            get { return mListData.Count; }
        }

        public bool IncludeBeginning
        {
            get { throw new NotImplementedException(); }
        }

        public IAveChange this[int index]
        {
            get { return mListData[index]; }
        }

        public IAveChangeToken LastChangeToken
        {
            get
            {
                if (this.Count == 0)
                {
                    return null;
                }
                return mListData[this.Count - 1].ChangeToken;
            }
        }

        public AveCollectionScope Scope
        {
            get { throw new NotImplementedException(); }
        }

        public Guid ScopeId
        {
            get { throw new NotImplementedException(); }
        }
    }
}
