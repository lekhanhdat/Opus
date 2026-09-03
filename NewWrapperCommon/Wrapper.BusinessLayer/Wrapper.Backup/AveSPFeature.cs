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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSiteFeature : AveSPFeature
    {
        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveSPSiteFeature(AveSPSite aveSite)
        {
            mAveParentSite = aveSite;
        }

        public override AveFeatureInfoBox GetFeatures()
        {
            return mAveParentSite.SPSite.FeatureSerializer.GetObjectData() as AveFeatureInfoBox;
        }

        public override void Export(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.WebInfo"))
            {
                AveFeatureInfoBox infoBox = GetFeatures();
                stream.WriteMetadata(AveMetadataType.SiteFeature, infoBox);
            }
        }
    }

    public class AveSPWebFeature : AveSPFeature
    {
        private AveSPWeb mAveSPWeb;

        public AveSPWebFeature(AveSPWeb aveWeb)
        {
            mAveSPWeb = aveWeb;
        }

        public override AveFeatureInfoBox GetFeatures()
        {
            return mAveSPWeb.SPWeb.FeatureSerializer.GetObjectData() as AveFeatureInfoBox;
        }

        public override void Export(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.Feature"))
            {
                AveFeatureInfoBox infoBox = GetFeatures();
                stream.WriteMetadata(AveMetadataType.WebFeature, infoBox);
            }
        }
    }

    public abstract class AveSPFeature 
    {
        public static AveSPFeature CreateInstance(object obj)
        {
            AveSPFeature instance = null;

            if (obj is AveSPSite)
            {
                instance = new AveSPSiteFeature((AveSPSite)obj);
            }
            else if (obj is AveSPWeb)
            {
                instance = new AveSPWebFeature((AveSPWeb)obj);
            }
            else if (obj is AveSPWebApp)
            {
                //instance = new AveWebAppFeature((AveSPWebApp)obj);
            }
            else
            {
                throw new Exception(string.Format("The object type:{0} is undefined.", obj.GetType().ToString()));
            }

            return instance;
        }

        public abstract void Export(IAveBackupStream stream);

        public abstract AveFeatureInfoBox GetFeatures();

        //public abstract string ExportAsXml();
    }
}