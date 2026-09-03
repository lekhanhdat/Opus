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
using System.Globalization;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFeatureDefinition : IAvePersistedObject
    {
        IAveFeatureDependencyCollection ActivationDependencies { get; }
        bool AlwaysForceInstall { get; }
        bool AutoActivateInCentralAdmin { get; }
        string DefaultResourceFile { get; }
        AveFeatureScope Scope { get; }
        bool Hidden { get; }
        Guid ID { get; set; }
        string Name { get; set; }
        Guid SolutionId { get; }
        string ReceiverAssembly { get; }
        string RootDirectory { get; }
        Version Version { get; }
        Guid FeatureId { get; }
        IAveFeatureDefinitionContext SPFeatureDefinitionContext { get; }
        string ReceiverClass { get; }
         
        string GetDescription(CultureInfo culture);
        string GetTitle(CultureInfo culture);
        XmlNode GetXmlDefinition(CultureInfo culture);
        IAveElementDefinitionCollection GetElementDefinitions(CultureInfo ciElements);
        bool HasElementType(Type typeElementOfInterest);
        bool SupportsLanguage(CultureInfo culture);

        #region add for SP2013
        int CompatibilityLevel { get; }
        #endregion
    }
}
