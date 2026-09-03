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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.RecordsManagement.InformationPolicy;
using System.Xml;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOPolicy : IAveOPolicy
    {
        private Policy mPolicy;
        private AveOPolicyItemCollection mItems;

        public AveOPolicy(Policy policy)
        { 
            mPolicy = policy; 
        }

        public AveOPolicy()
        { }

        internal Policy Policy
        {
            get
            {
                return mPolicy;
            }
        }

        #region IAveOPolicy Members

        public string Id
        {
            get 
            {
                return mPolicy.Id;
            }
        }

        public IAveOPolicyItemCollection Items
        {
            get
            {
                if (mItems == null)
                {
                    mItems = new AveOPolicyItemCollection(mPolicy.Items);
                }
                return mItems;
            }
        }

        public string Name
        {
            get
            {
                return mPolicy.Name;
            }
            set
            {
                mPolicy.Name = value;
            }
        }

        public string Statement
        {
            get
            {
                return mPolicy.Statement;
            }
            set
            {
                mPolicy.Statement = value;
            }
        }

        public bool CanHavePolicy(IAveContentType ct)
        {
            SPContentType contentType = null;
            if (ct != null)
            {
                contentType = (ct as AveContentType).ContentType;
            }
            return Policy.CanHavePolicy(contentType);
        }

        public void CreatePolicy(IAveContentType ct, IAveOPolicy globalPolicy)
        {
            SPContentType contentType = null;
            Policy policy = null;
            if (ct != null)
            {
                contentType = (ct as AveContentType).ContentType;
            }
            if (globalPolicy != null)
            {
                policy = (globalPolicy as AveOPolicy).Policy;
            }
            Policy.CreatePolicy(contentType, policy);
        }

        public void DeletePolicy(IAveContentType ct)
        {
            SPContentType contentType = null;
            if (ct != null)
            {
                contentType = (ct as AveContentType).ContentType;
            }
            Policy.DeletePolicy(contentType);
        }

        public XmlDocument Export()
        {
            return mPolicy.Export();
        }

        public IAveOPolicy GetPolicy(IAveContentType ct)
        {
            SPContentType contentType = null;
            if (ct != null)
            {
                contentType = (ct as AveContentType).ContentType;
            }
            Policy policy = Policy.GetPolicy(contentType);
            if (policy != null)
            {
                return new AveOPolicy(policy);
            }
            return null;
        }

        public bool InheritsPolicy(IAveContentType ct)
        {
            SPContentType contentType = null;
            if (ct != null)
            {
                contentType = (ct as AveContentType).ContentType;
            }
            return Policy.InheritsPolicy(contentType);
        }

        public void Update()
        {
            mPolicy.Update();
        }

        public string Description
        {
            get
            {
                return mPolicy.Description;
            }
            set
            {
                mPolicy.Description = value;
            }
        }

        public bool IsLocal
        {
            get 
            {
                return mPolicy.IsLocal;
            }
        }

        public string ModifiedBy
        {
            get { return mPolicy.ModifiedBy; }
        }

        public DateTime ModifiedDate
        {
            get { return mPolicy.ModifiedDate; }
        }

        public bool IsItemExempt(IAveListItem item)
        {
            SPListItem tempItem = (item as AveListItem) != null ? (item as AveListItem).ListItem : null;
            return Policy.IsItemExempt(tempItem);
        }

        public void SetExemption(IAveListItem item)
        {
            SPListItem tempItem = (item as AveListItem) != null ? (item as AveListItem).ListItem : null;
            Policy.SetExemption(tempItem);
        }

        public void RemoveExemption(IAveListItem item)
        {
            SPListItem tempItem = (item as AveListItem) != null ? (item as AveListItem).ListItem : null;
            Policy.RemoveExemption(tempItem);
        }

        #endregion   
    }
}
