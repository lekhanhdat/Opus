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



namespace AvePoint.ObjectModel.Server19
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Administration;
    #endregion

    class AveElementProvider : IAveElementProvider
    {
        private readonly string mElementProvider_Type = "Microsoft.SharePoint.SPElementProvider";
        private object mElementProvider;

        public AveElementProvider()
        {
            mElementProvider = AveAssemblyUtility.CreateInstance(mElementProvider_Type);
        }

        public AveElementProvider(object elementProvider)
        {
            mElementProvider = elementProvider;
        }

        public IAveElementProvider GetAvailableProvider()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveElementProvider.GetAvailableProvider"))
            {

                return new AveElementProvider(AveAssemblyUtility.InvokeStaticMethod(mElementProvider_Type, "GetAvailableProvider", new Type[] { }, new object[] { }));

            }

        }

        public List<TElementType> QueryForSortedElements<TElementType>(Dictionary<string, string> dictAttrPattern, List<IAveFeatureDefinition> lstfeatdefFeaturesOfInterest, List<TElementType> listofOptionalElementsToQuery, CultureInfo ciElements, int webUIVersion) where TElementType : IAveElementDefinition
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveElementProvider.QueryForSortedElements"))
            {

                List<SPFeatureDefinition> lstSPfeatdefFeaturesOfInterest = null;
                if (lstfeatdefFeaturesOfInterest != null)
                {
                    lstSPfeatdefFeaturesOfInterest = new List<SPFeatureDefinition>();
                    foreach (IAveFeatureDefinition featureDefinition in lstfeatdefFeaturesOfInterest)
                    {
                        lstSPfeatdefFeaturesOfInterest.Add((featureDefinition as AveFeatureDefinition).FeatureDefinition);
                    }
                }
                List<SPElementDefinition> listofSPOptionalElementsToQuery = null;
                if (listofOptionalElementsToQuery != null)
                {
                    listofSPOptionalElementsToQuery = new List<SPElementDefinition>();
                    foreach (IAveElementDefinition elementDefinition in listofOptionalElementsToQuery)
                    {
                        listofSPOptionalElementsToQuery.Add((elementDefinition as AveElementDefinition).ElementDefinition);
                    }
                }

                Type aveGeneralType = typeof(TElementType);
                string typeMapping = string.Empty;
                typeMapping = XmlConfiguration.GetTypeMapping(aveGeneralType.Name);
                Type spGeneralType = AveAssemblyUtility.GetGenerticType(aveGeneralType, typeMapping);

                IList list = AveAssemblyUtility.InvokeGenericMethod(mElementProvider, "QueryForSortedElements", new object[] { dictAttrPattern, lstSPfeatdefFeaturesOfInterest, listofSPOptionalElementsToQuery, ciElements, webUIVersion }, spGeneralType) as IList;

                List<TElementType> sortedElement = new List<TElementType>();
                if (list != null)
                {
                    foreach (SPElementDefinition elementDefinition in list)
                    {
                        sortedElement.Add((TElementType)AveServerAssemblyInit.CreateElement(typeof(IAveElementDefinition), elementDefinition));
                    }
                }
                return sortedElement;

            }

        }
    }
}
