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
using Microsoft.SharePoint.Workflow;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveWorkflowAssociationCollection : AveAbstractCommonCollection<IAveWorkflowAssociation>, IAveWorkflowAssociationCollection
    {
        private SPWorkflowAssociationCollection mWorkflowAssociationCollection;

        internal IAveSite ParentSite { get;set; }
        internal IAveWeb ParentWeb { get;set; }
        internal IAveList ParentList { get; set; }
        internal IAveContentType ParentContentType { get; set; }

        internal AveWorkflowAssociationCollectionParentObjectType ParentObjectType { get; set; }

        //[Obsolete]
        //public AveWorkflowAssociationCollection(SPWorkflowAssociationCollection workflowAssociationCollection)
        //    : base(workflowAssociationCollection)
        //{
        //    mWorkflowAssociationCollection = workflowAssociationCollection;
        //}

        /// <summary>
        /// for static method
        /// </summary>
        public AveWorkflowAssociationCollection()
            :base(null)
        { }

        public AveWorkflowAssociationCollection(IAveList parentList,SPWorkflowAssociationCollection workflowAssociationCollection)
            : base(workflowAssociationCollection)
        {
            ParentList = parentList;
            ParentWeb = parentList.ParentWeb;
            ParentSite = parentList.ParentWeb.Site;
            ParentObjectType = AveWorkflowAssociationCollectionParentObjectType.List;
            mWorkflowAssociationCollection = workflowAssociationCollection;
        }

        public AveWorkflowAssociationCollection(IAveWeb parentWeb,SPWorkflowAssociationCollection workflowAssociationCollection)
            : base(workflowAssociationCollection)
        {
            ParentList = null;
            ParentWeb = parentWeb;
            ParentSite = parentWeb.Site;
            ParentObjectType = AveWorkflowAssociationCollectionParentObjectType.Web;
            mWorkflowAssociationCollection = workflowAssociationCollection;
        }

        public AveWorkflowAssociationCollection(IAveContentType parentContentType, SPWorkflowAssociationCollection workflowAssociationCollection)
            : base(workflowAssociationCollection)
        {
            ParentContentType = parentContentType;
            if (parentContentType.ParentList != null)
            {
                ParentList = parentContentType.ParentList;
                ParentWeb = parentContentType.ParentList.ParentWeb;
                ParentObjectType = AveWorkflowAssociationCollectionParentObjectType.ListContentType;
            }
            else if (parentContentType.ParentWeb != null)
            {
                ParentWeb = parentContentType.ParentWeb;
                ParentObjectType = AveWorkflowAssociationCollectionParentObjectType.WebContentType;
            }
            else
            {
                throw new AveWrapperException("Invalid parent contentType object.");
            }

            mWorkflowAssociationCollection = workflowAssociationCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveWorkflowAssociation(this,t as SPWorkflowAssociation);
        }

        #region IAveWorkflowAssociationCollection Members

        public IAveWorkflowAssociation this[Guid workflowAssociationId]
        {
            get
            {
                SPWorkflowAssociation workflowAssociation = mWorkflowAssociationCollection[workflowAssociationId];
                if (workflowAssociation == null)
                {
                    return null;
                }
                return new AveWorkflowAssociation(this,workflowAssociation);
            }
        }

        public void Update(IAveWorkflowAssociation workflowAssociation)
        {
            mWorkflowAssociationCollection.Update((workflowAssociation as AveWorkflowAssociation).WorkflowAssociation);
        }

        public void Remove(IAveWorkflowAssociation association)
        {
            mWorkflowAssociationCollection.Remove((association as AveWorkflowAssociation).WorkflowAssociation);
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId)
        {
            return this.GetAssociationByBaseID(baseTemplateId, false);
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId, bool ignoreStartSettings)
        {
            object workFlowAssociation = AveAssemblyUtility.InvokeMethod(mWorkflowAssociationCollection, "GetAssociationByBaseID", new object[] { baseTemplateId, ignoreStartSettings });
            if (workFlowAssociation != null)
            {
                return new AveWorkflowAssociation(this,(SPWorkflowAssociation)workFlowAssociation);
            }
            return null;
        }

        /// <summary>static method
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="associationId"></param>
        /// <returns></returns>
        internal IAveWorkflowAssociation GetAssociationForListItemById(IAveListItem item, Guid associationId)
		{
			return GetAssociationForListItemById(item, associationId, AveTriState.NA);
		}

        /// <summary>static method
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="associationId"></param>
        /// <param name="tListAssociation"></param>
        /// <returns></returns>
        internal IAveWorkflowAssociation GetAssociationForListItemById(IAveListItem item, System.Guid associationId, AveTriState tListAssociation)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException();
            }
            if (tListAssociation == AveTriState.NA)
            {
                (item as AveListItem).ListItem.EnsureWorkflowInformation(true, false);
            }
            IAveWorkflowAssociation sPWorkflowAssociation = null;
            if (tListAssociation == AveTriState.True || tListAssociation == AveTriState.NA)
            {
                sPWorkflowAssociation = item.ParentList.WorkflowAssociations[associationId];
            }
            if (sPWorkflowAssociation == null && (tListAssociation == AveTriState.False || tListAssociation == AveTriState.NA))
            {
                sPWorkflowAssociation = item.ContentType.WorkflowAssociations[associationId];
            }
            return sPWorkflowAssociation;
        }

        public override IAveWorkflowAssociation this[int index]
        {
            get
            {
                return new AveWorkflowAssociation(this,mWorkflowAssociationCollection[index]);
            }
        }

        public override int Count
        {
            get { return mWorkflowAssociationCollection.Count; }
        }

        public IAveWorkflowAssociation Add(IAveWorkflowAssociation workflowAssociation)
        {
            SPWorkflowAssociation workFlowAssociation = mWorkflowAssociationCollection.Add((workflowAssociation as AveWorkflowAssociation).WorkflowAssociation);
            if (workFlowAssociation == null)
            {
                return null;
            }
            return new AveWorkflowAssociation(this,workFlowAssociation);
        }

        public IAveWorkflowAssociation GetAssociationByName(string name, System.Globalization.CultureInfo cultureInfo)
        {
            SPWorkflowAssociation workflowAssociation = mWorkflowAssociationCollection.GetAssociationByName(name, cultureInfo);
            if (workflowAssociation == null)
            {
                return null;
            }
            return new AveWorkflowAssociation(this,workflowAssociation);
        }

        public bool UpdateAssociationsToLatestVersion()
        {
            return mWorkflowAssociationCollection.UpdateAssociationsToLatestVersion();
        }

        public void RemoveAll()
        {
            if (mWorkflowAssociationCollection != null)
            {
                int count = mWorkflowAssociationCollection.Count;
                for (int k = count - 1; k >= 0; k--)
                {
                    mWorkflowAssociationCollection.Remove(mWorkflowAssociationCollection[k]);
                }
            }
        }

        #endregion
    }
}
