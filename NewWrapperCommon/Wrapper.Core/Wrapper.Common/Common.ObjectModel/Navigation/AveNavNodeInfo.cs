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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveNavigationInfo
    {
        public string Title;
        public string Url;
        public string ParentTitle;
        public bool IsExternal;
        public int NodeType;
        public string Target;
        public string MetaInfo;
        public bool HasMetaInfo;
        public int RankChild;
        public int Eid;
        public int EidParent;
        public string Description;
        public string Audience;
        public DateTime LastModifiedDate;

        public AveNavigationScope Scope;
        public List<AveNavigationInfo> Children = new List<AveNavigationInfo>();
        [NonSerialized]
        public AveUserResourceInfo TitleResource;
    }

    [Serializable]
    public class AveNavigationInfoList
    {
        public bool SharedTopLink { set; get; }

        public bool ShareQuickLaunch { set; get; }

        public bool PublishFeatureAppearance { set; get; }

        public bool BackupFromInheritedWeb { set; get; }

        public List<AveNavigationInfo> NavNodes = new List<AveNavigationInfo>();
    }

    public enum AveNavigationScope
    {
        TopNavigationBar,
        QuickLaunch,
        SearchNavigation
    }
}
