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

namespace AvePoint.ObjectModel.Common
{
    internal class AveFeatureSerializer : IAveFeatureSerializer
    {
        private AveSite m_Site;
        private AveWeb m_Web;
        private AveFeatureScope m_Scope;
        private AveFeatureImport m_FeatureImportManager = null;
        private IAveRequest m_Request;
        private AveFeatureCollection m_FeatureCollection;

        public AveFeatureSerializer(AveSite site, IAveRequest request)
        {
            m_Site = site;
            m_Scope = AveFeatureScope.Site;
            m_Request = request;
            m_FeatureCollection = site.Features as AveFeatureCollection;
            m_FeatureImportManager = new AveFeatureImport(site, request);
        }

        public AveFeatureSerializer(AveWeb web, IAveRequest request)
        {
            m_Web = web;
            m_Scope = AveFeatureScope.Web;
            m_Request = request;
            m_FeatureCollection = web.Features as AveFeatureCollection;
            m_FeatureImportManager = new AveFeatureImport(web, request);
        }

        public AveFeatureInfoBox GetObjectData()
        {
            AveFeatureInfoBox featureBox = new AveFeatureInfoBox();
            foreach (AveFeature feature in m_FeatureCollection)
            {
                AveFeatureInfo info = new AveFeatureInfo();
                info.Id = feature.DefinitionId;
                info.Scope = m_Scope;
                if (m_Scope == AveFeatureScope.Web)
                {
                     WrapperConfiguration.BPOS_S.WebFeatureDependencies.TryGetValue(feature.DefinitionId,out info.Dependencies);
                }
                else
                {
                    WrapperConfiguration.BPOS_S.SiteFeatureDependencies.TryGetValue(feature.DefinitionId, out info.Dependencies);
                }
                featureBox.FeatureList.Add(info);
            }
            //featureBox.FeatureList.Sort();
            return featureBox;
        }

        private List<Guid> GetDependencyFeatures(Guid featureId)
        {
            List<Guid> dependencyFeatures = new List<Guid>();
            if (featureId == AveSP2010FeatureDefinitions.PublishingWeb)
            {
                dependencyFeatures.Add(AveSP2010FeatureDefinitions.PublishingSite);
                dependencyFeatures.Add(new Guid("22a9ef51-737b-4ff2-9346-694633fe4416"));
            }
            else if (featureId == new Guid("00bfea71-4ea5-48d4-a4ad-7ea5c011abe5"))
            {
                dependencyFeatures.Add(new Guid("00bfea71-d1ce-42de-9c63-a44004ce0104"));
                dependencyFeatures.Add(new Guid("00bfea71-7e6d-4186-9ba8-c047ac750105"));
                dependencyFeatures.Add(new Guid("00bfea71-de22-43b2-a848-c05709900100"));
                dependencyFeatures.Add(new Guid("00bfea71-f381-423d-b9d1-da7a54c50110"));
                dependencyFeatures.Add(new Guid("00bfea71-6a49-43fa-b535-d15c05500108"));
                dependencyFeatures.Add(new Guid("00bfea71-e717-4e80-aa17-d0c71b360101"));
                dependencyFeatures.Add(new Guid("00bfea71-ec85-4903-972d-ebe475780106"));
                dependencyFeatures.Add(new Guid("00bfea71-9549-43f8-b978-e47e54a10600"));
                dependencyFeatures.Add(new Guid("00bfea71-513d-4ca0-96c2-6a47775c0119"));
                dependencyFeatures.Add(new Guid("00bfea71-3a1d-41d3-a0ee-651d11570120"));
                dependencyFeatures.Add(new Guid("00bfea71-5932-4f9c-ad71-1557e5751100"));
                dependencyFeatures.Add(new Guid("00bfea71-2062-426c-90bf-714c59600103"));
                dependencyFeatures.Add(new Guid("00bfea71-f600-43f6-a895-40c0de7b0117"));
                dependencyFeatures.Add(new Guid("00bfea71-52d4-45b3-b544-b1c71b620109"));
                dependencyFeatures.Add(new Guid("00bfea71-eb8a-40b1-80c7-506be7590102"));
                dependencyFeatures.Add(new Guid("00bfea71-a83e-497e-9ba0-7a5c597d0107"));
                dependencyFeatures.Add(new Guid("00bfea71-c796-4402-9f2f-0eb9a6e71b18"));
                dependencyFeatures.Add(new Guid("00bfea71-2d77-4a75-9fca-76516689e21a"));
                dependencyFeatures.Add(new Guid("00bfea71-4ea5-48d4-a4ad-305cf7030140"));
                dependencyFeatures.Add(new Guid("00bfea71-1e1d-4562-b56a-f05371bb0115"));
            }
            else if (featureId == new Guid("0806d127-06e6-447a-980e-2e90b03101b8"))
            {
                dependencyFeatures.Add(new Guid("e8734bb6-be8e-48a1-b036-5a40ff0b8a81"));
                dependencyFeatures.Add(new Guid("0be49fe9-9bc9-409d-abf9-702753bd878d"));
                dependencyFeatures.Add(new Guid("065c78be-5231-477e-a972-14177cc5b3c7"));
                dependencyFeatures.Add(new Guid("2510d73f-7109-4ccc-8a1c-314894deeb3a"));
                dependencyFeatures.Add(new Guid("00bfea71-dbd7-4f72-b8cb-da7ac0440130"));
            }
            else if (featureId == new Guid("99fe402e-89a0-45aa-9163-85342e865dc8"))
            {
                dependencyFeatures.Add(new Guid("e8734bb6-be8e-48a1-b036-5a40ff0b8a81"));
                dependencyFeatures.Add(new Guid("0be49fe9-9bc9-409d-abf9-702753bd878d"));
            }
            else if (featureId == new Guid("00bfea71-d8fe-4fec-8dad-01c19a6e4053"))
            {
                dependencyFeatures.Add(new Guid("00bfea71-c796-4402-9f2f-0eb9a6e71b18"));
            }
            return dependencyFeatures;
        }

