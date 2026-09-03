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
using AvePoint.RA.Contract.CustomizeConnector.Model.Columns;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class MultipleChoiceConnectorColumn : IConnectorColumn
    {
        public Contract.TemplateManagement.ColumnType Type => Contract.TemplateManagement.ColumnType.MultipleChoice;

        public async Task<(bool,CustomColumn)> TryConvertToCustomColumnAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            CustomColumn customColumn = null;
            if(valueJson is not List<object> optionObjList || optionObjList.Count == 0)
            {
                return (false, customColumn);
            }

            var optionList = optionObjList.ConvertAll(item => Convert.ToInt32(item));
            var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
            var choiceValues = optionList.ConvertAll(item => new ChoiceColumnValue
            {
                Name = options.First(i => i.Value == item).Name,
                Value = item.ToString()
            });

            customColumn = new CustomColumn
            {
                MultiChoice = choiceValues
            };

            return (true, customColumn);
        }

        public bool DefinitionValidate(CustomizeConnectorColumnInfo columnInfo)
        {
            if (string.IsNullOrEmpty(columnInfo.Extention))
            {
                return false;
            }

            try
            {
                var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
                if (options.Count == 0)
                {
                    return false;
                }

                if (options.Exists(item => item.Value < 1 || item.Order < 1))
                {
                    return false;
                }

                if (options.GroupBy(item => item.Value).Count() != options.Count)
                {
                    return false;
                }

                if (options.GroupBy(item => item.Order).Count() != options.Count)
                {
                    return false;
                }

                if (options.Any(item => string.IsNullOrWhiteSpace(item.Name)))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<CustomizeConnectorDataValidateResult> ValueValidateAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            var optionObjList = valueJson as List<object>;
            if (columnInfo.IsRequired && optionObjList != null && optionObjList.Any())
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if(!columnInfo.IsRequired && !string.IsNullOrWhiteSpace(valueJson?.ToString()) && optionObjList == null)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            if (!columnInfo.IsRequired && (optionObjList == null || !optionObjList.Any()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            var hasIllegalOption = optionObjList.Any(item => !int.TryParse(item?.ToString(), out _));
            if(hasIllegalOption)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }
            ArgumentCheck.NotNull(optionObjList, nameof(optionObjList));
            var optionList = optionObjList.ConvertAll(item => Convert.ToInt32(item));

            if(optionList.Exists(item => item < 1))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
            if(optionList.Exists(item => !options.Exists(i => i.Value == item)))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsMultipleUndefined"), columnInfo.InternalName));
            }

            return CustomizeConnectorDataValidateResult.Validated();
        }

        public async Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, bool forDisplay = true)
        {
            var res = new CustomizeConnectorNameValue<string>
            {
                Name = columnInfo.Name,
                Value = "",
            };

            if(customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                var options = customColumn.MultiChoice.Select(item => item.Name).ToList();
                res.Value = string.Join(", ", options);
            }

            return res;
        }

        public bool TryConvertToRulePolicy(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, out object value)
        {
            if (!customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                value = null;
                return false;
            }

            value = customColumn.MultiChoice;
            return true;
        }
    }
}
