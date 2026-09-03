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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.ListTitleMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
     public class AveListTitleMappingFactory
    {
         private List<AveListTitleMappingInfo> listTitleMappings;

         public List<AveListTitleMappingInfo> ListTitleMappings
         {
             get { return listTitleMappings; }
         }

         private List<AveListTitleMappingInfo> buildinListTitleMappings;

         public List<AveListTitleMappingInfo> BuildinListTitleMappings
         {
             get { return buildinListTitleMappings; }
         }
        

         public int ListTitleMappingCount
         {
             get { return listTitleMappings.Count; }
         }

         public AveListTitleMappingFactory(ListTitleMappingDataContract contract)
         {
             this.listTitleMappings = AveListTitleMappingConverter.Convert(contract);
             this.buildinListTitleMappings = new List<AveListTitleMappingInfo>();
         }

         public AveListTitleMappingFactory()
         {
             this.listTitleMappings = new List<AveListTitleMappingInfo>();
             this.buildinListTitleMappings = new List<AveListTitleMappingInfo>();
         }

         public string GetMappedTitleFromListTitleMapping(object listOrWeb, string listName, int compatibilityLevel)
         {
             string mappedName = listName;
             mappedName = GetMappedTitle(listOrWeb, listName, listTitleMappings);
             if (!mappedName.Equals(listName, StringComparison.OrdinalIgnoreCase))
             {
                 return mappedName;
             }
             if (compatibilityLevel == 15)
             {
                 mappedName = GetMappedTitle(listOrWeb, listName, buildinListTitleMappings);
             }
             return mappedName;
         }

         private string GetMappedTitle(object listOrWeb, string listName, List<AveListTitleMappingInfo> tempListTitleMapping)
         {
             if (listTitleMappings != null)
             {
                 foreach (AveListTitleMappingInfo mappingInfo in tempListTitleMapping)
                 {
                     if (mappingInfo.ListTitleMappingCondition.CheckCondition(listOrWeb, Guid.Empty))
                     {
                         foreach (AveListTitleMappingValueInfo info in mappingInfo.ListTitleMappingValueInfo)
                         {
                             if (string.Equals(listName, info.SourceName, StringComparison.OrdinalIgnoreCase))
                             {
                                 return info.DestinationName;
                             }
                         }
                     }
                 }
             }
             return listName;
         }
    }
}
