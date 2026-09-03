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
using Microsoft.SharePoint.WebControls;
using System.Linq;

namespace AvePoint.ObjectModel.Server19
{
    class AveProviderHierarchyElement : IAveProviderHierarchyElement
    {
        SPProviderHierarchyElement element;

        public AveProviderHierarchyElement(SPProviderHierarchyElement element)
        {
            this.element = element;
        }

        public IAveProviderHierarchyNode[] Children
        {
            get
            {
                return this.element.Children.Select(c => c == null ? null : new AveProviderHierarchyNode(c) as IAveProviderHierarchyNode).ToArray();

            }
            set
            {
                this.element.Children = value.Select(c => c == null ? null : (c as AveProviderHierarchyNode).ProviderHierarchyNode).ToArray();
            }
        }

        public int Count
        {
            get
            {
                return element.Count;
            }
            set
            {
                element.Count = value;
            }
        }

        public System.Collections.Generic.List<IAvePickerEntity> EntityData
        {
            get
            {
                return this.element.EntityData.Select(entity => entity == null ? null : new AvePickerEntity(entity) as IAvePickerEntity).ToList();
            }
            set
            {
                this.element.EntityData = value.Select(entity => entity == null ? null : (entity as AvePickerEntity).PickerEntity).ToList();
            }
        }

        public bool HasChildren
        {
            get { return this.element.HasChildren; }
        }

        public string HierarchyNodeID
        {
            get
            {
                return this.element.HierarchyNodeID;
            }
            set
            {
                this.element.HierarchyNodeID = value;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return this.element.IsLeaf;
            }
            set
            {
                this.element.IsLeaf = value;
            }
        }

        public string Name
        {
            get
            {
                return this.element.Name;
            }
            set
            {
                this.element.Name = value;
            }
        }

        public string ProviderName
        {
            get
            {
                return this.element.ProviderName;
            }
            set
            {
                this.element.ProviderName = value;
            }
        }
    }
}
