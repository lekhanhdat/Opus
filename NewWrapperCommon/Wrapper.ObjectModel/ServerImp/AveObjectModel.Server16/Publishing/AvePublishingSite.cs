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
using Microsoft.Office.RecordsManagement.RecordsRepository;
using Microsoft.SharePoint;
using System.Collections.Generic;
using Microsoft.Office.RecordsManagement.Holds;
using System;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Server16
{
    class AvePublishingSite : IAvePublishingSite
    {
        private PublishingSite mPublishingSite;
        readonly AveLogger logger = AveLogger.GetInstance(typeof(AvePublishingSite));
        public AvePublishingSite(PublishingSite publishingSite)
        {
            mPublishingSite = publishingSite;
        }

        public AvePublishingSite(IAveSite site)
        {
            mPublishingSite = new PublishingSite((site as AveSite).Site);
        }

        /// <summary>
        /// Contruct method for calling static method
        /// </summary>
        public AvePublishingSite()
        { }

        public bool IsPublishingSite(IAveSite site)
        {
            try
            {
                return PublishingSite.IsPublishingSite((site as AveSite).Site);
            }
            catch (Exception e)
            {
                logger.Debug("Check publishing web error. {0}", e.ToString());
                return site.Features[AveSP2013FeatureDefinitions.PublishingSite] == null;
            }
        }
    }
}
