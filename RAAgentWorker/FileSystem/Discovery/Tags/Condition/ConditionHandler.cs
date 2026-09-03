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
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Discovery.Tags.Condition
{
    public abstract class ConditionHandler
    {

        private static readonly Dictionary<ConditionCategory, ConditionHandler> s_handlers = new Dictionary<ConditionCategory, ConditionHandler>();

        public abstract ConditionCategory Category { get; }

        public abstract bool Handle(ConditionInfo info, object dataObject);

        static ConditionHandler()
        {
            var handleType = typeof(ConditionHandler);
            var assembly = Assembly.GetAssembly(handleType);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.BaseType?.Name == handleType.Name)
                {
                    var instace = Activator.CreateInstance(type) as ConditionHandler;
                    s_handlers.Add(instace.Category, instace);
                }
            }
        }

        protected ConditionHandler() { }

        public static ConditionHandler Get(ConditionCategory category)
        {
            return s_handlers[category];
        }
    }
}
