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

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOVisualization : IAveOVisualization
    {
        private Visualization mVisualization;

        public AveOVisualization(Visualization visualization)
        {
            mVisualization = visualization;
        }

        internal Visualization Visualization
        {
            get
            {
                return mVisualization;
            }
        }

        public string Name
        {
            get
            {
                return mVisualization.Name;
            }
            set
            {
                mVisualization.Name = value;
            }
        }

        public string Properties
        {
            get
            {
                return mVisualization.Properties;
            }
            set
            {
                mVisualization.Properties = value;
            }
        }

        public string SampleData
        {
            get
            {
                return mVisualization.SampleData;
            }
            set
            {
                mVisualization.SampleData = value;
            }
        }

        public string Xsl
        {
            get
            {
                return mVisualization.Xsl;
            }
            set
            {
                mVisualization.Xsl = value;
            }
        }
    }
}
