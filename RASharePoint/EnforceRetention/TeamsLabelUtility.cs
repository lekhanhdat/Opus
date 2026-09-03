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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention
{
    public class TeamsLabelUtility: SPOLabelUtility
    {
        public TeamsLabelUtility(bool addToStatistics = false)
        {
            mAddToStatistics = addToStatistics;
            Init();
        }

        protected override void Init()
        {
            mRetentionCache = new RetentionDataCache(RMRetentionSourceType.Teams);
            mRetentionCache.CacheTermChange(DateTime.UtcNow.Ticks);
        }
        protected override TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!mRetentionCache.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention, TeamsRetentionLabel = tempTerm.TeamsRetentionLabel };
                    mRetentionCache.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }

        public override bool UpdateLabel(IAveListItem aveItem, Guid termId, Guid recordId, Guid previousTermId)
        {
            if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                return false;
            }
            bool labelNotExist = false;
            if (termId != Guid.Empty)
            {
                //term id改变时才操作label
                try
                {
                    TermSettingsInfo termInfo = GetTermInfo(termId);
                    if (termInfo != null)
                    {
                        if ((termInfo.EnforceRetention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams)
                        {
                            labelNotExist = ApplyComplianceTag(aveItem, recordId, termInfo, termId, previousTermId);
                        }
                        else
                        {
                            if (previousTermId != termId)
                            {
                                RemoveComplianceTag(aveItem);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating retention label. Item url:{0} Error:{1}", aveItem.FullPath(), e.ToString());
                }
            }
            else
            {
                //term id改变时才操作label
                if (previousTermId != Guid.Empty)
                {
                    RemoveComplianceTag(aveItem);
                }
            }
            return labelNotExist;
        }
    }
}
