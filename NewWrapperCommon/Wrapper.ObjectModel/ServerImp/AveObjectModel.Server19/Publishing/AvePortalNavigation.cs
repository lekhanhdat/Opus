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
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Server19
{
    class AvePortalNavigation : IAvePortalNavigation
    {
        //private AvePublishingWeb avePublishingWeb;
        private readonly PublishingWeb publishingWeb;
        private readonly PortalNavigation navigation;

        public AvePortalNavigation(PublishingWeb publishingWeb)
        {
            this.publishingWeb = publishingWeb;
            this.navigation = publishingWeb.Navigation;
        }
        public AveAutomaticSortingMethod AutomaticSortingMethod
        {
            get
            {
                return (AveAutomaticSortingMethod)navigation.AutomaticSortingMethod;
            }
            set
            {
                navigation.AutomaticSortingMethod = (AutomaticSortingMethod)(value);
            }
        }

        public int CurrentDynamicChildLimit
        {
            get
            {
                return navigation.CurrentDynamicChildLimit;
            }
            set 
            {
                navigation.CurrentDynamicChildLimit = value;
            }
        }

        public bool CurrentIncludePages
        {
            get
            {
                return navigation.CurrentIncludePages;
            }
            set
            {
                navigation.CurrentIncludePages = value;
            }
        }

        public bool CurrentIncludeSubSites
        {
            get
            {
                return navigation.CurrentIncludeSubSites;
            }
            set
            {
                navigation.CurrentIncludeSubSites = value;
            }
        }

        public IAveNavigationNodeCollection CurrentNavigationNodes
        {
            get { return new AveNavigationNodeCollection(navigation.CurrentNavigationNodes); }
        }

        public int GlobalDynamicChildLimit
        {
            get
            {
                return navigation.GlobalDynamicChildLimit;
            }
            set
            {
                navigation.GlobalDynamicChildLimit = value;
            }
        }

        public bool GlobalIncludePages
        {
            get
            {
                return navigation.GlobalIncludePages;
            }
            set
            {
                navigation.GlobalIncludePages = value;
            }
        }

        public bool GlobalIncludeSubSites
        {
            get
            {
                return navigation.GlobalIncludeSubSites;
            }
            set
            {
                navigation.GlobalIncludeSubSites = value;
            }
        }

        public IAveNavigationNodeCollection GlobalNavigationNodes
        {
            get { return new AveNavigationNodeCollection(navigation.GlobalNavigationNodes); }
        }

        public bool InheritCurrent
        {
            get
            {
                return navigation.InheritCurrent;
            }
            set
            {
                navigation.InheritCurrent = value;
            }
        }

        public bool InheritGlobal
        {
            get
            {
                return navigation.InheritGlobal;
            }
            set
            {
                navigation.InheritGlobal = value;
            }
        }

        public AveOrderingMethod OrderingMethod
        {
            get
            {
                return (AveOrderingMethod)navigation.OrderingMethod;
            }
            set
            {
                navigation.OrderingMethod = (OrderingMethod)value;
            }
        }

        public bool ShowSiblings
        {
            get
            {
                return navigation.ShowSiblings;
            }
            set
            {
                navigation.ShowSiblings = value;
            }
        }

        public bool SortAscending
        {
            get
            {
                return navigation.SortAscending;
            }
            set
            {
                navigation.SortAscending = value;
            }
        }
    }
}
