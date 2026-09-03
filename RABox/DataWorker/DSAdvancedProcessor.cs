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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;

namespace RABox.DataWorker
{
    public class DSAdvancedProcessor
    {
        private RALogger logger = RALogger.GetInstance(typeof(DSAdvancedProcessor));

        public DataProducer Producer { get; set; }

        public DataConsumer Consumer { get; set; }

        public ReportCenter ReportCenter { get; set; }

        private CancellationTokenSource cancellationTokenSource;

        private DSAdvancedProcessor()
        {
            Producer = new DataProducer();
            Consumer = new DataConsumer();
            ReportCenter = new ReportCenter();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public static DSAdvancedProcessor Build()
        {
            return new DSAdvancedProcessor();
        }

        public async Task ProcessAsync(JobType jobType, string subJobId, string scopeId)
        {
            logger.Info("Start data sync job processor");

            try
            {
                //Job Initilize
                var jobMessage = PlatformWindsorManager.GetService<IRMSubJobDao>().GetSubJob(subJobId);

                Producer.Build();
                ReportCenter.Build(jobType, subJobId, NodeFlagType.BoxSync);
                //Consumer.InitProvider();
                Producer.Config(cancellationTokenSource.Token).Config(ReportCenter);
                Consumer.Config(cancellationTokenSource.Token).Config(ReportCenter);

                //Validate connection and tree node
                if (await Producer.Validate() != true || await Consumer.Validate() != true)
                {
                    return;
                }

                //ReportCenter.SetProgressInfo(Producer.GetProgressInfo());

                do
                {

                    var producerQueue = Producer.CreateDataQueue(10, 10);
                    var consumerQueue = Consumer.CreateDataQueue(10, 10);

                    Producer.Process(consumerQueue);
                    Consumer.Process(producerQueue);

                    Producer.Completed();
                    Consumer.Completed();

                }
                while (Producer.ShouldRetry());

                //ReportCenter.Finilize

            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while executing sync job. {e}");
                ReportCenter.SetJobFinish(JobStatus.Failed, e.Message);
            }
            finally
            {
                //Producer?.Dispose();
                //Consumer?.Dispose();
                ReportCenter?.Completed();
                //ReportCenter?.Dispose();
            }

            logger.Info("Data sync job completed");
        }
    }
}
