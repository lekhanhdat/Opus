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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class DateTimeConnectorColumn : IConnectorColumn
    {
        public Contract.TemplateManagement.ColumnType Type => Contract.TemplateManagement.ColumnType.DateTime;

        private readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        public bool DefinitionValidate(CustomizeConnectorColumnInfo columnInfo)
        {
            return true;
        }

        public async Task<(bool, CustomColumn)> TryConvertToCustomColumnAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            CustomColumn customColumn = null;
            if (string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return (false, customColumn);
            }

            var format = await GeneralSettingService.GetDateTimeFormatAsync();
            string[] formats = new[] { format, format.Replace('-', '/') };
            if (!DateTime.TryParseExact(valueJson.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime) || dateTime.Ticks < DateTime.MinValue.Ticks || dateTime.Ticks > DateTime.MaxValue.Ticks)
            {
                return (false, customColumn);
            }

            customColumn = new CustomColumn
            {
                Date = dateTime
            };

            return (true, customColumn);
        }

        public async Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, bool forDisplay = true)
        {
            var res = new CustomizeConnectorNameValue<string>
            {
                Name = columnInfo.Name,
                Value = ""
            };

            if (customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                var dateTimeFormat = forDisplay ? GeneralSettingService.ConvertTiksToDateTime(gls, customColumn.Date.Ticks, true).SimplifyFormatTime :
               GeneralSettingService.ConvertTiksToUTCDateTime(gls, customColumn.Date.Ticks).SimplifyFormatTime;
                res.Value = dateTimeFormat;
            }

            return res;
        }

        public async Task<CustomizeConnectorDataValidateResult> ValueValidateAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            if (columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if (!columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }
            var format = await GeneralSettingService.GetDateTimeFormatAsync();
            var formats = new[] { format, format.Replace('-', '/') };
            if (DateTime.TryParseExact(valueJson.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }
            else
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }
        }

        public bool TryConvertToRulePolicy(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, out object value)
        {
            if (!customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                value = null;
                return false;
            }

            value = customColumn.Date;
            return true;
        }
    }
}
