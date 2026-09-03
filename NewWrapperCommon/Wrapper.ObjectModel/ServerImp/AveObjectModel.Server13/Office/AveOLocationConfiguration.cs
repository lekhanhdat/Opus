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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOLocationConfiguration : IAveOLocationConfiguration
    {
        private LocationConfiguration mLocationConfiguration;
        private AveOVisualization mAveOVisualization;
        private AveOAuthenticationInformation mAveOAuthenticationInformation;

        public AveOLocationConfiguration(LocationConfiguration locationConfiguration)
        {
            mLocationConfiguration = locationConfiguration;
        }

        internal LocationConfiguration LocationConfiguration
        {
            get
            {
                return mLocationConfiguration;
            }
        }

        public string Name
        {
            get
            {
                return LocationConfiguration.Name;
            }
            set
            {
                LocationConfiguration.Name = value;
            }
        }

        public string InternalName
        {
            get
            {
                return LocationConfiguration.InternalName;
            }
            set
            {
                LocationConfiguration.InternalName = value;
            }
        }


        public string AdminDescription
        {
            get
            {
                return LocationConfiguration.AdminDescription;
            }
            set
            {
                LocationConfiguration.AdminDescription = value;
            }
        }

        public string Author
        {
            get
            {
                return LocationConfiguration.Author;
            }
            set
            {
                LocationConfiguration.Author = value;
            }
        }

        public Version Version
        {
            get
            {
                return LocationConfiguration.Version;
            }
            set
            {
                LocationConfiguration.Version = value;
            }
        }

        public AveOLocationType Type
        {
            get
            {
                return (AveOLocationType)LocationConfiguration.Type;
            }
            set
            {
                LocationConfiguration.Type = (LocationType)value;
            }
        }


        public string ConnectionUrlTemplate
        {
            get
            {
                return LocationConfiguration.ConnectionUrlTemplate;
            }
            set
            {
                LocationConfiguration.ConnectionUrlTemplate = value;
            }
        }

        public string MoreLinkTemplate
        {
            get
            {
                return LocationConfiguration.MoreLinkTemplate;
            }
            set
            {
                LocationConfiguration.MoreLinkTemplate = value;
            }
        }

        public bool IsRestrictedLocation
        {
            get
            {
                return LocationConfiguration.IsRestrictedLocation;
            }
            set
            {
                LocationConfiguration.IsRestrictedLocation = value;
            }
        }

        public IAveOVisualization SummaryVisualization
        {
            get
            {
                if (LocationConfiguration.SummaryVisualization != null)
                {
                    return new AveOVisualization(LocationConfiguration.SummaryVisualization);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    LocationConfiguration.SummaryVisualization = (value as AveOVisualization).Visualization;
                }
                else
                {
                    LocationConfiguration.SummaryVisualization = null;
                }
            }
        }

        public IAveOVisualization TopAnswerVisualization
        {
            get
            {
                if (LocationConfiguration.TopAnswerVisualization != null)
                {
                    return new AveOVisualization(LocationConfiguration.TopAnswerVisualization);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    LocationConfiguration.TopAnswerVisualization = (value as AveOVisualization).Visualization;
                }
                else
                {
                    LocationConfiguration.TopAnswerVisualization = null;
                }
            }
        }


        public IAveOAuthenticationInformation AuthInfo
        {
            get
            {
                if (LocationConfiguration.AuthInfo != null)
                {
                    return new AveOAuthenticationInformation(LocationConfiguration.AuthInfo);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    LocationConfiguration.AuthInfo = (value as AveOAuthenticationInformation).AuthenticationInformation;
                }
                else
                {
                    LocationConfiguration.AuthInfo = null;
                }
            }
        }
    }
}
