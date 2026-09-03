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
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWebPart : IWebPart
    {
        PartChromeType ChromeType { get; set; }
        string Height { get; set; }
        bool Hidden { get; set; }
        bool AllowClose { get; set; }
        bool AllowEdit { get; set; }
        bool AllowHide { get; set; }
        string ID { get; set; }
        string Title { get; set; }
        string TitleUrl { get; set; }
        string Width { get; set; }
        string ZoneID { get; set; }
        string AuthorizationFilter { get; set; }
        string WebPartTypeID { get; }
        int ZoneIndex { get; }
        string RealWebPartType { get; }
        bool IsClosed { get; }
        Guid StorageKey { get; }

        WebPartExportMode ExportMode { get; set; }

        void SetWebPartProperty(string propertyName, object value);
        string GetWebPartStringProperty(string propertyName);
        void Dispose();

        bool AllowConnect { get; set; }

        bool AllowMinimize { get; set; }

        bool AllowZoneChange { get; set; }

        PartChromeState ChromeState { get; set; }

        System.Web.UI.WebControls.ContentDirection Direction { get; set; }

        WebPartHelpMode HelpMode { get; set; }

        string HelpUrl { get; set; }

        string MissingAssembly { get; set; }
    }
}
