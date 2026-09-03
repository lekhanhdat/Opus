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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.Services;
using DocumentFormat.OpenXml;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Vml.Office;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using AvePoint.RA.Contract.TemplateManagement;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace AvePoint.RA.Common.Util
{
    public class ReportWordUtil : IDisposable
    {
        private IRALogger _logger = RALogger.GetInstance(typeof(ReportWordUtil));
        private string filepPath = null;
        private WordprocessingDocument wordprocessingDocument = null;
        private List<BookmarkStart> allBookmarkStart = null;
        private static int _drawingIdSeed = 0;
        private static uint NextDrawingId()
        {
            return (uint)System.Threading.Interlocked.Increment(ref _drawingIdSeed);
        }
        
        public ReportWordUtil(string filePath)
        {
            this.filepPath = filePath;
            wordprocessingDocument = WordprocessingDocument.Open(filepPath, true);
            allBookmarkStart = wordprocessingDocument.MainDocumentPart.RootElement.Descendants<BookmarkStart>().ToList();
        }

        /// <summary>
        /// 把模板word template copy到生成的word文档
        /// </summary>
        /// <param name="SourceFile"></param>
        /// <param name="ObjectFile"></param>
        public static void CopyFile(string SourceFile, string ObjectFile)
        {
            string sourceFile = SourceFile;
            string objectFile = ObjectFile;
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, objectFile, true);
            }
        }

        public void CreateTable(string tableBookMark, List<ExportBarcodeDataModel> models)
        {
            Body body = wordprocessingDocument.MainDocumentPart.Document.Body;
            List<BookmarkStart> allBookmarkStart = wordprocessingDocument.MainDocumentPart.RootElement.Descendants<BookmarkStart>().ToList();
            //通过索引获得table
            //var table = body.Elements<DocumentFormat.OpenXml.Wordprocessing.Table>().ElementAt(configModel.Index);
            //通过标签获得table
            BookmarkStart bookmarkStart = allBookmarkStart.Find(a => a.Name.Value == tableBookMark);
            if (bookmarkStart == null)
                return;
            var table = bookmarkStart.Parent.Parent.Parent.Parent;

            int i = 0;
            OpenXmlElement temp1;
            foreach (ExportBarcodeDataModel model in models)
            {
                i++;
                if (i <= 3)
                {
                    OpenXmlElement temp = CopyTable(table, body);
                    FillInTable(model, temp);
                }
                if (i > 3)
                {
                    if (i % 3 == 1)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph newPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run
                        (new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page },
                        new DocumentFormat.OpenXml.Wordprocessing.Text("")));
                        body.Append(newPara);
                        DocumentFormat.OpenXml.Wordprocessing.Break newLine = body.InsertAfter(new DocumentFormat.OpenXml.Wordprocessing.Break(), body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().Last());
                        temp1 = CopyTable(table, body, newLine);
                        FillInTable(model, temp1);
                    }
                    else
                    {
                        OpenXmlElement temp = CopyTable(table, body);
                        FillInTable(model, temp);
                    }
                }
            }
            table.Remove();
            RemoveEmptyParagraphs();
        }

        private void FillInTable(ExportBarcodeDataModel tableDataModel, OpenXmlElement table)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableRow row1 = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ElementAt(0);
            if (tableDataModel.Image != null)
            {
                DocumentFormat.OpenXml.Wordprocessing.TableCell cell1 = row1.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(0);
                MainDocumentPart mainPart1 = wordprocessingDocument.MainDocumentPart;
                ImagePart imagePart1 = mainPart1.AddImagePart(ImagePartType.Jpeg);
                Stream stream1 = new MemoryStream(tableDataModel.Image);
                imagePart1.FeedData(stream1);
                var picture1 = AssembleImage(tableDataModel.Barcode, wordprocessingDocument.MainDocumentPart.GetIdOfPart(imagePart1), true, tableDataModel.ImageWidth, tableDataModel.ImageHeight, 0, 0);
                var pic = cell1.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
                cell1.ReplaceChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(picture1)), pic);
            }

            //DocumentFormat.OpenXml.Wordprocessing.TableRow row1 = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ElementAt(0);
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell2 = row1.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(1);
            InsertBoldCell(cell2, tableDataModel.ColumnB, "20");
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell3 = row1.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(2);
            InsertBoldCell(cell3, tableDataModel.ColumnC, "20");


            DocumentFormat.OpenXml.Wordprocessing.TableRow row2 = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ElementAt(1);
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell4 = row2.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(1);
            InsertBoldCellForDes(cell4, tableDataModel.ColumnDValue, "20");


            DocumentFormat.OpenXml.Wordprocessing.TableRow row3 = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ElementAt(2);
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell5 = row3.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(1);
            InsertBoldCell(cell5, tableDataModel.ColumnE, "20");
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell6 = row3.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(2);
            InsertBoldCell(cell6, tableDataModel.ColumnF, "20");

            if (!string.IsNullOrEmpty(tableDataModel.UniqueId))
            {
                Int64Value cx = 0;
                Int64Value cy = 0;
                DocumentFormat.OpenXml.Wordprocessing.TableRow row4 = table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>().ElementAt(3);
                DocumentFormat.OpenXml.Wordprocessing.TableCell cell7 = row4.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().ElementAt(2);
                MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;
                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                var bi = new BarCodeImageInfo();
                var barcodeUtil = new BarcodeUtil();
                if (barcodeUtil.PreCheckBarcodeInfo(tableDataModel.Barcode))
                {
                    using (var barcodeStream = barcodeUtil.GetBarcodeStream(tableDataModel.Barcode, ref bi))
                    {
                        barcodeStream.Position = 0;
                        imagePart.FeedData(barcodeStream);
                        var w = bi.Width;
                        var h = bi.Height;
                        var hr = bi.HR;
                        var vr = bi.VR;
                        cx = (long)w * (long)((float)731520 / hr);
                        cy = (long)h * (long)((float)731520 / vr);
                    }

                    var picture = AssembleImage(tableDataModel.Barcode, wordprocessingDocument.MainDocumentPart.GetIdOfPart(imagePart), false, 0, 0, cx, cy);
                    cell7.RemoveAllChildren();
                    cell7.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(picture)));
                }
            }
        }

        /*private void InsertCell(DocumentFormat.OpenXml.Wordprocessing.TableCell cell, string value, string size)
        {
            cell.RemoveChild(cell.LastChild);
            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(GetNormalRun(new DocumentFormat.OpenXml.Wordprocessing.Text(value), size)));
        }*/
        private void InsertBoldCell(DocumentFormat.OpenXml.Wordprocessing.TableCell cell, string value, string size)
        {
            cell.RemoveChild(cell.LastChild);
            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(GetBoldRun(new DocumentFormat.OpenXml.Wordprocessing.Text(value), size));
            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties property = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
            //property.WordWrap = new WordWrap();//{ Val = OnOffValue.FromBoolean(false) }
            //property.WordWrap.Val = new OnOffValue(false);
            property.Justification = new Justification();
            property.Justification.Val = JustificationValues.Left;
            paragraph.ParagraphProperties = property;
            cell.Append(paragraph);
            //cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(GetBoldRun(new DocumentFormat.OpenXml.Wordprocessing.Text(value), size)));
        }

        private void InsertBoldCellForDes(DocumentFormat.OpenXml.Wordprocessing.TableCell cell, Dictionary<string, string> dicString, string size)
        {
            if (!dicString.IsNullOrEmpty())
            {
                cell.RemoveChild(cell.LastChild);
                foreach (var columnstring in dicString)
                {
                    string value = columnstring.Value;
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(GetNormalRunForDes(new DocumentFormat.OpenXml.Wordprocessing.Text(value), size));
                    DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties property = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
                    //property.WordWrap = new WordWrap();//{ Val = OnOffValue.FromBoolean(false) }
                    //property.WordWrap.Val = new OnOffValue(false);
                    property.Justification = new Justification();
                    property.Justification.Val = JustificationValues.Left;
                    paragraph.ParagraphProperties = property;
                    cell.Append(paragraph);
                    //cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(GetNormalRunForDes(new DocumentFormat.OpenXml.Wordprocessing.Text(value), size)));
                }
            }
        }
        private DocumentFormat.OpenXml.Wordprocessing.Run GetBoldRun(DocumentFormat.OpenXml.Wordprocessing.Text text, string size)
        {
            DocumentFormat.OpenXml.Wordprocessing.Run run = new DocumentFormat.OpenXml.Wordprocessing.Run(text);
            DocumentFormat.OpenXml.Wordprocessing.RunProperties property = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            property.RunFonts = new RunFonts() { Ascii = "Open Sans" };
            property.Bold = new Bold();
            property.FontSize = new FontSize();
            property.FontSize.Val = size;
            run.RunProperties = property;
            return run;
        }

        private DocumentFormat.OpenXml.Wordprocessing.Run GetNormalRunForDes(DocumentFormat.OpenXml.Wordprocessing.Text text, string size)
        {
            //DocumentFormat.OpenXml.Wordprocessing.Run run = new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page },
            //text);
            DocumentFormat.OpenXml.Wordprocessing.Run run= new DocumentFormat.OpenXml.Wordprocessing.Run(text);
            DocumentFormat.OpenXml.Wordprocessing.RunProperties property = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            property.RunFonts = new RunFonts() { Ascii = "Open Sans" };
            property.FontSize = new FontSize();
            property.FontSize.Val = size;
            run.RunProperties = property;
            return run;
        }


        public OpenXmlElement CopyTable(OpenXmlElement baseTable, Body body)
        {
            DocumentFormat.OpenXml.Wordprocessing.Break newLine = body.InsertAfter(new DocumentFormat.OpenXml.Wordprocessing.Break(), body.Elements<DocumentFormat.OpenXml.Wordprocessing.Table>().Last());
            OpenXmlElement temp = body.InsertAfter(baseTable.CloneNode(true), newLine);
            return temp;
        }

        public OpenXmlElement CopyTable(OpenXmlElement baseTable, Body body, DocumentFormat.OpenXml.Wordprocessing.Break newLine)
        {
            //DocumentFormat.OpenXml.Wordprocessing.Break newLine = body.InsertAfter(new DocumentFormat.OpenXml.Wordprocessing.Break(), body.Elements<DocumentFormat.OpenXml.Wordprocessing.Table>().Last());
            OpenXmlElement temp = body.InsertAfter(baseTable.CloneNode(true), newLine);
            return temp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="relationshipId"></param>
        /// <param name="isTemplatePic">判断A区域的</param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHegiht"></param>
        /// <returns></returns>
        private Drawing AssembleImage(string Name, string relationshipId, bool isTemplatePic, int imageWidth, int imageHegiht, Int64Value cx, Int64Value cy)
        {
            double topEdge = 0;
            double leftEdge = 0;
            double width = 2040000L;
            double heigh = 504000L;
             
            if (isTemplatePic)
            {
                width = 1600000L;
                heigh = 1300000L;
                if (imageWidth != 0 && imageHegiht != 0)
                {
                    if (imageWidth > imageHegiht)
                    {
                        heigh = ((double)imageHegiht / imageWidth) * width;
                    }
                    else if (imageWidth < imageHegiht)
                    {
                        width = ((double)imageWidth / imageHegiht) * heigh;
                    }
                }
                if (1600000L - width > 0)
                {
                    leftEdge = (1600000L - width) / 2;
                }
                if (1300000L - heigh > 0)
                {
                    topEdge = (1300000L - heigh) / 2;
                }

            }
            Drawing element =
              new Drawing(
                new Inline(
                  new Extent() { Cx = isTemplatePic ? (Int64Value)width : cx, Cy = isTemplatePic ? (Int64Value)heigh : cy }, // 调节图片大小
                                                                                                                             //new Extent() { Cx = (Int64Value)width, Cy =  (Int64Value)heigh}, // 调节图片大小
                  new EffectExtent()
                  {
                      LeftEdge = (Int64Value)leftEdge,
                      TopEdge = (Int64Value)topEdge,
                      RightEdge = 0L,
                      BottomEdge = 0L
                  },
                  new DocProperties()
                  {
                      Id = (UInt32Value)NextDrawingId(),
                      Name = Name
                  },
                  new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                    new GraphicFrameLocks() { NoChangeAspect = true }),
                  new Graphic(
                    new GraphicData(
                      new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                        new PIC.NonVisualPictureProperties(
                          new PIC.NonVisualDrawingProperties()
                          {
                              Id = (UInt32Value)NextDrawingId(),
                              Name = Name + ".jpg"
                          },
                          new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                        new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                          new DocumentFormat.OpenXml.Drawing.Blip(
                            new DocumentFormat.OpenXml.Drawing.BlipExtensionList(
                              new DocumentFormat.OpenXml.Drawing.BlipExtension()
                              {
                                  Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                              })
                          )
                          {
                              Embed = relationshipId,
                              CompressionState =
                              DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print
                          },
                          new DocumentFormat.OpenXml.Drawing.Stretch(
                            new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                        new PIC.ShapeProperties(
                          new DocumentFormat.OpenXml.Drawing.Transform2D(
                            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                            //new DocumentFormat.OpenXml.Drawing.Extents() { Cx = (Int64Value)width, Cy = (Int64Value)heigh}), //与上面的对准
                            new DocumentFormat.OpenXml.Drawing.Extents() { Cx = isTemplatePic ? (Int64Value)width : cx, Cy = isTemplatePic ? (Int64Value)heigh : cy }), //与上面的对准
                          new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                            new DocumentFormat.OpenXml.Drawing.AdjustValueList()
                          )
                          { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                )
                {
                    DistanceFromTop = (UInt32Value)0U,
                    DistanceFromBottom = (UInt32Value)0U,
                    DistanceFromLeft = (UInt32Value)0U,
                    DistanceFromRight = (UInt32Value)0U,
                    //EditId = "50D07946"
                });
            return element;
        }

        public void Dispose()
        {
            if (wordprocessingDocument != null)
            {
                wordprocessingDocument.Dispose();
            }
        }

        public static void CopyTemplateAndFillVml(string templatePath, string outputPath, List<LabelItem> labels, BarcodeTemplateLabelType labelType = BarcodeTemplateLabelType.Label_95x65)
        {
            try
            {
                CopyFile(templatePath, outputPath);
                using var document = WordprocessingDocument.Open(outputPath, true);
                ReportWordUtil.FillLabelsToVmlShapes(document, labels, labelType);
                document.MainDocumentPart.Document.Save();
            }
            catch (Exception)
            {
            }
        }

        public static void FillLabelsToVmlShapes(WordprocessingDocument doc, List<LabelItem> labels, BarcodeTemplateLabelType labelType = BarcodeTemplateLabelType.Label_95x65)
        {
            if (doc == null || labels == null) return;
            var mainPart = doc.MainDocumentPart;
            var body = mainPart?.Document?.Body;
            if (body == null) return;
            var roundRects = body.Descendants<DocumentFormat.OpenXml.Vml.RoundRectangle>().ToList();
            int shapesPerPage = roundRects.Count;
            if (shapesPerPage == 0) return;
            int totalPages = (int)Math.Ceiling((double)labels.Count / shapesPerPage);
            if (totalPages > 1)
            {
                DuplicateFirstPageContent(body, totalPages, labelType);
                roundRects = body.Descendants<DocumentFormat.OpenXml.Vml.RoundRectangle>().ToList();
            }
            int fillCount = Math.Min(labels.Count, roundRects.Count);
            for (int i = 0; i < fillCount; i++)
            {
                var label = labels[i];
                var shape = roundRects[i];
                var textBox = shape.Elements<DocumentFormat.OpenXml.Vml.TextBox>().FirstOrDefault();
                if (textBox == null)
                {
                    textBox = new DocumentFormat.OpenXml.Vml.TextBox();
                    shape.AppendChild(textBox);
                }
                textBox.RemoveAllChildren();
                textBox.Inset = "2pt,2pt,2pt,2pt";
                textBox.Style = "v-text-anchor:middle";
                var content = new DocumentFormat.OpenXml.Wordprocessing.TextBoxContent();

                // --- Dynamic height extraction ---
                int totalHeightTwips = 3900; // fallback default
                var styleAttr = shape.Style?.Value;
                if (!string.IsNullOrEmpty(styleAttr))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(styleAttr, @"height:([0-9.]+)(cm|pt|in|mm|px)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        double val = double.Parse(match.Groups[1].Value);
                        string unit = match.Groups[2].Value.ToLower();
                        switch (unit)
                        {
                            case "cm": totalHeightTwips = (int)(val * 567); break;
                            case "mm": totalHeightTwips = (int)(val * 56.7); break;
                            case "in": totalHeightTwips = (int)(val * 1440); break;
                            case "pt": totalHeightTwips = (int)(val * 20); break;
                            case "px": totalHeightTwips = (int)(val * 15); break; // rough px->twips
                        }
                    }
                }
                int rowHeightTwips = totalHeightTwips / 3;
                ComposeLabelContentForVmlRich(content, label, mainPart, rowHeightTwips);
                textBox.Append(content);
            }
        }

        public static int GetRoundRectCount(WordprocessingDocument doc)
        {
            try
            {
                var body = doc?.MainDocumentPart?.Document?.Body;
                if (body == null) return 0;
                return body.Descendants<DocumentFormat.OpenXml.Vml.RoundRectangle>().Count();
            }
            catch
            {
                return 0;
            }
        }

        public static int GetRoundRectCount(string docPath)
        {
            if (string.IsNullOrWhiteSpace(docPath) || !System.IO.File.Exists(docPath)) return 0;
            try
            {
                using var doc = WordprocessingDocument.Open(docPath, false);
                return GetRoundRectCount(doc);
            }
            catch
            {
                return 0;
            }
        }

        private static void ComposeLabelContentForVmlRich(DocumentFormat.OpenXml.Wordprocessing.TextBoxContent content, LabelItem label, MainDocumentPart mainPart, int rowHeightTwips)
        {
            content.RemoveAllChildren();
            var topProps = label.Properties?.Where(p => p.Position == Contract.TemplateManagement.BarcodeTemplatePosition.Above).ToList() ?? new List<PropertyItem>();
            var bottomProps = label.Properties?.Where(p => p.Position == Contract.TemplateManagement.BarcodeTemplatePosition.Under).ToList() ?? new List<PropertyItem>();
            int totalHeightTwips = rowHeightTwips * 3;
            const int BarcodeSidePaddingTwips = 80;
            const int MinSectionHeightTwips = 40;

            int barcodeHeightTwips = 0;
            Drawing barcodeDrawing = null;
            if (!string.IsNullOrWhiteSpace(label.Barcode))
            {
                try
                {
                    Int64Value cx = 0;
                    Int64Value cy = 0;
                    var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
                    var bi = new BarCodeImageInfo();
                    var barcodeUtil = new BarcodeUtil();
                    if (barcodeUtil.PreCheckBarcodeInfo(label.Barcode))
                    {
                        using (var barcodeStream = barcodeUtil.GetBarcodeStream(label.Barcode, ref bi))
                        {
                            barcodeStream.Position = 0;
                            imagePart.FeedData(barcodeStream);
                            var w = bi.Width; var h = bi.Height; var hr = bi.HR; var vr = bi.VR;
                            cx = (long)w * (long)((float)731520 / hr);
                            cy = (long)h * (long)((float)731520 / vr);
                            if (cy != null && cy > 0)
                            {
                                try { barcodeHeightTwips = (int)((long)cy * 1440 / 914400); } catch { barcodeHeightTwips = 0; }
                            }
                            barcodeDrawing = AssembleImageStatic(label.Barcode, mainPart.GetIdOfPart(imagePart), false, 0, 0, cx, cy);
                        }
                    }
                }
                catch { barcodeHeightTwips = 0; }
            }
            if (barcodeHeightTwips <= 0)
            {
                barcodeHeightTwips = rowHeightTwips;
            }

            int desiredMidHeight = barcodeHeightTwips + BarcodeSidePaddingTwips * 2;
            if (desiredMidHeight > totalHeightTwips - MinSectionHeightTwips * 2)
            {
                desiredMidHeight = Math.Max(totalHeightTwips - MinSectionHeightTwips * 2, MinSectionHeightTwips);
            }
            int remaining = totalHeightTwips - desiredMidHeight;
            if (remaining < MinSectionHeightTwips * 2)
            {
                int forcedTopBottom = MinSectionHeightTwips * 2;
                if (totalHeightTwips >= forcedTopBottom + MinSectionHeightTwips)
                {
                    desiredMidHeight = totalHeightTwips - forcedTopBottom;
                    remaining = forcedTopBottom;
                }
                else
                {
                    remaining = 0;
                    desiredMidHeight = totalHeightTwips;
                }
            }
            int topSectionHeight = remaining / 2;
            int bottomSectionHeight = remaining - topSectionHeight;
            if (topSectionHeight < MinSectionHeightTwips && remaining > 0)
            {
                topSectionHeight = MinSectionHeightTwips;
                bottomSectionHeight = remaining - topSectionHeight;
            }
            if (bottomSectionHeight < MinSectionHeightTwips && remaining > 0)
            {
                bottomSectionHeight = MinSectionHeightTwips;
                topSectionHeight = remaining - bottomSectionHeight;
            }

            var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
            var tableWidth = new DocumentFormat.OpenXml.Wordprocessing.TableWidth { Width = "5000", Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Pct };
            var tableProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                    new DocumentFormat.OpenXml.Wordprocessing.TopBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                    new DocumentFormat.OpenXml.Wordprocessing.LeftBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                    new DocumentFormat.OpenXml.Wordprocessing.BottomBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                    new DocumentFormat.OpenXml.Wordprocessing.RightBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                    new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                    new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None }
                ),
                new DocumentFormat.OpenXml.Wordprocessing.TableLayout { Type = DocumentFormat.OpenXml.Wordprocessing.TableLayoutValues.Fixed },
                tableWidth
            );
            table.Append(tableProps.CloneNode(true));

            var cellWidth = new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth { Width = "5000", Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Pct };

            var rowTop = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            var rowTopProps = new DocumentFormat.OpenXml.Wordprocessing.TableRowProperties();
            rowTopProps.Append(new DocumentFormat.OpenXml.Wordprocessing.TableRowHeight { Val = (DocumentFormat.OpenXml.UInt32Value)(uint)topSectionHeight, HeightType = DocumentFormat.OpenXml.Wordprocessing.HeightRuleValues.Exact });
            rowTop.Append(rowTopProps);
            var cellTop = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            var cellTopProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                new DocumentFormat.OpenXml.Wordprocessing.TableCellVerticalAlignment { Val = DocumentFormat.OpenXml.Wordprocessing.TableVerticalAlignmentValues.Bottom },
                cellWidth.CloneNode(true)
            );
            cellTopProps.Append(new DocumentFormat.OpenXml.Wordprocessing.TableCellMargin(
                new DocumentFormat.OpenXml.Wordprocessing.BottomMargin { Width = BarcodeSidePaddingTwips.ToString(), Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa }
            ));
            cellTop.Append(cellTopProps.CloneNode(true));

            const int SafetyMarginTwips = 40;
            if (label.Logo?.Enabled == true && label.Logo.ImageBytes != null && label.Logo.ImageBytes.Length > 0 && label.Logo.Position == Contract.TemplateManagement.BarcodeTemplatePosition.Above)
            {
                var logoPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                var logoProps = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
                logoProps.Append(new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center });
                logoProps.Append(new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" });
                logoPara.Append(logoProps.CloneNode(true));
                int availableTopHeightTwips = Math.Max(topSectionHeight - BarcodeSidePaddingTwips - SafetyMarginTwips, MinSectionHeightTwips);
                var logoRun = CreateLogoRunStaticLimited(label.Logo, mainPart, availableTopHeightTwips, 0.85);
                if (logoRun != null) logoPara.Append(logoRun);
                cellTop.Append(logoPara);
            }

            if (topProps.Any())
            {
                foreach (var tp in topProps)
                {
                    var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    var props = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
                    props.Append(new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center });
                    props.Append(new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" });
                    p.Append(props.CloneNode(true));
                    var run = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    var runProps = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
                    string fontSize = (tp.FontSize == null || tp.FontSize == 0) ? "16" : tp.FontSize.ToString();
                    runProps.Append(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = fontSize });
                    run.Append(runProps.CloneNode(true));
                    run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text($"{tp.Value}"));
                    p.Append(run.CloneNode(true));
                    cellTop.Append(p.CloneNode(true));
                }
            }
            if (!cellTop.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().Any())
            {
                var placeholderTop = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                    new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                        new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center },
                        new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" })
                );
                placeholderTop.Append(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(" ")));
                cellTop.Append(placeholderTop);
            }

            rowTop.Append(cellTop.CloneNode(true));
            table.Append(rowTop.CloneNode(true));

            var rowMid = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            var rowMidProps = new DocumentFormat.OpenXml.Wordprocessing.TableRowProperties();
            var rowMidHeight = new DocumentFormat.OpenXml.Wordprocessing.TableRowHeight { Val = (DocumentFormat.OpenXml.UInt32Value)(uint)desiredMidHeight, HeightType = DocumentFormat.OpenXml.Wordprocessing.HeightRuleValues.Exact };
            rowMidProps.Append(rowMidHeight);
            rowMid.Append(rowMidProps);
            var cellMid = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            var cellMidProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                new DocumentFormat.OpenXml.Wordprocessing.TableCellVerticalAlignment { Val = DocumentFormat.OpenXml.Wordprocessing.TableVerticalAlignmentValues.Center },
                cellWidth.CloneNode(true)
            );
            cellMid.Append(cellMidProps.CloneNode(true));
            bool hasMidContent = false;
            var midPara = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var midProps = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
            midProps.Append(new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center });
            midProps.Append(new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" });
            midPara.Append(midProps.CloneNode(true));
            if (barcodeDrawing != null)
            {
                var barcodeRun = new DocumentFormat.OpenXml.Wordprocessing.Run(barcodeDrawing.CloneNode(true));
                midPara.Append(barcodeRun.CloneNode(true));
                hasMidContent = true;
            }
            else if (!string.IsNullOrWhiteSpace(label.Barcode))
            {
                var fallback = new DocumentFormat.OpenXml.Wordprocessing.Run();
                fallback.Append(new DocumentFormat.OpenXml.Wordprocessing.Text($"[{label.Barcode}]"));
                midPara.Append(fallback.CloneNode(true));
                hasMidContent = true;
            }
            if (!hasMidContent)
            {
                midPara.Append(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(" ")));
            }
            cellMid.Append(midPara.CloneNode(true));
            rowMid.Append(cellMid.CloneNode(true));
            table.Append(rowMid.CloneNode(true));

            var rowBottom = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            var rowBottomProps = new DocumentFormat.OpenXml.Wordprocessing.TableRowProperties();
            rowBottomProps.Append(new DocumentFormat.OpenXml.Wordprocessing.TableRowHeight { Val = (DocumentFormat.OpenXml.UInt32Value)(uint)bottomSectionHeight, HeightType = DocumentFormat.OpenXml.Wordprocessing.HeightRuleValues.Exact });
            rowBottom.Append(rowBottomProps);
            var cellBottom = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            var cellBottomProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                new DocumentFormat.OpenXml.Wordprocessing.TableCellVerticalAlignment { Val = DocumentFormat.OpenXml.Wordprocessing.TableVerticalAlignmentValues.Top },
                cellWidth.CloneNode(true)
            );
            cellBottomProps.Append(new DocumentFormat.OpenXml.Wordprocessing.TableCellMargin(
                new DocumentFormat.OpenXml.Wordprocessing.TopMargin { Width = BarcodeSidePaddingTwips.ToString(), Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa }
            ));
            cellBottom.Append(cellBottomProps.CloneNode(true));

            if (label.Logo?.Enabled == true && label.Logo.ImageBytes != null && label.Logo.ImageBytes.Length > 0 && label.Logo.Position == Contract.TemplateManagement.BarcodeTemplatePosition.Under)
            {
                int availableBottomHeightTwips = Math.Max(bottomSectionHeight - BarcodeSidePaddingTwips - SafetyMarginTwips, MinSectionHeightTwips);
                var logoContainer = EnsureLogoFits(label.Logo, mainPart, availableBottomHeightTwips, 0.75);
                if (logoContainer != null) cellBottom.Append(logoContainer);
            }


            if (bottomProps.Any())
            {
                foreach (var bp in bottomProps)
                {
                    var pBottom = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    var pBottomPr = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
                    pBottomPr.Append(new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center });
                    pBottomPr.Append(new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" });
                    pBottom.Append(pBottomPr.CloneNode(true));
                    var run = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    var runProps = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
                    string fontSize = (bp.FontSize == null || bp.FontSize == 0) ? "16" : bp.FontSize.ToString();
                    runProps.Append(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = fontSize });
                    run.Append(runProps.CloneNode(true));
                    run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text($"{bp.Value}"));
                    pBottom.Append(run.CloneNode(true));
                    cellBottom.Append(pBottom.CloneNode(true));
                }
            }
            if (!cellBottom.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().Any())
            {
                var placeholderBottom = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                    new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                        new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center },
                        new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" })
                );
                placeholderBottom.Append(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(" ")));
                cellBottom.Append(placeholderBottom);
            }
            rowBottom.Append(cellBottom.CloneNode(true));
            table.Append(rowBottom.CloneNode(true));

            content.Append(table.CloneNode(true));
        }

    private static DocumentFormat.OpenXml.Wordprocessing.Run CreateLogoRunStaticLimited(LogoItem logo, MainDocumentPart mainPart, int availableHeightTwips, double scaleFactor = 1.0)
        {
            try
            {
                if (mainPart == null || logo?.ImageBytes == null || logo.ImageBytes.Length == 0) return null;
                var imagePart = mainPart.AddImagePart(ImagePartType.Png);
                using (var ms = new MemoryStream(logo.ImageBytes)) imagePart.FeedData(ms);
                var relId = mainPart.GetIdOfPart(imagePart);
                int origW = logo.Width > 0 ? logo.Width : 100;
                int origH = logo.Height > 0 ? logo.Height : 100;

                long maxHeightEmu = (long)availableHeightTwips * 635L;
                long widthEmu = origW * 9525L;
                long heightEmu = origH * 9525L;
                if (heightEmu > maxHeightEmu && heightEmu > 0)
                {
                    double scale = (double)maxHeightEmu / heightEmu;
                    heightEmu = maxHeightEmu;
                    widthEmu = (long)(widthEmu * scale);
                }
                if (scaleFactor > 0 && scaleFactor < 1.0)
                {
                    widthEmu = (long)(widthEmu * scaleFactor);
                    heightEmu = (long)(heightEmu * scaleFactor);
                }

                var inline = new DW.Inline(
                    new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = (UInt32Value)NextDrawingId(), Name = logo.FileName ?? "logo" },
                    new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties { Id = (UInt32Value)NextDrawingId(), Name = logo.FileName ?? "logo" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip { Embed = relId },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset { X = 0L, Y = 0L },
                                        new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                );
                inline.DistanceFromTop = 0U;
                inline.DistanceFromBottom = 0U;
                inline.DistanceFromLeft = 0U;
                inline.DistanceFromRight = 0U;
                return new DocumentFormat.OpenXml.Wordprocessing.Run(new Drawing(inline));
            }
            catch { return null; }
        }

        private static DocumentFormat.OpenXml.Wordprocessing.Table EnsureLogoFits(LogoItem logo, MainDocumentPart mainPart, int availableHeightTwips, double scaleFactor = 1.0)
        {
            try
            {
                var run = CreateLogoRunStaticLimited(logo, mainPart, availableHeightTwips, scaleFactor);
                if (run == null) return null;
                var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                var tblProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                        new DocumentFormat.OpenXml.Wordprocessing.TopBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                        new DocumentFormat.OpenXml.Wordprocessing.LeftBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                        new DocumentFormat.OpenXml.Wordprocessing.BottomBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                        new DocumentFormat.OpenXml.Wordprocessing.RightBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.None }
                    ),
                    new DocumentFormat.OpenXml.Wordprocessing.TableLayout { Type = DocumentFormat.OpenXml.Wordprocessing.TableLayoutValues.Fixed },
                    new DocumentFormat.OpenXml.Wordprocessing.TableWidth { Width = "5000", Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Pct }
                );
                table.Append(tblProps);
                var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                var rowPr = new DocumentFormat.OpenXml.Wordprocessing.TableRowProperties();
                rowPr.Append(new DocumentFormat.OpenXml.Wordprocessing.TableRowHeight { Val = (DocumentFormat.OpenXml.UInt32Value)(uint)availableHeightTwips, HeightType = DocumentFormat.OpenXml.Wordprocessing.HeightRuleValues.Exact });
                row.Append(rowPr);
                var cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
                var cellPr = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.TableCellVerticalAlignment { Val = DocumentFormat.OpenXml.Wordprocessing.TableVerticalAlignmentValues.Center },
                    new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth { Width = "5000", Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Pct }
                );
                cell.Append(cellPr);
                var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                    new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                        new DocumentFormat.OpenXml.Wordprocessing.Justification { Val = DocumentFormat.OpenXml.Wordprocessing.JustificationValues.Center },
                        new DocumentFormat.OpenXml.Wordprocessing.SpacingBetweenLines { Before = "0", After = "0" })
                );
                p.Append(run);
                cell.Append(p);
                row.Append(cell);
                table.Append(row);
                return table;
            }
            catch { return null; }
        }

        private static Drawing AssembleImageStatic(string Name, string relationshipId, bool isTemplatePic, int imageWidth, int imageHegiht, Int64Value cx, Int64Value cy)
        {
            long widthEmu = isTemplatePic ? (imageWidth > 0 ? imageWidth * 9525L : 1600000L) : (cx > 0 ? (long)cx : 2000000L);
            long heightEmu = isTemplatePic ? (imageHegiht > 0 ? imageHegiht * 9525L : 1300000L) : (cy > 0 ? (long)cy : 800000L);
            
            var inline = new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                new DW.DocProperties { Id = (UInt32Value)NextDrawingId(), Name = Name },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = (UInt32Value)NextDrawingId(), Name = Name },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationshipId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                    ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                )
            );
            // Set distances as properties
            inline.DistanceFromTop = 0U;
            inline.DistanceFromBottom = 0U;
            inline.DistanceFromLeft = 0U;
            inline.DistanceFromRight = 0U;
            return new Drawing(inline);
        }

        private static void DuplicateFirstPageContent(Body body, int totalPages, BarcodeTemplateLabelType labelType = BarcodeTemplateLabelType.Label_95x65)
        {
            var firstPageElements = body.Elements().ToList();
            if (firstPageElements.Count == 0) return;
            for (int page = 2; page <= totalPages; page++)
            {
                foreach (var el in firstPageElements)
                {
                    body.AppendChild(el.CloneNode(true));
                }
            }
        }

        private void RemoveEmptyParagraphs()
        {
            try
            {
                var body = wordprocessingDocument.MainDocumentPart.Document.Body;
                foreach (var p in body.Descendants<Paragraph>())
                {
                    var text = p.InnerText?.Trim();
                    bool isEmptyParagraph = string.IsNullOrWhiteSpace(text) && !p.HasChildren &&
                    p.Descendants<Run>().All(r => string.IsNullOrWhiteSpace(r.InnerText));
                    if (isEmptyParagraph) p.Remove();
                }
                wordprocessingDocument.MainDocumentPart.Document.Save();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while removing empty paragraphs from the document. Ex: {ex}");
            }
        }
    }
}
