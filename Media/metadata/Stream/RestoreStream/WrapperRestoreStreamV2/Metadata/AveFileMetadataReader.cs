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

namespace AvePoint.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.RA.CommonUtil;


/// <summary>
/// DONOT DELETE, talk to qlluo@avepoint.com or wbhu@avepoint.com
/// </summary>
public class AveFileMetadataReader : IAveMetadataReader
{
    private static RALogger logger = RALogger.GetInstance(typeof(AveFileMetadataReader));
    protected bool IsAllMetadataLoaded = false;

    protected List<AveMetadataV2> MetadataList { get; set; }

    protected List<AveMetadataV2> UsedMetadataList { get; set; }

    private AveXmlTextReader internalReader { get; set; }

    public AveFileMetadataReader(Stream stream, string cacheDirectory)
    {
        var streamReader = new StreamReader(stream, Encoding.UTF8);
        XmlReaderSettings setting = new XmlReaderSettings();
        setting.IgnoreComments = true;
        setting.IgnoreWhitespace = true;
        setting.CloseInput = false;
        var reader = XmlReader.Create(streamReader, setting);
        internalReader = new AveXmlTextReader(reader, cacheDirectory);
    }

    public AveFileMetadataReader(StreamReader streamReader, string cacheDirectory)
    {
        XmlReaderSettings setting = new XmlReaderSettings();
        setting.IgnoreComments = true;
        setting.IgnoreWhitespace = true;
        setting.CloseInput = false;
        var reader = XmlReader.Create(streamReader, setting);
        internalReader = new AveXmlTextReader(reader, cacheDirectory);
    }

    private bool ReadNextMetadataElement(out AveMetadataType aveMetadataType)
    {
        aveMetadataType = AveMetadataType.Unknown;
        bool isRead = false;
        while (true)
        {
            if (internalReader.ReadState == ReadState.Initial)
            {
                //Read data Node
                internalReader.Read();
                //Read first child element metadata
                internalReader.Read();
            }
            if (internalReader.NodeType == XmlNodeType.Element)
            {
                string name = internalReader.GetAttribute(AveMetadataConstants.COLUMN_NAME);
                if (!string.IsNullOrEmpty(name) && Enum.IsDefined(typeof(AveMetadataType), name))
                {
                    aveMetadataType = (AveMetadataType)Enum.Parse(typeof(AveMetadataType), name);
                    logger.Info("Reader metadata by type :{0}", name);
                    isRead = true;
                    break;
                }
                if (!ShouldContinue())
                {
                    break;
                }
                //this code should not go into
                //read next
                internalReader.Read();

                logger.Warn("[NodeName:{0}][AttributeName:{1}][NodeType:{2}][AttributeCount:{3}]",
                    internalReader.Name, name, internalReader.NodeType, internalReader.AttributeCount);
            }
            else
            {
                if (!ShouldContinue())
                {
                    break;
                }
                //read next
                internalReader.Read();
            }
        }
        logger.Info("IsMetadataReaded:{0}", isRead);
        return isRead;
    }

    private bool ShouldContinue()
    {
        bool shouldContinue = true;
        if (internalReader.ReadState == ReadState.EndOfFile || internalReader.ReadState == ReadState.Closed)
        {
            logger.Info("Read end of xml reader V2.");
            //read end of reader
            shouldContinue = false;
        }
        if (internalReader.ReadState == ReadState.Error)
        {
            throw new XmlException("Internal reader state is Error.");
        }
        return shouldContinue;
    }

    protected void EnsureReadAllMetadata()
    {
        if (!IsAllMetadataLoaded)
        {
            DateTime start = DateTime.Now;
            logger.Info("Begin to load all metadata");
            StringBuilder elementBuilder = new StringBuilder();
            elementBuilder.AppendLine("MetadataList");
            if (MetadataList != null)
            {
                MetadataList.Clear();
            }
            else
            {
                MetadataList = new List<AveMetadataV2>();
            }
            if (UsedMetadataList != null)
            {
                UsedMetadataList.Clear();
            }
            else
            {
                UsedMetadataList = new List<AveMetadataV2>();
            }
            AveMetadataType metadataType;
            while (ReadNextMetadataElement(out metadataType))
            {
                string metadataPath = ReadCurrentMetadataPath(metadataType);
                var metadata = new AveMetadataV2(metadataPath, metadataType);
                elementBuilder.AppendLine(string.Format("[{0}][{1}]", metadataType, metadataPath));
                MetadataList.Add(metadata);
            }
            IsAllMetadataLoaded = true;
            logger.Info("Load all metadata complete.TimeCost:[{0}],Details:{1}", DateTime.Now - start, elementBuilder);
        }
        else
        {
            logger.Info("Metadata already loaded previously.");
        }
    }

    private string ReadCurrentMetadataPath(AveMetadataType metadataType)
    {
        return internalReader.ReadXmlToFile(metadataType);
    }

    public AveMetadata ReadMetadata()
    {
        logger.Info("Begin to read metadata");
        EnsureReadAllMetadata();
        if (MetadataList == null || MetadataList.Count == 0)
        {
            return null;
        }
        var metadata = MetadataList.FirstOrDefault();
        if (metadata != null)
        {
            MetadataList.RemoveAt(0);
            UsedMetadataList.Add(metadata);
            return metadata;
        }
        return null;
    }

    public AveMetadata TryReadMetadata(AveMetadataType type)
    {
        EnsureReadAllMetadata();
        if (MetadataList == null || MetadataList.Count == 0)
        {
            return null;
        }
        var metadata = MetadataList.Where(t => t.MetadataType == type).FirstOrDefault();
        logger.Info("TryRead Metadata from MetadataElementList with type {0} successfully.", type);
        return metadata;
    }

    public List<AveMetadata> TryReadMetadataList(AveMetadataType type)
    {
        EnsureReadAllMetadata();
        if (MetadataList == null || MetadataList.Count == 0)
        {
            return null;
        }
        var metadataList = MetadataList.Where(t => t.MetadataType == type).ToList<AveMetadata>();
        logger.Warn("TryReadMetadataList successfully,Type:{0},Count:{1}.", type, metadataList.Count);
        return metadataList;
    }

    public void Dispose()
    {
        EnsureReadAllMetadata();
        internalReader = null;
        if (MetadataList != null)
        {
            MetadataList.ForEach(t => t.Dispose());
            MetadataList.Clear();
            MetadataList = null;
        }
        if (UsedMetadataList != null)
        {
            UsedMetadataList.ForEach(t => t.Dispose());
            UsedMetadataList.Clear();
            UsedMetadataList = null;
        }
    }
}