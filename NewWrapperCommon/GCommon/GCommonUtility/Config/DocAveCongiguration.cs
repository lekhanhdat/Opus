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


namespace AvePoint.GCommon.Configurations
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    #endregion

    public class DocAveCongiguration
    {
        static DocAveConfigurationSectionHandler docAve;

        static DocAveCongiguration()
        {
            if (docAve == null)
            {
                docAve = ConfigurationManager.GetSection("DocAve".ToLowerInvariant())
                    as DocAveConfigurationSectionHandler;
            }
        }

        public static Agent Agent
        {
            get { return docAve.Agent; }
        }

        public static Diagnostics Diagnostics
        {
            get { return docAve.Diagnostics; }
        }

        public static Media Media
        {
            get { return docAve.Media; }
        }

        public static Manager Manager
        {
            get { return docAve.Manager; }
        }

        public static Report Report
        {
            get { return docAve.Report; }
        }
    }
}
