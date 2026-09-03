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
namespace LS.SPWorkflowProcessor
{
    using System;
    using System.Collections.Generic;

    public class BuiltinWorkflowBaseIdCollection
    {
        #region sp2010 builtin workflow template base id

        private const string Three_State = "C6964BFF-BF8D-41AC-AD5E-B61EC111731A";//Three-state

        private const string Disposition_Approval = "DD19A800-37C1-43C0-816D-F8EB5F4A4145";//Disposition Approval

        private const string Schedule_Web_Analytics_Alerts = "1BE2E16E-961B-4898-9DFD-D33D15981EAE";//Schedule Web Analytics Alerts

        private const string Schedule_Web_Analytics_Reports = "49A1FFA8-B55F-486A-8D8B-0963C3027F45";//Schedule Web Analytics Reports

        #region Approval

        private const string Approval_EN = "8AD4D8F0-93A7-4941-9657-CF3706F00409";//Approval for EN

        private const string Approval_JP = "8AD4D8F0-93A7-4941-9657-CF3706F00411";//Approval for JP

        private const string Approval_GE = "8AD4D8F0-93A7-4941-9657-CF3706F00407";//Approval for Ger

        private const string Approval_SP = "8AD4D8F0-93A7-4941-9657-CF3706F00C0A";//Approval for Spanish

        private const string Approval_FR = "8AD4D8F0-93A7-4941-9657-CF3706F0040C";//Approval for French

        #endregion

        #region Collect Feedback

        private const string Collect_Feedback_EN = "3BFB07CB-5C6A-4266-849B-8D6711700409";//Collect Feedback for EN

        private const string Collect_Feedback_JP = "3BFB07CB-5C6A-4266-849B-8D6711700411";//Collect Feedback for JP

        private const string Collect_Feedback_GE = "3BFB07CB-5C6A-4266-849B-8D6711700407";//Collect Feedback for Ger

        private const string Collect_Feedback_SP = "3BFB07CB-5C6A-4266-849B-8D6711700C0A";//Collect Feedback for Spanish

        private const string Collect_Feedback_FR = "3BFB07CB-5C6A-4266-849B-8D671170040C";//Collect Feedback for French

        #endregion

        #region Collect signature

        private const string Collect_Signature_EN = "77C71F43-F403-484B-BCB2-303710E00409";//Collect signature for EN

        private const string Collect_Signature_JP = "77C71F43-F403-484B-BCB2-303710E00411";//Collect signature for JP

        private const string Collect_Signature_GE = "77C71F43-F403-484B-BCB2-303710E00407";//Collect signature for Ger

        private const string Collect_Signature_SP = "77C71F43-F403-484B-BCB2-303710E00C0A";//Collect signature for Spanish

        private const string Collect_Signature_FR = "77C71F43-F403-484B-BCB2-303710E0040C";//Collect signature for French

        #endregion

        #region Publishing Approval

        private const string Publishing_Approval_EN = "E43856D2-1BB4-40ef-B08B-016D89A00409";//Publishing Approval for EN

        private const string Publishing_Approval_JP = "E43856D2-1BB4-40ef-B08B-016D89A00411";//Publishing Approval for JP

        private const string Publishing_Approval_GE = "E43856D2-1BB4-40ef-B08B-016D89A00407";//Publishing Approval for Ger

        private const string Publishing_Approval_SP = "E43856D2-1BB4-40ef-B08B-016D89A00C0A";//Publishing Approval for Spanish

        private const string Publishing_Approval_FR = "E43856D2-1BB4-40ef-B08B-016D89A0040C";//Publishing Approval for French

        #endregion


        private static readonly List<Guid> mThree_StateWorkflowTemplateBaseIdForSP10 = new List<Guid> { new Guid(Three_State) };

        private static readonly List<Guid> mDisposition_ApprovalWorkflowTemplateBaseIdForSP10 = new List<Guid> { new Guid(Disposition_Approval) };

        private static readonly List<Guid> mSchedule_Web_Analytics_AlertsWorkflowTemplateBaseIdForSP10 = new List<Guid> { new Guid(Schedule_Web_Analytics_Alerts) };

        private static readonly List<Guid> mSchedule_Web_Analytics_ReportsWorkflowTemplateBaseIdForSP10 = new List<Guid> { new Guid(Schedule_Web_Analytics_Reports) };

        private static readonly List<Guid> mApprovalWorkflowTemplateBaseIdForSP10 = new List<Guid> 
                        {
                            new Guid(Approval_EN),
                            new Guid(Approval_FR),
                            new Guid(Approval_GE),
                            new Guid(Approval_JP),
                            new Guid(Approval_SP)
                        };

        private static readonly List<Guid> mCollect_FeedbackWorkflowTemplateBaseIdForSP10 = new List<Guid> 
                        {
                            new Guid(Collect_Feedback_EN),
                            new Guid(Collect_Feedback_FR),
                            new Guid(Collect_Feedback_GE),
                            new Guid(Collect_Feedback_JP),
                            new Guid(Collect_Feedback_SP)
                        };

