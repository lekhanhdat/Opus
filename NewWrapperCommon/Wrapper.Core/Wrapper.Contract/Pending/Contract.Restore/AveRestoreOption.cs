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

namespace AvePoint.Wrapper.Restore
{
    public class AveRestoreOption
    {
        public AveUserRestoreOption mAveUserRestoreOption = new AveUserRestoreOption();
        public AveGroupRestoreOption mAveGroupRestoreOption = new AveGroupRestoreOption();
        public AvePermissionRestoreOption mAvePermissionRestoreOption = new AvePermissionRestoreOption();
        public AveRoleRestoreOption mAveRoleRestoreOption = new AveRoleRestoreOption();
        public AveFeatureRestoreOption mAveFeatureRestoreOption = new AveFeatureRestoreOption();
        public AveSolutionRestoreOption mAveSolutionRestoreOption = new AveSolutionRestoreOption();
        public AveTopLinkBarRestoreOption mAveTopLinkBarRestoreOption = new AveTopLinkBarRestoreOption();
        public AveContentTypeRestoreOption mAveContentTypeRestoreOption = new AveContentTypeRestoreOption();
        public AveWorkFlowRestoreOption mAveWorkFlowRestoreOption = new AveWorkFlowRestoreOption();
        public AveFieldRestoreOption mAveFieldRestoreOption = new AveFieldRestoreOption();
        public AveQuickLaunchRestoreOption mAveQuickLaunchRestoreOption = new AveQuickLaunchRestoreOption();
        public AveListRestoreOption mAveListRestoreOption = new AveListRestoreOption();
        public AveAlertRestoreOption mAveAlertRestoreOption = new AveAlertRestoreOption();
        public AveVersionRestoreOption mAveVersionRestoreOption = new AveVersionRestoreOption();
        public AveWebPartrestoreOption mAveWebPartrestoreOption = new AveWebPartrestoreOption();
        public AveAttachmentRestoreOption mAveAttachmentRestoreOption = new AveAttachmentRestoreOption();
        public AveItemRestoreOption mAveItemRestoreOption = new AveItemRestoreOption();
        public AveEventReceiverOption mAveEventReceiverOption = new AveEventReceiverOption();
        public AveRestoreMode mAveRestoreMode = AveRestoreMode.OverWrite;
        public AveStorageOption mAveStorgeOption = new AveStorageOption();
        private static int mDefaultRequestOption;
        public int mRequestOption;
        public AveTermStoreRestoreOption TermStoreRestoreOption = new AveTermStoreRestoreOption();
        public AveTaxonomyGroupRestoreOption TaxonomyGroupRestoreOption = new AveTaxonomyGroupRestoreOption();
        public AveTermSetRestoreOption TermSetRestoreOption = new AveTermSetRestoreOption();
        public AveTermRestoreOption TermRestoreOption = new AveTermRestoreOption();


        /// <summary>
        /// Init mRequestOption with AveRestoreMode.ReplicatoreDefault.
        /// </summary>
        public AveRestoreOption()
            : this((int)AveRestoreMode.ReplicatorDefault)
        {
        }

        /// <summary>
        /// Init mRequestOption with argument.
        /// </summary>
        /// <param name="defaultRequestOption"></param>
        public AveRestoreOption(int defaultRequestOption)
        {
            mRequestOption = mDefaultRequestOption = defaultRequestOption;
        }

        public void SetRequestOption(bool restoreProperty, bool restoreSecurity, int restoreOption)
        {
            mRequestOption = mDefaultRequestOption;
            if (restoreProperty)
            {
                Set(AveRestoreMode.RestoreProperty);
            }
            if (restoreSecurity)
            {
                Set(AveRestoreMode.RestoreSecurity);
            }
            mAveRestoreMode = (AveRestoreMode)restoreOption;
            Set(mAveRestoreMode);
        }

        public void ResetProperty(bool property)
        {
            if (property)
            {
                mRequestOption |= (int)AveRestoreMode.RestoreProperty;
            }
            else
            {
                mRequestOption &= ~(int)AveRestoreMode.RestoreProperty;
            }
        }

        public void ResetSecurity(bool security)
        {
            if (security)
            {
                mRequestOption |= (int)AveRestoreMode.RestoreSecurity;
            }
            else
            {
                mRequestOption &= ~(int)AveRestoreMode.RestoreSecurity;
            }
        }

        public void ResetRestoreMode(int restoreOption)
        {
            mRequestOption &= ~(int)mAveRestoreMode;
            mAveRestoreMode = (AveRestoreMode)restoreOption;
            mRequestOption |= restoreOption;
        }

        public void ResetRequestOption(bool restoreProperty, bool restoreSecurity, int restoreOption)
        {
            mRequestOption = 0;
            if (restoreProperty)
            {
                Set(AveRestoreMode.RestoreProperty);
            }
            if (restoreSecurity)
            {
                Set(AveRestoreMode.RestoreSecurity);
            }
            mAveRestoreMode = (AveRestoreMode)restoreOption;
            Set(mAveRestoreMode);
        }

        public bool CheckRestoreOption(AveRestoreMode aroc)
        {
            if (aroc == AveRestoreMode.Replace)
            {
                return ((mRequestOption & (int)0x0008) != 0);
            }
            if (aroc == AveRestoreMode.OverWriteByModifiedTime)
            {
                return ((mRequestOption & (int)0x0010) != 0);
            }
            return ((mRequestOption & (int)aroc) != 0);
        }