        private List<Guid> GetSiteDependencyFeatures(Guid featureId)
        {
            List<Guid> dependencyFeatures = new List<Guid>();
            if (featureId ==  AveSP2010FeatureDefinitions.PublishingSite)
            {
                dependencyFeatures.Add(new Guid("a392da98-270b-4e85-9769-04c0fde267aa"));
                dependencyFeatures.Add(new Guid("aebc918d-b20f-4a11-a1db-9ed84d79c87e"));
                dependencyFeatures.Add(new Guid("89e0306d-453b-4ec5-8d68-42067cdbf98e"));
                dependencyFeatures.Add(new Guid("d3f51be2-38a8-4e44-ba84-940d35be1566"));
                dependencyFeatures.Add(new Guid("4bcccd62-dcaf-46dc-a7d4-e38277ef33f4"));
                dependencyFeatures.Add(new Guid("068bc832-4951-11dc-8314-0800200c9a66"));
                dependencyFeatures.Add(new Guid("a942a218-fa43-4d11-9d85-c01e3e3a37cb"));
                dependencyFeatures.Add(new Guid("915c240e-a6cc-49b8-8b2c-0bff8b553ed3"));
                dependencyFeatures.Add(new Guid("0c8a9a47-22a9-4798-82f1-00e62a96006e"));
                dependencyFeatures.Add(new Guid("5bccb9a4-b903-4fd1-8620-b795fa33c9ba"));
            }
            else if (featureId == new Guid("8581a8a7-cf16-4770-ac54-260265ddb0b2"))
            {
                dependencyFeatures.Add(new Guid("14aafd3a-fcb9-4bb7-9ad7-d8e36b663bbd"));
                dependencyFeatures.Add(new Guid("5f3b0127-2f1d-4cfd-8dd2-85ad1fb00bfc"));
                dependencyFeatures.Add(new Guid("2ed1c45e-a73b-4779-ae81-1524e4de467a"));
                dependencyFeatures.Add(new Guid("0c8a9a47-22a9-4798-82f1-00e62a96006e"));
                dependencyFeatures.Add(new Guid("5bccb9a4-b903-4fd1-8620-b795fa33c9ba"));
                dependencyFeatures.Add(new Guid("c88c4ff1-dbf5-4649-ad9f-c6c426ebcbf5"));
                dependencyFeatures.Add(new Guid("4248e21f-a816-4c88-8cab-79d82201da7b"));
                dependencyFeatures.Add(new Guid("43f41342-1a37-4372-8ca0-b44d881e4434"));
                dependencyFeatures.Add(new Guid("5a979115-6b71-45a5-9881-cdc872051a69"));
                dependencyFeatures.Add(new Guid("3cb475e7-4e87-45eb-a1f3-db96ad7cf313"));
                dependencyFeatures.Add(new Guid("4c42ab64-55af-4c7c-986a-ac216a6e0c0e"));
                dependencyFeatures.Add(new Guid("9fec40ea-a949-407d-be09-6cba26470a0c"));
                dependencyFeatures.Add(new Guid("875d1044-c0cf-4244-8865-d2a0039c2a49"));
                dependencyFeatures.Add(new Guid("5eac763d-fbf5-4d6f-a76b-eded7dd7b0a5"));
                dependencyFeatures.Add(new Guid("6e8f2b8d-d765-4e69-84ea-5702574c11d6"));
                dependencyFeatures.Add(new Guid("744b5fd3-3b09-4da6-9bd1-de18315b045d"));
            }
            else if (featureId == new Guid("b21b090c-c796-4b0f-ac0f-7ef1659c20ae"))
            {
                dependencyFeatures.Add(new Guid("14aafd3a-fcb9-4bb7-9ad7-d8e36b663bbd"));
                dependencyFeatures.Add(new Guid("5f3b0127-2f1d-4cfd-8dd2-85ad1fb00bfc"));
                dependencyFeatures.Add(new Guid("2ed1c45e-a73b-4779-ae81-1524e4de467a"));
                dependencyFeatures.Add(new Guid("0c8a9a47-22a9-4798-82f1-00e62a96006e"));
                dependencyFeatures.Add(new Guid("5bccb9a4-b903-4fd1-8620-b795fa33c9ba"));
            }
            else if (featureId == new Guid("0af5989a-3aea-4519-8ab0-85d91abe39ff"))
            {
                dependencyFeatures.Add(new Guid("c9c9515d-e4e2-4001-9050-74f980f93160"));
                dependencyFeatures.Add(new Guid("b5934f65-a844-4e67-82e5-92f66aafe912"));
                dependencyFeatures.Add(new Guid("c4773de6-ba70-4583-b751-2a7b1dc67e3a"));
                dependencyFeatures.Add(new Guid("c6561405-ea03-40a9-a57f-f25472942a22"));
            }
            else if (featureId == new Guid("c04234f4-13b8-4462-9108-b4f5159beae6"))
            {
                dependencyFeatures.Add(new Guid("2acf27a5-f703-4277-9f5d-24d70110b18b"));
                dependencyFeatures.Add(new Guid("8e947bf0-fe40-4dff-be3d-a8b88112ade6"));
            }
            else if (featureId == new Guid("063c26fa-3ccc-4180-8a84-b6f98e991df3"))
            {
                dependencyFeatures.Add(new Guid("5bccb9a4-b903-4fd1-8620-b795fa33c9ba"));
            }
            else if (featureId == new Guid("695b6570-a48b-4a8e-8ea5-26ea7fc1d162"))
            {
                dependencyFeatures.Add(new Guid("ca7bd552-10b1-4563-85b9-5ed1d39c962a"));
            }
            else if (featureId == new Guid("c85e5759-f323-4efb-b548-443d2216efb5"))
            {
                dependencyFeatures.Add(new Guid("c9c9515d-e4e2-4001-9050-74f980f93160"));
            }
            return dependencyFeatures;
        }

        public object SetObjectData(List<AveFeatureInfo> featureInfoList)
        {
            if (featureInfoList != null)
            {
                m_FeatureImportManager.Run(featureInfoList);
            }
            return null;
        }
    }
}
