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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.RA.Common.Util.ExcelPreview
{
    internal sealed class OdsExcelPreviewReader : IExcelPreviewReader
    {
        private const string OfficeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        private const string TableNs = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
        private const string TextNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

        public ExcelPreviewSheetData Read(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
            {
                var contentEntry = archive.GetEntry("content.xml");
                if (contentEntry == null)
                {
                    throw new InvalidDataException("ODS content.xml not found.");
                }

                using (var entryStream = contentEntry.Open())
                using (var reader = CreateXmlReader(entryStream))
                {
                    var rows = new List<string[]>();
                    var firstSheetSeen = false;
                    var insideFirstSheet = false;
                    var headerColumnCount = 0;

                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if (reader.LocalName == "table" && reader.NamespaceURI == TableNs)
                        {
                            if (!firstSheetSeen)
                            {
                                firstSheetSeen = true;
                                insideFirstSheet = true;
                                continue;
                            }

                            if (insideFirstSheet)
                            {
                                break;
                            }
                        }

                        if (!insideFirstSheet || reader.LocalName != "table-row" || reader.NamespaceURI != TableNs)
                        {
                            continue;
                        }

                        var expandedRows = ReadExpandedRows(reader, headerColumnCount, 51 - rows.Count);
                        headerColumnCount = expandedRows.HeaderColumnCount;
                        foreach (var row in expandedRows.Rows)
                        {
                            rows.Add(row);
                            if (rows.Count == 51)
                            {
                                break;
                            }
                        }

                        if (rows.Count == 51)
                        {
                            break;
                        }
                    }

                    if (rows.Count == 0 || headerColumnCount == 0)
                    {
                        return new ExcelPreviewSheetData(new string[0], new List<string[]>());
                    }

                    return new ExcelPreviewSheetData(rows[0], rows.Skip(1).Take(50).ToList());
                }
            }
        }

        private static ExpandedRowsResult ReadExpandedRows(XmlReader rowReader, int currentHeaderColumnCount, int remainingRowCapacity)
        {
            if (remainingRowCapacity <= 0)
            {
                return new ExpandedRowsResult(currentHeaderColumnCount, new List<string[]>());
            }

            var rowRepeatCount = GetRepeatCount(rowReader, "number-rows-repeated");
            var repeatedCells = new List<(string Text, int RepeatCount)>();
            using (var subtree = rowReader.ReadSubtree())
            {
                while (subtree.Read())
                {
                    if (subtree.NodeType != XmlNodeType.Element || subtree.NamespaceURI != TableNs)
                    {
                        continue;
                    }

                    if (subtree.LocalName != "table-cell" && subtree.LocalName != "covered-table-cell")
                    {
                        continue;
                    }

                    repeatedCells.Add((ReadCellText(subtree), GetRepeatCount(subtree, "number-columns-repeated")));
                }
            }

            var headerColumnCount = currentHeaderColumnCount;
            if (headerColumnCount == 0)
            {
                headerColumnCount = GetLastNonEmptyHeaderWidth(repeatedCells);
            }

            if (headerColumnCount == 0)
            {
                return new ExpandedRowsResult(headerColumnCount, new List<string[]>());
            }

            var rowsToMaterialize = Math.Min(rowRepeatCount, remainingRowCapacity);
            var rows = new List<string[]>(rowsToMaterialize);
            for (var i = 0; i < rowsToMaterialize; i++)
            {
                rows.Add(MaterializeRow(repeatedCells, headerColumnCount));
            }

            return new ExpandedRowsResult(headerColumnCount, rows);
        }

        private static int GetLastNonEmptyHeaderWidth(IEnumerable<(string Text, int RepeatCount)> repeatedCells)
        {
            var column = 0;
            var lastNonEmpty = 0;
            foreach (var cell in repeatedCells)
            {
                if (!string.IsNullOrEmpty(cell.Text))
                {
                    lastNonEmpty = column + cell.RepeatCount;
                }

                column += cell.RepeatCount;
            }

            return lastNonEmpty;
        }

        private static string[] MaterializeRow(IEnumerable<(string Text, int RepeatCount)> repeatedCells, int width)
        {
            var row = new string[width];
            var column = 0;
            foreach (var cell in repeatedCells)
            {
                for (var i = 0; i < cell.RepeatCount && column < width; i++, column++)
                {
                    row[column] = cell.Text;
                }

                if (column >= width)
                {
                    break;
                }
            }

            return row;
        }

        private static string ReadCellText(XmlReader reader)
        {
            var fallbackValue = reader.GetAttribute("string-value", OfficeNs)
                ?? reader.GetAttribute("date-value", OfficeNs)
                ?? reader.GetAttribute("time-value", OfficeNs)
                ?? reader.GetAttribute("boolean-value", OfficeNs)
                ?? reader.GetAttribute("value", OfficeNs);
            if (reader.IsEmptyElement)
            {
                return fallbackValue ?? string.Empty;
            }

            var builder = new StringBuilder();
            var hasParagraphContent = false;
            using (var subtree = reader.ReadSubtree())
            {
                while (subtree.Read())
                {
                    if (subtree.NodeType == XmlNodeType.Text || subtree.NodeType == XmlNodeType.CDATA)
                    {
                        builder.Append(subtree.Value);
                        continue;
                    }

                    if (subtree.NodeType != XmlNodeType.Element || subtree.NamespaceURI != TextNs)
                    {
                        continue;
                    }

                    if (subtree.LocalName == "p")
                    {
                        if (hasParagraphContent && builder.Length > 0)
                        {
                            builder.AppendLine();
                        }

                        hasParagraphContent = true;
                    }
                    else if (subtree.LocalName == "s")
                    {
                        var countText = subtree.GetAttribute("c", TextNs);
                        var count = 1;
                        if (!string.IsNullOrWhiteSpace(countText) && int.TryParse(countText, out var parsedCount))
                        {
                            count = parsedCount;
                        }

                        builder.Append(' ', count);
                    }
                    else if (subtree.LocalName == "tab")
                    {
                        builder.Append('\t');
                    }
                    else if (subtree.LocalName == "line-break")
                    {
                        builder.AppendLine();
                    }
                }
            }

            return builder.Length > 0 ? builder.ToString() : fallbackValue ?? string.Empty;
        }

        private static int GetRepeatCount(XmlReader reader, string attributeName)
        {
            var repeatText = reader.GetAttribute(attributeName, TableNs);
            if (!string.IsNullOrWhiteSpace(repeatText)
                && int.TryParse(repeatText, out var repeatCount)
                && repeatCount > 0)
            {
                return repeatCount;
            }

            return 1;
        }

        private static XmlReader CreateXmlReader(Stream stream)
        {
            return XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = false
            });
        }

        private sealed class ExpandedRowsResult
        {
            public ExpandedRowsResult(int headerColumnCount, List<string[]> rows)
            {
                HeaderColumnCount = headerColumnCount;
                Rows = rows;
            }

            public int HeaderColumnCount { get; }

            public List<string[]> Rows { get; }
        }
    }
}
