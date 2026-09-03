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
using Aspose.Cells;
using AvePoint.RA.Common.Util;
using System.IO.Compression;
using System.Text;

namespace RADiscoveryUnitTest
{
    [TestClass]
    public class ExcelPreviewCsvTests
    {
        [DataTestMethod]
        [DataRow("xlsx")]
        [DataRow("xls")]
        [DataRow("xlsm")]
        [DataRow("xlsb")]
        [DataRow("xltx")]
        [DataRow("xlt")]
        [DataRow("ods")]
        public void ReadExcelPreviewAsCsv_ReturnsHeaderAndTop50Rows(string extension)
        {
            using var stream = CreateWorkbookStream(extension, workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Preview";
                sheet.Cells["A1"].PutValue("Name");
                sheet.Cells["B1"].PutValue("Comment");
                for (var i = 0; i < 55; i++)
                {
                    sheet.Cells[i + 1, 0].PutValue($"Row {i + 1}");
                    sheet.Cells[i + 1, 1].PutValue($"Value {i + 1}");
                }
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, $"sample.{extension}");
            var rows = csv.Split(new[] { "\r\n" }, StringSplitOptions.None).Where(x => x.Length > 0).ToArray();

            Assert.AreEqual(51, rows.Length);
            Assert.AreEqual("Name,Comment", rows[0]);
            Assert.AreEqual("Row 1,Value 1", rows[1]);
            Assert.AreEqual("Row 50,Value 50", rows[50]);
            Assert.IsFalse(csv.Contains("Row 51", StringComparison.Ordinal));
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_PreservesEmptyRowsAndCsvEscaping()
        {
            using var stream = CreateWorkbookStream("xlsx", workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Cells["A1"].PutValue("Name");
                sheet.Cells["B1"].PutValue("Comment");
                sheet.Cells["A2"].PutValue("Alice");
                sheet.Cells["B2"].PutValue("hello, \"world\"");
                sheet.Cells["A4"].PutValue("Bob");
                sheet.Cells["B4"].PutValue("line1\nline2");
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xlsx");
            var rows = csv.Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.AreEqual("Name,Comment", rows[0]);
            Assert.AreEqual("Alice,\"hello, \"\"world\"\"\"", rows[1]);
            Assert.AreEqual(",", rows[2]);
            Assert.AreEqual("Bob,\"line1\nline2\"", rows[3]);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_ThrowsForInvalidArguments()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("x"));

            ExpectThrows<ArgumentException>(() => ExcelUtil.ReadExcelPreviewAsCsv(stream, string.Empty));
            ExpectThrows<ArgumentException>(() => ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample"));
            ExpectThrows<ArgumentOutOfRangeException>(() => ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xlsx", 0));
        }

        [TestMethod]
        public void CanReadExcelPreviewAsCsv_ReturnsExpectedSupportState()
        {
            Assert.IsTrue(ExcelUtil.CanReadExcelPreviewAsCsv("sample.xlsx"));
            Assert.IsTrue(ExcelUtil.CanReadExcelPreviewAsCsv("sample.ods"));
            Assert.IsFalse(ExcelUtil.CanReadExcelPreviewAsCsv("sample.docx"));
            Assert.IsFalse(ExcelUtil.CanReadExcelPreviewAsCsv("sample"));
            Assert.IsFalse(ExcelUtil.CanReadExcelPreviewAsCsv(string.Empty));
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_TruncatesOnlyAtFullRowBoundary()
        {
            using var stream = CreateWorkbookStream("xlsx", workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Cells["A1"].PutValue("Header");
                sheet.Cells["A2"].PutValue(new string('A', 20));
                sheet.Cells["A3"].PutValue(new string('B', 20));
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xlsx", 35);

            Assert.AreEqual("Header\r\nAAAAAAAAAAAAAAAAAAAA\r\n", csv);
            Assert.IsFalse(csv.Contains('B'));
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_ReturnsHeaderOnlyWhenNoDataRowsExist()
        {
            using var stream = CreateWorkbookStream("xlsx", workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Cells["A1"].PutValue("Header");
                sheet.Cells["B1"].PutValue("Value");
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xlsx");

            Assert.AreEqual("Header,Value\r\n", csv);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_FailsFastWhenExtensionDoesNotMatchContent()
        {
            using var stream = CreateWorkbookStream("xlsx", workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Cells["A1"].PutValue("Header");
            });

            ExpectThrows<Exception>(() => ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.ods"));
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_ReturnsEmptyWhenFirstXltxSheetHasNoRows()
        {
            using var stream = CreateWorkbookStream("xltx", workbook =>
            {
                var first = workbook.Worksheets[0];
                first.Name = "First";
                var secondIndex = workbook.Worksheets.Add();
                var second = workbook.Worksheets[secondIndex];
                second.Name = "Second";
                second.Cells["A1"].PutValue("Header");
                second.Cells["A2"].PutValue("ShouldNotAppear");
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xltx");

            Assert.AreEqual(string.Empty, csv);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_UsesLastNonEmptyHeaderCellAsBoundary()
        {
            using var stream = CreateWorkbookStream("xlsx", workbook =>
            {
                var sheet = workbook.Worksheets[0];
                sheet.Cells["A1"].PutValue("A");
                sheet.Cells["B1"].PutValue(string.Empty);
                sheet.Cells["C1"].PutValue("C");
                sheet.Cells["A2"].PutValue("1");
                sheet.Cells["B2"].PutValue(string.Empty);
                sheet.Cells["C2"].PutValue("3");
                sheet.Cells["D2"].PutValue("Ignored");
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.xlsx");

            Assert.AreEqual("A,,C\r\n1,,3\r\n", csv);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_ReturnsEmptyWhenFirstOdsSheetHasNoCells()
        {
            using var stream = CreateWorkbookStream("ods", workbook =>
            {
                workbook.Worksheets[0].Name = "Empty";
                var secondIndex = workbook.Worksheets.Add();
                var second = workbook.Worksheets[secondIndex];
                second.Name = "Data";
                second.Cells["A1"].PutValue("Header");
                second.Cells["A2"].PutValue("ShouldNotAppear");
            });

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.ods");

            Assert.AreEqual(string.Empty, csv);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_ExpandsRepeatedOdsRowsAndStopsAt50PhysicalRows()
        {
            const string odsContent = """
<?xml version="1.0" encoding="UTF-8"?>
<office:document-content xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
 xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
 xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
 <office:body><office:spreadsheet><table:table table:name="Sheet1">
  <table:table-row>
   <table:table-cell office:string-value="A"><text:p>A</text:p></table:table-cell>
   <table:table-cell table:number-columns-repeated="2"/>
   <table:table-cell office:string-value="D"><text:p>D</text:p></table:table-cell>
  </table:table-row>
  <table:table-row table:number-rows-repeated="60">
   <table:table-cell office:string-value="1"><text:p>1</text:p></table:table-cell>
   <table:table-cell office:string-value=""><text:p></text:p></table:table-cell>
   <table:table-cell office:string-value="3"><text:p>3</text:p></table:table-cell>
   <table:table-cell office:string-value="4"><text:p>4</text:p></table:table-cell>
  </table:table-row>
 </table:table></office:spreadsheet></office:body>
</office:document-content>
""";
            using var stream = CreateOdsFromContentXml(odsContent);

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.ods");
            var rows = csv.Split(new[] { "\r\n" }, StringSplitOptions.None).Where(x => x.Length > 0).ToArray();

            Assert.AreEqual(51, rows.Length);
            Assert.AreEqual("A,,,D", rows[0]);
            Assert.AreEqual("1,,3,4", rows[1]);
            Assert.AreEqual("1,,3,4", rows[50]);
        }

        [TestMethod]
        public void ReadExcelPreviewAsCsv_DoesNotFullyMaterializeHugeRepeatedOdsRows()
        {
            const string odsContent = """
<?xml version="1.0" encoding="UTF-8"?>
<office:document-content xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
 xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
 xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
 <office:body><office:spreadsheet><table:table table:name="Sheet1">
  <table:table-row>
   <table:table-cell office:string-value="Header"><text:p>Header</text:p></table:table-cell>
  </table:table-row>
  <table:table-row table:number-rows-repeated="2147483647">
   <table:table-cell office:string-value="Value"><text:p>Value</text:p></table:table-cell>
  </table:table-row>
 </table:table></office:spreadsheet></office:body>
</office:document-content>
""";
            using var stream = CreateOdsFromContentXml(odsContent);

            var csv = ExcelUtil.ReadExcelPreviewAsCsv(stream, "sample.ods");
            var rows = csv.Split(new[] { "\r\n" }, StringSplitOptions.None).Where(x => x.Length > 0).ToArray();

            Assert.AreEqual(51, rows.Length);
            Assert.AreEqual("Header", rows[0]);
            Assert.AreEqual("Value", rows[1]);
            Assert.AreEqual("Value", rows[50]);
        }

        private static MemoryStream CreateWorkbookStream(string extension, Action<Workbook> build)
        {
            var workbook = new Workbook();
            build(workbook);
            var stream = new MemoryStream();
            workbook.Save(stream, extension switch
            {
                "xlsx" => SaveFormat.Xlsx,
                "xls" => SaveFormat.Excel97To2003,
                "xlsm" => SaveFormat.Xlsm,
                "xlsb" => SaveFormat.Xlsb,
                "xltx" => SaveFormat.Xltx,
                "xlt" => SaveFormat.Xlt,
                "ods" => SaveFormat.Ods,
                _ => throw new ArgumentOutOfRangeException(nameof(extension))
            });
            stream.Position = 0;
            return stream;
        }

        private static void ExpectThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        }

        private static MemoryStream CreateOdsFromContentXml(string contentXml)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var mimetype = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
                using (var writer = new StreamWriter(mimetype.Open(), new UTF8Encoding(false)))
                {
                    writer.Write("application/vnd.oasis.opendocument.spreadsheet");
                }

                var content = archive.CreateEntry("content.xml", CompressionLevel.Fastest);
                using (var writer = new StreamWriter(content.Open(), new UTF8Encoding(false)))
                {
                    writer.Write(contentXml);
                }
            }

            stream.Position = 0;
            return stream;
        }
    }
}
