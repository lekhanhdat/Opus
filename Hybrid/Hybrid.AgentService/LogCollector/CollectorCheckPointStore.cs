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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.LogCollector
{
    public interface ICollectorCheckPointStore
    {
        DateTimeOffset? GetLastCheckPoint(string collectorName);
        void UpdateCheckPoint(string collectorName, DateTimeOffset? lastCheckPoint);
    }

    public class CollectorCheckpointModel
    {
        [JsonPropertyName("collector")]
        public string Collector { get; set; }
        [JsonPropertyName("lastCheckpoint")]
        public DateTimeOffset LastCheckpoint { get; set; }
    }


    public class CollectorCheckPoint : ICollectorCheckPointStore
    {
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string storeDirectory;
        private readonly object syncRoot = new object();

        public CollectorCheckPoint(string storeDirectory)
        {
            this.storeDirectory = storeDirectory;
            try
            {
                Directory.CreateDirectory(this.storeDirectory);
            }
            catch(Exception ex) 
            {
                this.logger.Error("Failed to create checkpoint directory '{0}'. Details: {1}.", this.storeDirectory, ex.ToString());
                throw;
            }
        }

        public DateTimeOffset? GetLastCheckPoint(string collectorName)
        {
            var filePath = Path.Combine(this.storeDirectory, $"{collectorName}.json");
            if (!File.Exists(filePath))
            { 
                return null;
            }
            try
            {
                string raw;

                lock (this.syncRoot)
                {
                    raw = File.ReadAllText(filePath);
                }

                var model = JsonSerializer.Deserialize<CollectorCheckpointModel>(raw);

                
                if (model?.LastCheckpoint != null)
                {
                    return model.LastCheckpoint;
                }
                logger.Warn("Checkpoint file '{0}' contains invalid JSON or missing lastCheckpoint. Content: {1}.",
                    filePath, raw);

            }
            catch (Exception ex)
            {
                logger.Warn("Failed to read checkpoint file '{0}'. Details: {1}.", filePath, ex.ToString());
            }
            return null;
        }

        public void UpdateCheckPoint(string collectorName, DateTimeOffset? lastCheckPoint)
        {
            var filePath = this.GetCollectorFilePath($"{collectorName}.json");

            var payload = JsonSerializer.Serialize(new
            {
                collector = collectorName,
                lastCheckpoint = lastCheckPoint
            });

            try
            {
                lock (this.syncRoot)
                {
                    File.WriteAllText(filePath, payload);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Failed to persist checkpoint for collector '{0}'. Details: {1}.", collectorName, ex.ToString());
                throw;
            }
        }

        public string GetCollectorFilePath(string collectorName)
        {
            return Path.Combine(this.storeDirectory, collectorName);
        }
    }
}
