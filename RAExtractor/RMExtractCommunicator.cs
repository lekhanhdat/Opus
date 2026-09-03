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
using AvePoint.RA.Common.RAProcess;
using AvePoint.RA.Common.RAProcess.Extractor;
using AvePoint.RA.Common.RAProcess.Locker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAExtractor
{
    public class RMExtractCommunicator
    {
        private readonly Func<RMPipeExtractorRequestData, Task<RMPipeExtractorReponseData>> _callback;

        private readonly RMProcessMessageQueue _producerMessageQueue;

        private readonly RMProcessMessageQueue _consumerMessageQueue;

        private readonly Task _receiverTask;

        public RMExtractCommunicator(Func<RMPipeExtractorRequestData, Task<RMPipeExtractorReponseData>> callback)
        {
            _callback = callback;
            _producerMessageQueue = new(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Full_Text_Index", RMPipeExtractorDefinition.EXTRACT_PRODUCER_MESSAGE_CONTAINER_PATH), new RMProcessMutexLocker("GlobalFullTextIndexProducer"));
            _consumerMessageQueue = new(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Full_Text_Index", RMPipeExtractorDefinition.EXTRACT_CONSUMER_MESSAGE_CONTAINER_PATH), new RMProcessMutexLocker("GlobalFullTextIndexConsumer"));
            _receiverTask = Task.Run(() => StartReceiver());
        }

        private void StartReceiver()
        {
            while (true)
            {
                var message = _producerMessageQueue.Dequeue();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    ClientOnConnect(message);
                }
                Task.Delay(1000).Wait();
            }
        }

        private void ClientOnConnect(string message)
        {
            RMPipeExtractorRequestData? requestData = null;
            try
            {
                requestData = JsonConvert.DeserializeObject<RMPipeExtractorRequestData>(message);
                var responseData = _callback(requestData).GetAwaiter().GetResult();
                responseData.IndexDBUniqueId = requestData.IndexDBUniqueId;

                _consumerMessageQueue.Enqueue(JsonConvert.SerializeObject(responseData));
            }
            catch (Exception e)
            {
                if (requestData != null)
                {
                    var responseData = new RMPipeExtractorReponseData
                    {
                        IndexDBUniqueId = requestData.IndexDBUniqueId,
                        Succeed = false,
                        ErrorMessage = e.ToString()
                    };

                    _consumerMessageQueue.Enqueue(JsonConvert.SerializeObject(responseData));
                }
            }
        }

        public void Dispose()
        {
            _receiverTask.Dispose();
        }
    }
}
