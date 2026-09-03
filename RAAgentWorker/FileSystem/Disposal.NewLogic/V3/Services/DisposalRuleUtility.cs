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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.FileSystem;
using RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services
{
    /// <summary>
    /// Stateless utility methods for evaluating disposal rules.
    /// </summary>
    public class DisposalRuleUtility
    {
        public static bool IsRemoveRule(Rule rule)
        {
            if (rule?.FSRule?.spMoveOption?.MoveSetting != null
                && rule.FSRule.spMoveOption.MoveDestination != null)
            {
                return false;
            }

            return true;
        }

        public static RuleAction GetRuleAction(Rule rule)
        {
            if (rule?.FSRule?.spMoveOption?.MoveSetting != null)
            {
                return RuleAction.MoveAndDeclare;
            }

            return RuleAction.ArchiveAndRemove;
        }

        public static string GetActionString(int action)
        {
            switch (action)
            {
                case (int)RuleAction.ArchiveAndRemove:
                    return "RM_FS_DisposalAction_Remove";
                case (int)RuleAction.MoveAndDeclare:
                    return "RM_FS_DisposalAction_Move";
                default:
                    return string.Empty;
            }
        }
    }
}

