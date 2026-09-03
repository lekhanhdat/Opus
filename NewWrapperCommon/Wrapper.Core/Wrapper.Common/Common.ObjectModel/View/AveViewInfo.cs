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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveViewInfo
    {
        public Guid Id;
        public string Title;
        public bool IsPersonal;
        public object ViewType;
        public bool? IsDefaultView;
        public bool IsMobileView;
        public bool IsDefaultMobileView;
        public int? UserID;
        public byte BaseViewId;
        public bool Hidden;
        /// <summary>
        /// Add in DocAve 6.10. Only for Online and Server 19
        /// </summary>
        public string ListViewXml;
        /// <summary>
        /// Add in DocAve 6.10. Only for Online and Server 19
        /// </summary>
        public Dictionary<int, List<string>> MappingForSpotlight = new Dictionary<int, List<string>>();

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint view type")]
        public static int GetViewType(string viewType)
        {
            int spViewType = 0;

            if (!string.IsNullOrEmpty(viewType))
            {
                switch (viewType.ToLower(CultureInfo.InvariantCulture))
                {
                    case "html":
                        spViewType = 1;
                        break;
                    case "grid":
                        spViewType = 0x800;
                        break;
                    case "recurrence":
                        spViewType = 0x2001;
                        break;
                    case "chart":
                        spViewType = 0x20000;
                        break;
                    case "calendar":
                        spViewType = 0x80000;
                        break;
                    case "gantt":
                        spViewType = 0x4000000;
                        break;
                    default:
                        spViewType = 0;
                        break;
                }
            }

            return spViewType;
        }

        public string LeafName { get; set; }
    }
}
