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
namespace AvePoint.Wrapper.Common
{

    using System;

    public class AveSP2010FeatureDefinitions
    {
        public static Guid PublishingSite = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
        public static Guid PublishingWeb = new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb");

        public static Guid StandardSiteFeature = new Guid("b21b090c-c796-4b0f-ac0f-7ef1659c20ae");

        public static Guid PerformancePointServicesSiteFeature = new Guid("a1cb5b7f-e5e9-421b-915f-bf519b0760ef");

        public static Guid Ratings = new Guid("915c240e-a6cc-49b8-8b2c-0bff8b553ed3");

        public static Guid DocumentRouting = new Guid("7AD5272A-2694-4349-953E-EA5EF290E97C");

        #region workflow features

        public static Guid OffWFCommon = new Guid("c9c9515d-e4e2-4001-9050-74f980f93160"); //Microsoft Office Server workflows feature(hidden)

        public static Guid ReviewPublishingSPD = new Guid("a44d2aa3-affc-4d58-8db4-f4a3af053188"); //Publishing Approval Workflow feature
       
        public static Guid ReviewWorkflowsSPD = new Guid("b5934f65-a844-4e67-82e5-92f66aafe912"); //hidden feature

        public static Guid SignaturesWorkflowSPD = new Guid("c4773de6-ba70-4583-b751-2a7b1dc67e3a"); //hidden feature

        public static Guid TranslationWorkflow = new Guid("c6561405-ea03-40a9-a57f-f25472942a22"); //hidden feature

        public static Guid Workflows = new Guid("0af5989a-3aea-4519-8ab0-85d91abe39ff");//Aggregated set of out-of-box workflow features provided by SharePoint.

        public static Guid ExpirationWorkflow = new Guid("c85e5759-f323-4efb-b548-443d2216efb5");//Disposition Approval Workflow

        public static Guid IssueTrackingWorkflow = new Guid("fde5d850-671e-4143-950a-87b473922dc7");//Three State Workflow

        #endregion

        #region nintex site feature

        public static Guid NintexWorkflow = new Guid("0561d315-d5db-4736-929e-26da142812c5");

        public static Guid NintexWorkflowInfoPath = new Guid("80bf3218-7353-11df-af9f-058bdfd72085");

        public static Guid NintexWorkflowContentTypeUpgrade = new Guid("86c83d16-605d-41b4-bfdd-c75947899ac7");

        public static Guid NintexWorkflowWebParts = new Guid("eb657559-be37-4b91-a369-1c201183c779");

        public static Guid NintexWorkflowEnterpriseWebParts = new Guid("53164b55-e60f-4bed-b582-a87da32b92f1"); //NintexWorkflowEnterpriseWebParts

        public static Guid NintexWorkflowLiveSite = new Guid("54668547-c03f-4bb5-aaab-d9568ebaf9c9");

        #endregion

        #region nintex web feature

        public static Guid NintexWorkflowWeb = new Guid("9bf7bf98-5660-498a-9399-bc656a61ed5d");

        public static Guid NintexWorkflowEnterpriseWeb = new Guid("2fb9d5df-2fb5-403d-b155-535c256be1dc");

        #endregion
    }

    public class AveSP2013FeatureDefinitions : AveSP2010FeatureDefinitions
    {
        public static Guid SiteFeed = new Guid("15a572c6-e545-4d32-897a-bab6f5846e18");
        public static Guid AppCatalogSettings = new Guid("f8bea737-255e-4758-ab82-e34bb46f5828");
        public static Guid MySiteMicroBlog = new Guid("ea23650b-0340-4708-b465-441a41c37af7");

    }

    
}
