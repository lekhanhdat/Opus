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




using AvePoint.Wrapper.Restore;

namespace AvePoint.Item.Restore
{
    public class RestoreContentDto
    {
        private AveRestoreOption mRestoreOption = new AveRestoreOption(0);

        public AveRestoreOption RestoreOption
        {
            get { return mRestoreOption; }
            set { mRestoreOption = value; }
        }

        public System.Guid UniqueId { get; set; }

        public string Name { get; set; }

        public bool IsFailed { get; set; }

        public string ParentName { get; set; }

        public char Type { get; set; }

        public string SrcName { get; set; }

        public bool IsMyProfileList { get; set; }

        public char ReplaceType { get; set; }

        public string OwnerLogin { get; set; }

        public bool IsAppData { get; set; }
        /// <summary>
        /// used for site level
        /// </summary>
        public bool IsChecked { get; set; }
        public bool IsSelected { get; set; }
        public bool ParentIsSelected { get; set; }
        public bool IsCurrentVersion { get; set; }

        public string SrcUrl { get; set; }

        public int CompatibilityLevel { get; set; }   //SAAS-10617  Create Site Collection时使用

        public int LCID { get; set; }

        public string Owner { get; set; }

        public string Template { get; set; }

        public string Title { get; set; }

        public string SiteUrl { get; set; }
        public string StubType { get; set; }
        public string OopSourceUrl { get; set; }
        public string Id { get; set; }
        public string StorageId { get; set; }
        public string BackUpJobId { get; set; }
        public string ItemPathMd5 { get; set; }

        public long ArchiveTime { get; set; }
    }
}
