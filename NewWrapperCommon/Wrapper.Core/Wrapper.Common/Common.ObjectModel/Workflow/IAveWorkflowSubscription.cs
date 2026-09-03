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

namespace AvePoint.Wrapper.Common
{
    public interface IAveWorkflowSubscription
    {
        // Fields
        // private readonly IDictionary<string, string> propertyDefinitions;

        // Methods

        //IAveWorkflowSubscription();
        string GetProperty(string name);
        void SetProperty(string name, string value);

        // Properties
        //internal string CanonicalId { get; }
        Guid DefinitionId { get; set; }

        bool Enabled { get; set; }

        Guid EventSourceId { get; set; }

        List<string> EventTypes { get; set; }

        Guid Id { get; set; }

        bool ManualStartBypassesActivationLimit { get; set; }

        string Name { get; set; }

        IDictionary<string, string> PropertyDefinitions { get; }
        bool StatusColumnCreated { get; set; }

        string StatusFieldName { get; set; }

    }
}
