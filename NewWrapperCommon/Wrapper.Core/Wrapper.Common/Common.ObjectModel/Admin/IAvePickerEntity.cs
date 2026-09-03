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
using System.Collections;
using System.Xml;
using System.Web.UI;

namespace AvePoint.Wrapper.Common
{
    public interface IAvePickerEntity
    {
        void Clear();
        string ConvertEntitiesToXmlData(IEnumerable entities);
        List<IAvePickerEntity> ParseEntitiesFromXml(string str);
        string ToXmlData();
        string ToXmlData(bool isIE);
        void WriteToXml(XmlTextWriter writer, bool includeMultipleMatches);

        // Properties
        IAveClaim Claim { get; set; }
        string Description { get; set; }
        string DisplayText { get; set; }
        Hashtable EntityData { get; set; }
        List<Pair> EntityDataElements { get; set; }
        string EntityGroupName { get; set; }
        string EntityType { get; set; }
        object HierarchyIdentifier { get; set; }
        bool IsResolved { get; set; }
        string Key { get; set; }
        ArrayList MultipleMatches { get; set; }
        string ProviderDisplayName { get; set; }
        string ProviderName { get; set; }
    }
}