        private void Set(AveRestoreMode x)
        {
            mRequestOption |= (int)x;
        }
    }
    /// <summary>
    /// add for stub data restore option
    /// </summary>
    public class AveStorageOption
    {
        public bool DESTSTUB_CONTENT = false;//Archiver的currentversion如果还原成content，需要修改docflag为65536
        public bool MIG_STUB_PIC_THUMBNAILS = false;
    }
    public class AveUserRestoreOption
    {
        public bool SITE_USER = true;
    }

    public class AveGroupRestoreOption
    {
        public bool SITE_GROUP = true;
    }

    public class AvePermissionRestoreOption
    {
        public bool SITE_PERMISSION = true;
    }

    public class AveRoleRestoreOption
    {
        public bool SITE_ROLE = true;
    }

    public class AveFeatureRestoreOption
    {
        public bool SITE_FEATURE = true;
        public bool WEB_FEATURE = true;
    }

    public class AveSolutionRestoreOption
    {
        public bool SITE_SOLUTION = true;
        public bool WEB_SOLUTION = true;
    }

    public class AveTopLinkBarRestoreOption
    {
        public bool WEB_TOPLINKBAR = true;
    }

    public class AveWorkFlowRestoreOption
    {
        public bool WEB_WORKFLOW = true;
        public bool LIST_WORKFLOW = true;
    }

    public class AveQuickLaunchRestoreOption
    {
        public bool WEB_QUICKLUNCH = true;
    }

    public class AveListRestoreOption
    {
        public bool VerifyListTemplateFeature = false;
    }

    public class AveAlertRestoreOption
    {
        public bool LIST_ALERT = true;

        public bool ITEM_ALERT = true;
    }

    public class AveVersionRestoreOption
    {
        public bool ITEM_VERSION = true;
    }

    public class AveWebPartrestoreOption
    {
        public bool ITEM_WEBPART = true;
    }

    public class AveAttachmentRestoreOption
    {
        public bool ITEM_ATTACHMENT = true;
    }

    public class AveItemRestoreOption
    {
        public bool IncreaceVerionWithRowId = false;//only for replicator to create version in destination to improve the performance
        public bool NewItemWithOutVerifyConflict = false;//only for replicator to create the item to improve the performance
        public bool DELETE_ITEM = true;//both item & replicator use this option when want overwrite whole item
        public bool OVERITE_ITEM = true; //only item use it,
        public bool DISCARD_ITEM_ONLY = false; //only replicator use this option, when only replicate discard operation
        public bool DISCARD_ITEM_POSSIBLE = false;//only replicator use this option, always set to TRUE
        public bool KEEP_ITEM_TPGUID = false;// only replicator use this option
        public bool CheckConflictByUniqueId = false;// only for HSM office365 IB check conflict item
        public bool MOVE_ITEM_TO_CONFLICT_FOLDER = false;//only replicator use this option, when conflict and manual, set to true
        /// <summary>
        /// TODO 这个可以外围直接设置还原到conflict folder中，这样会比较好些。
        /// </summary>
        public bool MOVE_SOURCE_ITEM_TO_FOLDER = false;//only replicator use and when conflict and manual and target win
        public bool SKIP_IF_SAME_MODIFIEDTIME = true;//Skip restore the item when conflict and have same modified time
        public bool IsProcessSolutionStatus = true;//When runs DMJob, set false to skip to Process Solution Status
        /// <summary>
        /// verify the page layout exist before restoring
        /// </summary>
        public bool VerifyPageLayout = true;
        public string MatchItemFieldDisplayValue = string.Empty;
    }

    /// <summary>
    /// 这个主要是在还原任何对象，包括Site，Web，List，Item等时，可以在当前的进程里不触发客户的Event Receiver
    /// </summary>
    [Obsolete("This method will be deprecated and removed later. key--001")]
    public class AveEventReceiverOption
    {
        public bool DISABLE_EVENT_RECEIVER = false;
    }

    /// <summary>
    /// move to wrapper common
    /// </summary>
    //public enum AveRestoreMode
    //{
    //    Default = 0x0001,
    //    OverWrite = 0x0002,
    //    Append = 0x0004,
    //    Replace = 0x000A,
    //    OverWriteByModifiedTime = 0x0012,
    //    AppendANewVersion = 0x0020,
    //    UpgradeOnly = 0x000D, //for app
    //    Restore = 0x000E,
    //    RestoreProperty = 0x0100,
    //    RestoreSecurity = 0x0200,
    //    ReplicatorDefault = 0x0302,
    //}

    public class AveTermStoreRestoreOption
    {
        public bool RESTOREPROPERTIES = true;
        public bool RESTORESECURITY = true;
    }

    public class AveTaxonomyGroupRestoreOption
    {
        public bool RESTOREPROPERTIES = true;
        public bool RESTORESECURITY = true;
    }

    public class AveTermSetRestoreOption
    {
        public bool RESTOREPROPERTIES = true;
        public bool RESTORESECURITY = true;
    }

    public class AveTermRestoreOption
    {
        public bool RESTOREPROPERTIES = true;
        public bool RESTORESECURITY = true;
    }

}