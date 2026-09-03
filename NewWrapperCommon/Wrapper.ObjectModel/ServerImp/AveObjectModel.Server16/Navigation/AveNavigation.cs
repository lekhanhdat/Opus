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
using Microsoft.SharePoint.Navigation;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveNavigation : AveServerObject, IAveNavigation
    {
        private SPNavigation mNavigation;
        private AveNavigationNodeCollection mQuickLaunch;
        private AveNavigationNodeCollection mTopNavigationBar;
        private AveNavigationNodeCollection mSearchNav;

        public AveNavigation(SPNavigation navigation)
        {
            mNavigation = navigation;
        }

        #region IAveNavigation Members

        public IAveNavigationNodeCollection QuickLaunch
        {
            get
            {
                if (mQuickLaunch == null)
                {
                    SPNavigationNodeCollection navigationNodes = mNavigation.QuickLaunch;
                    if (navigationNodes != null)
                    {
                        mQuickLaunch = new AveNavigationNodeCollection(navigationNodes);
                    }
                }
                return mQuickLaunch;
            }
        }

        public IAveNavigationNodeCollection TopNavigationBar
        {
            get
            {
                if (mTopNavigationBar == null)
                {
                    SPNavigationNodeCollection navigationNodes = mNavigation.TopNavigationBar;
                    if (navigationNodes != null)
                    {
                        mTopNavigationBar = new AveNavigationNodeCollection(navigationNodes);
                    }
                }
                return mTopNavigationBar;
            }
        }

        public bool UseShared
        {
            get
            {
                return mNavigation.UseShared;
            }
            set
            {
                mNavigation.UseShared = value;
            }
        }

        public IAveNavigationNode Home
        {
            get
            {
                return this.GetNodeById(0x3e8);
            }
        }

        public IAveNavigationNode GetNodeById(int id)
        {
            if (id == 0)
            {
                return null;
            }
            SPNavigationNode navigationNode = mNavigation.GetNodeById(id);
            if (navigationNode == null)
            {
                return null;
            }
            return new AveNavigationNode(navigationNode);
        }

        public IAveNavigationNode AddToQuickLaunch(IAveNavigationNode node, AveQuickLaunchHeading heading)
        {
            return new AveNavigationNode(mNavigation.AddToQuickLaunch((node as AveNavigationNode).NavigationNode, (SPQuickLaunchHeading)heading));
        }

        public bool RestoreNavigation(AveNavigationInfoList navigationNodes, NavigationRestoreSetting setting)
        {
            return false;
        }

        public IAveNavigationNodeCollection SearchNav
        {
            get
            {
                if (mSearchNav == null)
                {
                    SPNavigationNodeCollection navigationNodes = mNavigation.SearchNav;
                    if (navigationNodes != null)
                    {
                        mSearchNav = new AveNavigationNodeCollection(navigationNodes);
                    }                   
                }
                return mSearchNav;
            }
        }
        #endregion

    }
}