        private static readonly List<Guid> mCollect_SignatureWorkflowTemplateBaseIdForSP10 = new List<Guid> 
                        {
                            new Guid(Collect_Signature_EN),
                            new Guid(Collect_Signature_FR),
                            new Guid(Collect_Signature_GE),
                            new Guid(Collect_Signature_JP),
                            new Guid(Collect_Signature_SP)
                        };

        private static readonly List<Guid> mPublishing_ApprovalWorkflowTemplateBaseIdForSP10 = new List<Guid> 
                        {
                            new Guid(Publishing_Approval_EN),
                            new Guid(Publishing_Approval_FR),
                            new Guid(Publishing_Approval_GE),
                            new Guid(Publishing_Approval_JP),
                            new Guid(Publishing_Approval_SP)
                        };

        public static List<Guid> Three_StateWorkflowTemplateBaseIdForSP10 { get { return mThree_StateWorkflowTemplateBaseIdForSP10; } }

        public static List<Guid> Disposition_ApprovalWorkflowTemplateBaseIdForSP10 { get { return mDisposition_ApprovalWorkflowTemplateBaseIdForSP10; } }
        public static List<Guid> Schedule_Web_Analytics_AlertsWorkflowTemplateBaseIdForSP10 { get { return mSchedule_Web_Analytics_AlertsWorkflowTemplateBaseIdForSP10; } }
        public static List<Guid> Schedule_Web_Analytics_ReportsWorkflowTemplateBaseIdForSP10 { get { return mSchedule_Web_Analytics_ReportsWorkflowTemplateBaseIdForSP10; } }

        public static List<Guid> ApprovalWorkflowTemplateBaseIdForSP10 { get { return mApprovalWorkflowTemplateBaseIdForSP10; } }

        public static List<Guid> Collect_FeedbackWorkflowTemplateBaseIdForSP10 { get { return mCollect_FeedbackWorkflowTemplateBaseIdForSP10; } }

        public static List<Guid> Collect_SignatureWorkflowTemplateBaseIdForSP10 { get { return mCollect_SignatureWorkflowTemplateBaseIdForSP10; } }

        public static List<Guid> Publishing_ApprovalWorkflowTemplateBaseIdForSP10 { get { return mPublishing_ApprovalWorkflowTemplateBaseIdForSP10; } }

        #endregion

        #region sp2007 builtin workflow template base id

        private const string Translation_Management_07 = "B4154DF4-CC53-4C4F-ADEF-1ECF0B7417F6";//Translation Management 2007

        private const string Three_state_07 = "C6964BFF-BF8D-41AC-AD5E-B61EC111731A";//Three-state 2007

        private const string Disposition_Approval_07 = "DD19A800-37C1-43C0-816D-F8EB5F4A4145";//Disposition Approval 2007

        private const string Approval_07 = "C6964BFF-BF8D-41AC-AD5E-B61EC111731C";//Approval 2007

        private const string Collect_Feedback_07 = "46C389A4-6E18-476C-AA17-289B0C79FB8F";//Collect Feedback 2007 

        private const string Collect_Signature_07 = "2F213931-3B93-4F81-B021-3022434A3114";//Collect signature 2007

        private static List<Guid> mBuiltinBaseIdsForSP07 = new List<Guid> 
                        {
                            new Guid(Translation_Management_07),
                            new Guid(Three_state_07),
                            new Guid(Disposition_Approval_07),
                            new Guid(Approval_07),
                            new Guid(Collect_Feedback_07),
                            new Guid(Collect_Signature_07)
                        };

        public static List<Guid> BuiltinBaseIdsForSP07 { get { return mBuiltinBaseIdsForSP07; } }

        #endregion

        #region check is builtin base id

        public static bool IsBuiltinBaseIdForSP2007(Guid baseId)
        {
          return BuiltinBaseIdsForSP07.Contains(baseId);
        }

        public static bool IsBuiltinBaseIdForSP2010(Guid baseId)
        {
            if (ApprovalWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Collect_FeedbackWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Collect_SignatureWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Disposition_ApprovalWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Publishing_ApprovalWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Three_StateWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Schedule_Web_Analytics_AlertsWorkflowTemplateBaseIdForSP10.Contains(baseId)
                || Schedule_Web_Analytics_ReportsWorkflowTemplateBaseIdForSP10.Contains(baseId))
            {
              return true;
            }
            return false;
        }

        public static bool IsBuiltinBaseId(Guid baseId)
        {
            return (IsBuiltinBaseIdForSP2007(baseId) || IsBuiltinBaseIdForSP2010(baseId));
        }

        #endregion
        public static bool Is10ApprovalOrFeedbackBaseId(Guid baseId)
        {
            if (ApprovalWorkflowTemplateBaseIdForSP10.Contains(baseId) || Collect_FeedbackWorkflowTemplateBaseIdForSP10.Contains(baseId))
            {
                return true;
            }
            return false;
        }

        public static bool Is07ApprovalOrFeedbackBaseId(Guid baseId)
        {
            if ((new Guid(Approval_07) == baseId) || (new Guid(Collect_Feedback_07) == baseId))
            {
                return true;
            }
            return false;
        }
    }
}
