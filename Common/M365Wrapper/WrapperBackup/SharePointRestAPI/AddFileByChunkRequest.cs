/********************************************************************
 *
 * PROPRIETARY and CONFIDENTIAL
 *
 * This file is licensed from, and is a trade secret of:
 *
 * AvePoint, Inc.
 * 525 Washington Blvd, Suite 1400
 * Jersey City, NJ 07310
 * United States of America
 * Telephone: +1-201-793-1111
 * WWW: www.avepoint.com
 *
 * Refer to your License Agreement for restrictions on use,
 * duplication, or disclosure.
 *
 * RESTRICTED RIGHTS LEGEND
 *
 * Use, duplication, or disclosure by the Government is
 * subject to restrictions as set forth in subdivision
 * (c)(1)(ii) of the Rights in Technical Data and Computer
 * Software clause at DFARS 252.227-7013 (Oct. 1988) and
 * FAR 52.227-19 (C) (June 1987).
 *
 * Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 * Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace ExchangeUtility.Graph.SharePointRestAPI
{
    using AvePoint.RA.CommonUtil;
    using Microsoft365.Authentication;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    class AddFileByChunkRequest(string siteUrl, ITokenProvider tokenProvider) : SharePointRestBase<EmptyObject>(siteUrl, tokenProvider)
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AddFileByChunkRequest));

        public string FolderServerRelativeUrl { get; set; }
        public string FileName { get; set; }
        public string FileServerRelativeUrl { get; set; }
        public bool OverWrite { get; set; } = false;
        public Stream Content { get; set; }
        public int ChunkSize { get; set; } = 5 * 1024 * 1024;
        public int MaxRetries { get; set; } = 10;
        public int InitialDelayMs { get; set; } = 2000;

        private ChunkUploadAction _currentAction;
        private Guid _uploadId;
        private long _currentOffset;
        private Stream _currentChunkStream;

        public override Stream PostRequestStream => _currentChunkStream;

        public override string RequestMethod => METHOD_POST;

        public override string RequestUrl
        {
            get
            {
                string escapedUrl = this.FileServerRelativeUrl?.Replace("'", "''") ?? string.Empty;

                return _currentAction switch
                {
                    ChunkUploadAction.Start => $"{this.restBaseUrl}/web/GetFileByServerRelativeUrl('{escapedUrl}')/StartUpload(uploadId=guid'{_uploadId}')",
                    ChunkUploadAction.Continue => $"{this.restBaseUrl}/web/GetFileByServerRelativeUrl('{escapedUrl}')/ContinueUpload(uploadId=guid'{_uploadId}',fileOffset={_currentOffset})",
                    ChunkUploadAction.Finish => $"{this.restBaseUrl}/web/GetFileByServerRelativeUrl('{escapedUrl}')/FinishUpload(uploadId=guid'{_uploadId}',fileOffset={_currentOffset})",
                    _ => throw new NotSupportedException()
                };
            }
        }

        /// <summary>
        /// Executes the chunk uploading sequence.
        /// Throws any exceptions immediately to be handled by the outer orchestrator.
        /// </summary>
        public new void Execute()
        {
            this.ValidateArguments();

            int attempt = 0;

            while (true)
            {
                try
                {
                    attempt++;
                    logger.Info("Starting chunk upload execution attempt {0}/{1}.", attempt, this.MaxRetries);

                    if (attempt > 1)
                    {
                        if (!this.Content.CanSeek)
                        {
                            throw new InvalidOperationException("Stream is not seekable. Cannot perform execution.");
                        }
                        logger.Info("Resetting stream pointer to beginning for retry attempt {0}.", attempt);
                        this.Content.Seek(0L, SeekOrigin.Begin);
                    }

                    CreatePlaceholderFile();
                    UploadChunks();
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("Execution attempt {0}/{1} failed. Error: {2}", attempt, this.MaxRetries, ex.Message);
                    if (attempt >= this.MaxRetries || !this.Content.CanSeek)
                    {
                        logger.Error("Maximum retry threshold reached or stream non-seekable. Escalating exception to caller.");
                        throw;
                    }
                    PerformBackoffDelay(attempt);
                }
            }
        }

        private void UploadChunks()
        {
            _currentOffset = 0;
            _uploadId = Guid.NewGuid();

            long totalBytes = this.Content.CanSeek ? this.Content.Length : 0;
            long totalChunks = totalBytes > 0 ? (long)Math.Ceiling((double)totalBytes / ChunkSize) : 0;
            long chunkIndex = 0;
            bool firstChunk = true;

            byte[] currentBuffer = new byte[ChunkSize];
            byte[] nextBuffer = new byte[ChunkSize];

            int currentRead = this.Content.Read(currentBuffer, 0, ChunkSize);

            while (currentRead > 0)
            {
                int nextRead = this.Content.Read(nextBuffer, 0, ChunkSize);
                bool lastChunk = (nextRead == 0);

                chunkIndex++;

                logger.Info("Chunk Progress [{0}/{1}] ChunkSize:{2}", chunkIndex, totalChunks > 0 ? totalChunks.ToString() : "?", currentRead);

                if (firstChunk)
                {
                    _currentAction = lastChunk ? ChunkUploadAction.Finish : ChunkUploadAction.Start;
                    firstChunk = false;
                }
                else
                {
                    _currentAction = lastChunk ? ChunkUploadAction.Finish : ChunkUploadAction.Continue;
                }

                using (_currentChunkStream = new MemoryStream(currentBuffer, 0, currentRead, false))
                {
                    base.Execute();
                    _currentOffset += currentRead;
                }

                if (lastChunk)
                {
                    break;
                }

                currentRead = nextRead;
                Array.Copy(nextBuffer, currentBuffer, nextRead);
            }
        }

        private void PerformBackoffDelay(int attempt)
        {
            int backoffDelay = this.InitialDelayMs * attempt;
            logger.Info("Sleeping for {0}ms before next retry.", backoffDelay);
            Thread.Sleep(backoffDelay);
        }

        private void CreatePlaceholderFile()
        {
            using var emptyStream = new MemoryStream();
            var addPlaceholderRequest = new AddFileRequest(this.SiteUrl, this.TokenProvider)
            {
                FolderServerRelativeUrl = this.FolderServerRelativeUrl,
                FileName = this.FileName,
                OverWrite = this.OverWrite,
                Content = emptyStream
            };
            addPlaceholderRequest.Execute();
        }

        protected override void ValidateArguments()
        {
            if (string.IsNullOrEmpty(this.FolderServerRelativeUrl))
                throw new ArgumentNullException(nameof(this.FolderServerRelativeUrl));
            if (string.IsNullOrEmpty(this.FileName))
                throw new ArgumentNullException(nameof(this.FileName));
            if (string.IsNullOrEmpty(this.FileServerRelativeUrl))
                throw new ArgumentNullException(nameof(this.FileServerRelativeUrl));
            if (this.Content == null)
                throw new ArgumentNullException(nameof(this.Content));
            if (!this.Content.CanRead)
                throw new InvalidOperationException("Content is not readable.");
        }
    }

    public enum ChunkUploadAction
    {
        Start,
        Continue,
        Finish
    }
}
