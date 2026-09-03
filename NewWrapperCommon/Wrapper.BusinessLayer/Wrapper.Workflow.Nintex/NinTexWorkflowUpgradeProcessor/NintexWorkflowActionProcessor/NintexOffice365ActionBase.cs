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

namespace LS.SPWorkflowProcessor
{
    abstract class NintexOffice365ActionBase : NWActionProcessorBase
    {
        protected NintexOffice365ActionBase(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
        }
        protected Property CreateDestnationSharePointUrlProperty(string id, string parameterName, string parameterDescription)
        {
            return new Property
            {
                ID = id,
                DesignerType = "Text",
                DisplayName = "SharePoint Online URL",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name=parameterName,
                        Description=parameterDescription,
                        Required=true,
                        DataType ="String",
                        DesignerType="Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue("String", workflowActionProcessor.Web.Site.Url.Substring(0, workflowActionProcessor.Web.Site.Url.IndexOf(workflowActionProcessor.Web.Site.ServerRelativeUrl,StringComparison.OrdinalIgnoreCase)))
                        }
                    }
                }
            };
        }

        protected Property CreateDestinationSiteURLProperty(string id, string parameterName, string parameterDescription)
        {
            return new Property
            {
                ID = id,
                DesignerType = "Text",
                DisplayName = "Destination site URL",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name=parameterName,//"InputDestinationSiteUrl",
                        Description=parameterDescription,//,
                        Required=true,
                        DataType ="String",
                        DesignerType="Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue
                            {
                                Type="String",
                                Value = new Value(base.workflowActionProcessor.Web.Site.Url)
                            }
                        }
                    }
                }
            };
        }

        protected Property CreateUserNameProperty(string id)
        {
            return new Property
            {
                ID = id,
                DesignerType = "Text",
                DisplayName = "Username",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name="InputUserName",
                        Required=true,
                        DataType ="String",
                        DesignerType="Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue
                            {
                                Type="String",
                                Value = new Value(base.workflowActionProcessor.Web.Site.UserAccountInfo.UserName)
                            }
                        }
                    }
                }
            };
        }

        protected Property CreatePasswordProperty(string id, string password, string description)
        {
            return new Property
            {
                ID = id,
                DesignerType = "Secure",
                DisplayName = "Password",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name="InputPassword",
                        Required=true,
                        DataType ="String",
                        DesignerType="Secure",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue
                            {
                                Type="String",
                                Value = new Value(password)
                            }
                        }
                    }
                }
            };
        }

    }
}
