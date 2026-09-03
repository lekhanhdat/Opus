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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Workflow.Builder.Interface;
using AvePoint.RA.Workflow.DisposalReview;
using System;
using System.Activities;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xaml;

namespace AvePoint.RA.Workflow.Builder
{
    public class XamlBuilder
    {
        /// <summary>
        /// Build the workflow xaml.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>xaml string of the workflow definition</returns>
        public static string BuildXaml(WorkflowDefinitionDto definition)
        {
            //convert to xaml string...
            IWorkflowBuilder workflowBuilder = CreateWorkflowBuilder(definition);
            ActivityBuilder activityBuilder = workflowBuilder.BuildActivityBuilder();

            // Serialize the workflow to XAML and store it in a string.
            StringBuilder sb = new StringBuilder();
            StringWriter tw = new StringWriter(sb);
            XamlWriter xw = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(tw, new XamlSchemaContext()));
            XamlServices.Save(xw, activityBuilder);
            string xaml = sb.ToString();

            return xaml;

        }

        /// <summary>
        /// Get the workflow based on the workflow definition xaml string
        /// </summary>
        /// <param name="xamlStr"></param>
        /// <returns>The activity represent the workflow</returns>
        public static Activity LoadActivityFromXaml(string xamlStr)
        {
            ActivityXamlServicesSettings settings = new ActivityXamlServicesSettings
            {
                CompileExpressions = true
            };

            return ActivityXamlServices.Load(new StringReader(xamlStr), settings);
        }

        /// <summary>
        /// Validate if there is any errors or warnings of the workflow definition xaml
        /// </summary>
        /// <param name="xamlStr"></param>
        /// <returns>A list which contains errors or warnings, if there is no errors or warning, returns a blank list without elements</returns>
        public static List<string> ValidateXaml(string xamlStr)
        {
            var activity = LoadActivityFromXaml(xamlStr);
            return ValidateActivity(activity);
        }

        /// <summary>
        /// Validate if there is any errors or warnings of the workflow definition activity
        /// </summary>
        /// <param name="acvitity"></param>
        /// <returns>A list which contains errors or warnings, if there is no errors or warning, returns a blank list without elements</returns>
        public static List<string> ValidateActivity(Activity acvitity)
        {
            List<string> listError = new List<string>();

            var results = ActivityValidationServices.Validate(acvitity);

            if (results.Errors.Count > 0)
            {
                foreach (var error in results.Errors)
                {
                    listError.Add(error.Message);
                }

                foreach (var warning in results.Warnings)
                {
                    listError.Add(warning.Message);
                }
            }

            return listError;
        }


        private static IWorkflowBuilder CreateWorkflowBuilder(WorkflowDefinitionDto definition)
        {
            if (definition.Type == RMWorkflowType.DisposalReview) return new DisposalReviewWorkflowBuilder(definition);

            throw new Exception($"No workflow builder defined for type : {definition.Type}");
        }
    }
}
