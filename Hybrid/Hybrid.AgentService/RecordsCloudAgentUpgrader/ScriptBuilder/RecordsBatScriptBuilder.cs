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
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader
{
    public class RecordsBatScriptBuilder
    {
        private BatchCondition _conditions = BatchCondition.None;

        public string GenerateArguments(params string[] agrs)
        {
            return string.Join(" ", agrs.Select(a => $"\"{a}\""));
        }

        public RecordsBatScriptBuilder EnableRollback()
        {
            _conditions |= BatchCondition.EnableRollback;
            return this;
        }

        public RecordsBatScriptBuilder DisableRollback()
        {
            _conditions &= ~BatchCondition.EnableRollback;
            return this;
        }

        public RecordsBatScriptBuilder EnableKillWorker()
        {
            _conditions |= BatchCondition.EnableKillWorker;
            return this;
        }

        public RecordsBatScriptBuilder DisableKillWorker()
        {
            _conditions &= ~BatchCondition.EnableKillWorker;
            return this;
        }

        public RecordsBatScriptBuilder EnableAutoStartService()
        {
            _conditions |= BatchCondition.EnableAutoStartService;
            return this;
        }

        public RecordsBatScriptBuilder DisableAutoStartService()
        {
            _conditions &= ~BatchCondition.EnableAutoStartService;
            return this;
        }

        public RecordsBatScriptBuilder EnableRequireAdminPermission()
        {
            _conditions |= BatchCondition.EnableRequireAdministrator;
            return this;
        }

        public RecordsBatScriptBuilder DisableRequireAdminPermission()
        {
            _conditions &= ~BatchCondition.EnableRequireAdministrator;
            return this;
        }

        public RecordsBatScriptBuilder EnableParamValidation()
        {
            _conditions |= BatchCondition.EnableParamValidation;
            return this;
        }

        public RecordsBatScriptBuilder DisableParamValidation()
        {
            _conditions &= ~BatchCondition.EnableParamValidation;
            return this;
        }

        public RecordsBatScriptBuilder EnableReapplyServiceAccount()
        {
            _conditions |= BatchCondition.EnableReapplyServiceAccount;
            return this;
        }

        public RecordsBatScriptBuilder DisableReapplyServiceAccount()
        {
            _conditions &= ~BatchCondition.EnableReapplyServiceAccount;
            return this;
        }

        private string Build()
        {
            var sb = new StringBuilder();

            sb.Append(RecordsBatScriptSection.HEADER);
            
            if (_conditions.HasFlag(BatchCondition.EnableRequireAdministrator))
                sb.Append(RecordsBatScriptSection.REQUIRE_ADMIN);
            
            sb.Append(RecordsBatScriptSection.PARAMETERS_DEFINITION);

            if (_conditions.HasFlag(BatchCondition.EnableParamValidation)){
                sb.Append(RecordsBatScriptSection.BASIC_VALIDATION);
                sb.Append(RecordsBatScriptSection.ADVANCED_VALIDATION);
            }

            if (_conditions.HasFlag(BatchCondition.EnableKillWorker))
                sb.Append(RecordsBatScriptSection.PRE_UPGRADE)
                    .Replace(RecordsBatScriptSection.KILL_WORKER_MARK, RecordsBatScriptSection.KILL_WORKER);
            else
                sb.Append(RecordsBatScriptSection.PRE_UPGRADE
                    .Replace(RecordsBatScriptSection.KILL_WORKER_MARK, string.Empty));

            sb.Append(RecordsBatScriptSection.INSTALLATION);
            sb.Append(RecordsBatScriptSection.UPGRADE_FAILED_LABEL);

            if (_conditions.HasFlag(BatchCondition.EnableRollback))
                sb.Append(RecordsBatScriptSection.ROLLBACK_INSTALLATION);

            if (_conditions.HasFlag(BatchCondition.EnableReapplyServiceAccount))
                sb.Append(RecordsBatScriptSection.REAPPLY_SERVICE_ACCOUNT);

            if (_conditions.HasFlag(BatchCondition.EnableAutoStartService))
                sb.Append(RecordsBatScriptSection.START_SERVICE);

            sb.Append(RecordsBatScriptSection.FOOTER);

            return sb.ToString();
        }

        public void SaveToFile(string path)
        {
            var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(path, Build(), utf8WithoutBom);
        }

    }

    public enum BatchCondition
    {
        None = 0,
        EnableRequireAdministrator = 1,
        EnableParamValidation = 2,
        EnableKillWorker = 4,
        EnableRollback = 8,
        EnableReapplyServiceAccount = 16,
        EnableAutoStartService = 32,
    }
}
