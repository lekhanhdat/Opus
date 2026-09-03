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
using System.Collections;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace HSMCommon
{
    public interface ISPMMetadataGenerator
    {
        T GetData<T>(string type);
        List<T> GetDataCollection<T>(string type);
    }

    public class SPMMetadataGenerator : ISPMMetadataGenerator
    {
        public Dictionary<string, object> Data { get; set; }
        public string Type { get; set; }

        public SPMMetadataGenerator(Dictionary<string, object> data)
        {
            Data = data;
        }

        public List<T> GetDataCollection<T>(string type)
        {
            List<T> dataCollection;
            if (Data.ContainsKey(type))
            {
                dataCollection = Activator.CreateInstance<List<T>>();
                foreach (AveMetadata metadata in (Data[type] as List<AveMetadata>))
                {
                    dataCollection.Add(metadata.GetMetadata<T>());
                }
            }
            else
            {
                dataCollection = default(List<T>);
            }
            return dataCollection;
        }

        public T GetData<T>(string type)
        {
            T data;           
            if (Data.ContainsKey(type))
            {
                AveMetadata metadata = Data[type] as AveMetadata;
                switch (metadata.MetadataType)
                {
                    case AveMetadataType.DocProperty:
                    case AveMetadataType.DocData:
                        data = Activator.CreateInstance<T>();
                        metadata.GetMetadata((IDictionary)data);
                        break;
                    default:
                        return metadata.GetMetadata<T>();
                }
            }
            else
            {
                data = default(T);
            }
            return data;
        }
    }
}
