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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace LS.SPWorkflowProcessor
{
    public class NintexWFCompressor
    {
        public static Stream CompressNintexWFData(byte[] manIfestData, byte[] actions, byte[] metadata, byte[] settings, byte[] lists, byte[] variableContent, List<byte[]> formFilesBytes)
        {
            var stream = new MemoryStream();
            using (ZipOutputStream zipStream = new ZipOutputStream(stream))
            {
                zipStream.SetLevel(9);

                CompressData(stream, zipStream, "Manifest.xml", manIfestData);
                if (lists != null)
                {
                    CompressData(stream, zipStream, "Lists.xml", lists);
                }
                if (variableContent != null)
                {
                    CompressData(stream, zipStream, @"Workflow\Variables.xml", variableContent);
                }
                CompressData(stream, zipStream, @"Workflow\Actions.xml", actions);
                CompressData(stream, zipStream, @"Workflow\Metadata.xml", metadata);
                CompressData(stream, zipStream, @"Workflow\Settings.xml", settings);
                CompressNintexForm(formFilesBytes, stream, zipStream);

            }
            return new MemoryStream(stream.ToArray());
        }

        private static void CompressNintexForm(List<byte[]> formFilesBytes, Stream stream, ZipOutputStream zipStream)
        {
            if (formFilesBytes.Count > 0)
            {
                for (int i = 1; i <= formFilesBytes.Count; i++)
                {
                    CompressData(stream, zipStream, string.Format(@"Forms\Form{0}.xml", i.ToString()), formFilesBytes[i - 1]);
                }
            }
        }

        private static void CompressData(Stream stream, ZipOutputStream zipStream, string dataName, byte[] data)
        {
            ZipEntry entry = new ZipEntry(Path.GetFileName(dataName));
            entry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry);
            zipStream.Write(data, 0, data.Length);
        }

    }
}
