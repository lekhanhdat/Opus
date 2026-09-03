using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public static class RMFileSystemConcurrencyExtension
    {
        public static async IAsyncEnumerable<TResult> ParallelSelectAsync<TSource, TResult>(
                this IAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, Task<TResult>> body,
                int maxDegreeOfParallelism,
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var transformBlock = new TransformBlock<TSource, TResult>(
                async item => await body(item, cancellationToken).ConfigureAwait(false),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism * 2,
                    CancellationToken = cancellationToken
                });

            var producerTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                    {
                        await transformBlock.SendAsync(item, cancellationToken).ConfigureAwait(false);
                    }
                    transformBlock.Complete();
                }
                catch (Exception ex)
                {
                    ((IDataflowBlock)transformBlock).Fault(ex);
                }
            }, cancellationToken);

            while (await transformBlock.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
                while (transformBlock.TryReceive(out var result))
                {
                    yield return result;
                }
            }

            await producerTask.ConfigureAwait(false);
            await transformBlock.Completion.ConfigureAwait(false);
        }

        public static async Task ParallelForEachAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, Task> body,
            int maxDegreeOfParallelism,
            CancellationToken cancellationToken = default)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (maxDegreeOfParallelism <= 0) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));

            var actionBlock = new ActionBlock<TSource>(
                async item =>
                {
                    await body(item, cancellationToken).ConfigureAwait(false);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism * 2,
                    CancellationToken = cancellationToken
                });

            try
            {
                await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    await actionBlock.SendAsync(item, cancellationToken).ConfigureAwait(false);
                }

                actionBlock.Complete();
            }
            catch (Exception ex)
            {
                ((IDataflowBlock)actionBlock).Fault(ex);
                throw;
            }

            await actionBlock.Completion.ConfigureAwait(false);
        }
    }
}

