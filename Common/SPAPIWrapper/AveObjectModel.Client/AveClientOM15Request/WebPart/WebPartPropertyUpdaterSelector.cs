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
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client.WebParts;
namespace AveClientOM15Request
{
    internal class WebPartPropertyUpdaterSelector
    {

        public static WebPartPropertyUpdater Select(WebPartDefinition webpartDefinition, AveWebPartBaseInfo webpartBaseInfo)
        {
            IWebPartPropertyExtractor propertyExtractor = WebPartExtractorFactory.Create(webpartBaseInfo.DefinitionXml);
            TypeInfo typeInfo = TypeInfo.Parse(propertyExtractor.TypeFullName);
            AveWebPartType webPartType = AveWebPartTypeMapping.GetWebPartType(typeInfo);

            switch (webPartType)
            {
                case AveWebPartType.TagCloudWebPart:
                    return new TagCloudWebPartUpdater(webpartDefinition, webpartBaseInfo, propertyExtractor);
                case AveWebPartType.SocialCommentWebPart:
                    return new SocialCommentWebPartUpdater(webpartDefinition, webpartBaseInfo, propertyExtractor);
                case AveWebPartType.XsltListViewWebPart:
                    return new XsltListViewWebPart(webpartDefinition, webpartBaseInfo, propertyExtractor);
                default:
                    return new CommonWebPartPropertyUpdater(webpartDefinition, webpartBaseInfo, propertyExtractor);
            }            
        }
    }
}
