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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class TaxonomyConnectorColumn : IConnectorColumn
    {
        public Contract.TemplateManagement.ColumnType Type => Contract.TemplateManagement.ColumnType.Taxonomy;

        private static readonly char TermPathSeparator = '|';

        private readonly ITermDao TermDao = PlatformWindsorManager.GetService<ITermDao>();

        private readonly List<RMTerm> TermInfos;

        private readonly ConcurrentDictionary<string, RMTerm> TermFullPathMapping = new();

        public TaxonomyConnectorColumn()
        {
            TermInfos = TermDao.GetAllNotRemoveTermsForce();
        }

        public bool DefinitionValidate(CustomizeConnectorColumnInfo columnInfo)
        {
            return true;
        }

        public async Task<(bool, CustomColumn)> TryConvertToCustomColumnAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            CustomColumn customColumn = null;
            if(string.IsNullOrWhiteSpace(valueJson?.ToString()))
            {
                return (false, customColumn);
            }

            var termFullPath = valueJson.ToString().Replace(TermPathSeparator, '/');

            if(TermFullPathMapping.TryGetValue(termFullPath, out var termInfo))
            {
                customColumn = new CustomColumn
                {
                    Id = termInfo.UniqueId.ToString(),
                    Name = termInfo.Name
                };

                return (true, customColumn);
            }

            return (false, customColumn);
        }

        public async Task<CustomizeConnectorDataValidateResult> ValueValidateAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            if(columnInfo.IsRequired && string.IsNullOrWhiteSpace(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if(!columnInfo.IsRequired && string.IsNullOrWhiteSpace(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            var termFullPath = valueJson.ToString();
            if(!termFullPath.Contains(TermPathSeparator))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            var termName = termFullPath.Split(TermPathSeparator, StringSplitOptions.RemoveEmptyEntries).Last();
            if(string.IsNullOrWhiteSpace(termName))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            var convertedTermFullPath = termFullPath.Replace(TermPathSeparator, '/');

            if(TermFullPathMapping.ContainsKey(convertedTermFullPath))
            {
                var term = TermFullPathMapping[convertedTermFullPath];
                if (!term.IsDeprecated && TermIsInTime(term.TermExpirationFrom, term.TermExpirationTo))
                {
                    return CustomizeConnectorDataValidateResult.Validated();
                }
                else
                {
                    return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_TermIsExpired"), columnInfo.InternalName, termFullPath));
                }
            }

            var matchedTermInfoes = TermInfos.Where(item => item.Name == termName).ToList();
            if(!matchedTermInfoes.Any())
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsExistTerm"), columnInfo.InternalName, termFullPath));
            }

            foreach(var termInfo in matchedTermInfoes)
            {
                var fullPath = TermDao.GetTermNamesPathByTermId(termInfo.UniqueId);
                TermFullPathMapping.TryAdd(fullPath, termInfo);
                if(fullPath == convertedTermFullPath)
                {
                    var term = TermFullPathMapping[convertedTermFullPath];
                    if (!term.IsDeprecated && TermIsInTime(term.TermExpirationFrom, term.TermExpirationTo))
                    {
                        return CustomizeConnectorDataValidateResult.Validated();
                    }
                    else
                    {
                        return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_TermIsExpired"), columnInfo.InternalName, termFullPath));
                    }                    
                }
            }

            return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsExistTerm"), columnInfo.InternalName, termFullPath));
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
                var termId = new Guid(customColumn.Id);
                var fullPath = TermDao.GetTermNamesPathByTermId(termId);
                res.Value = fullPath;
            }

            return res;
        }

        private static bool TermIsInTime(long TermExpirationFrom, long TermExpirationTo)
        {
            if (TermExpirationFrom == 0 && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom == 0 && TermExpirationTo > DateTime.UtcNow.Ticks)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && TermExpirationTo == 0)
            {
                return true;
            }
            else if (TermExpirationFrom <= DateTime.UtcNow.Ticks && DateTime.UtcNow.Ticks <= TermExpirationTo)
            {
                return true;
            }
            return false;
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
