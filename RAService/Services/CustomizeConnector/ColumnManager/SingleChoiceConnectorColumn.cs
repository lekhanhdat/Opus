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
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.Contract.CustomizeConnector.Model.Columns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class SingleChoiceConnectorColumn : IConnectorColumn
    {
        public Contract.TemplateManagement.ColumnType Type => Contract.TemplateManagement.ColumnType.SingleChoice;

        public async Task<(bool, CustomColumn) > TryConvertToCustomColumnAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            CustomColumn customColumn = null;
            if(string.IsNullOrEmpty(valueJson?.ToString()) || valueJson.ToString() == "0")
            {
                return (false, customColumn);
            }

            var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
            var optionValue = int.Parse(valueJson.ToString());
            var option = options.First(item => item.Value == optionValue);
            customColumn = new CustomColumn
            {
                Value = option.Value.ToString(),
                Name = option.Name,
            };

            return (true, customColumn);
        }

        public bool DefinitionValidate(CustomizeConnectorColumnInfo columnInfo)
        {
            if(string.IsNullOrEmpty(columnInfo.Extention))
            {
                return false;
            }

            try
            {
                var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
                if(options.Count == 0)
                {
                    return false;
                }

                if(options.Exists(item => item.Value < 1 || item.Order < 1))
                {
                    return false;
                }

                if(options.GroupBy(item => item.Value).Count() != options.Count)
                {
                    return false;
                }

                if(options.GroupBy(item => item.Order).Count() != options.Count)
                {
                    return false;
                }

                if(options.Any(item => string.IsNullOrWhiteSpace(item.Name)))
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
            if(columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if(!columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            if(!int.TryParse(valueJson.ToString(), out var optionValue) || optionValue < 1)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(columnInfo.Extention);
            if(!options.Exists(item => item.Value == optionValue))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsSingleUndefined"), columnInfo.InternalName, optionValue));
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

            if (customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                res.Value = customColumn.Name;
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

            value = customColumn.Name;
            return true;
        }
    }
}
