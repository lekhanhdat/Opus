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
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils;

public class RMDiscoveryOffice365SiteDataExportor : IAsyncDisposable
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteDataExportor));
    
    private readonly Dictionary<string, string> _columNameMappingI18NDictionary;
    
    private readonly RMDiscoveryOffice365SiteInfo _siteInfo;
    
    private readonly string _csvFilePath;

    private FileStream _fileStream;

    private StreamWriter _writer;
    
    private readonly List<string> _listLongTypeNotNeedConvert = ["CreatedMonth", "ModifiedMonth", "HistoryVersionsCount", "ItemId"];

    private readonly int _recordsInOneSheet;

    private int _currentCountInSheet;

    private readonly IEnumerable<string> _headerNames;

    private int _sheetIndex;
    
    private readonly string _fileExtension = ".csv";
    
    public RMDiscoveryOffice365SiteDataExportor(string folderPath, RMDiscoveryOffice365SiteInfo siteInfo, Dictionary<string, string> columNameMappingI18NDictionary, int recordsInOneSheet)
    {
        _columNameMappingI18NDictionary = columNameMappingI18NDictionary;
        _csvFilePath = folderPath + Path.DirectorySeparatorChar + GenerateSitePath(siteInfo.Url);
        _fileStream = new FileStream(_csvFilePath + _fileExtension, FileMode.Create, FileAccess.Write);
        _writer = new StreamWriter(_fileStream, new UTF8Encoding(true)); // UTF-8 BOM
        _siteInfo = siteInfo;
        _headerNames = columNameMappingI18NDictionary.Values;
        _recordsInOneSheet =  recordsInOneSheet == 0 ? 1_000_000 : recordsInOneSheet;
    }

    public async Task WriteHeaderAsync()
    {
        await _writer.WriteLineAsync(string.Join(',', _headerNames));
        _currentCountInSheet++;
    }
    
    public async Task WriteAsync(ExpandoObject item)
    {
        try
        {
            if (_currentCountInSheet > _recordsInOneSheet)
            {
               await CreateNewSheet();
            }
            var generatedItem = GenerateData(item);
            await _writer.WriteLineAsync(string.Join(',', generatedItem));
            _currentCountInSheet++;
        }
        catch (Exception ex)
        {
            _logger.Error($"Occured error while writing data to CSV of site {_siteInfo.Url}, {_siteInfo.Id}, {ex}");
        }
    }

    private async Task CreateNewSheet()
    {
        await DisposeAsync();
        _sheetIndex++;
        var newSheet = _csvFilePath + $"_{_sheetIndex}" + _fileExtension;
        _fileStream = new FileStream(newSheet, FileMode.Create, FileAccess.Write);
        _writer = new StreamWriter(_fileStream, new UTF8Encoding(true)); // UTF-8 BOM
        _currentCountInSheet = 0;
        await WriteHeaderAsync();
    }

    private string[] GenerateData(ExpandoObject item)
    {
        var res = new List<string>();
        
        foreach (var (internalName, displayName) in _columNameMappingI18NDictionary)
        {
            var contentValue = item.FirstOrDefault(column => column.Key.EqualsIgnoreCase(internalName)).Value;
            res.Add(ConvertValue(internalName, contentValue));
        }

        return res.ToArray();
    }

    private string ConvertValue(string headerName, object value)
    {
        return value switch
        {
            null => "",
            not null when headerName == "SPObjectType" => I18NEntity.GetString("RM_JS_JM_DiscoveryFileType"),
            long longValue when !_listLongTypeNotNeedConvert.Contains(headerName) => (longValue / 1024.0).ToString("F2"),
            ExpandoObject tempObject => (tempObject.GetValue<long>("total_size") / 1024.0).ToString("F2"),
            _ => $"\"{value.ToString()}\""
        };
    }

    public void CheckIfMoreThanOneSheet()
    {
        if (_sheetIndex > 0)
        {
            var currentPath = _csvFilePath + _fileExtension;
            var currentFileInfo = new FileInfo(currentPath);
            currentFileInfo.MoveTo(_csvFilePath + "_0" + _fileExtension);
        }
    }
    
    private string GenerateSitePath(string siteUrl)
    {
        const char slash = '/';
        const char hash = '#';
        const string replaceChar1 = "://";
        const string replaceChar2 = ":";
        
        string ReplaceFirst(string currentValue, string oldValue, string newValue)
        {
            var num = currentValue.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            return num < 0 ? currentValue : currentValue.Remove(num, oldValue.Length).Insert(num, newValue);
        }
        
        var siteUri = new Uri(siteUrl);

        var tempPath = ReplaceFirst(siteUri.AbsoluteUri,replaceChar1, hash.ToString());
        if (tempPath.IndexOf(replaceChar2, StringComparison.Ordinal) > 0)
        {
            tempPath = ReplaceFirst(tempPath,replaceChar2, hash.ToString());
        }

        return tempPath.Replace(slash, hash);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await _fileStream.DisposeAsync();
    }
}