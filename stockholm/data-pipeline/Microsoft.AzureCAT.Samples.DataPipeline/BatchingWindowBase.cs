
namespace Microsoft.AzureCAT.Samples.DataPipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using System.Linq;

    public class BatchingWindowBase<TInput, TOutput> : IDisposable
    {
        private BufferBlock<TInput> _buffer;
        private BatchBlock<TInput> _batcher;

        private TransformBlock<IEnumerable<TInput>, IEnumerable<TOutput>> _aggregator;
        private ActionBlock<IEnumerable<TOutput>> _publisher;

        private readonly CancellationTokenSource _tokenSource;
        private System.Threading.Timer _windowTimer;
        private IDisposable[] _disposables;

        private readonly Func<TInput, bool> _filterFunc;
        private readonly Func<TInput, string> _nameFunc;
        private readonly Func<IEnumerable<TInput>, IEnumerable<TOutput>> _transformFunc;
        private readonly Func<IEnumerable<TOutput>, Task> _publishFunc;

        private long _droppedEvents;

        private readonly ILogger _logger;

        public BatchingWindowBase(BatchingWindowConfiguration config,
          ILogger logger)
        {
            this._logger = logger;
            this._tokenSource = new CancellationTokenSource();

            // Set up the message transforms         
            this._publishFunc = Publish;
            this._transformFunc = Transform;

            // Set up the publisher - publish back into the pipeline
            InitializeFlow(config);
        }

        public BatchingWindowBase(BatchingWindowConfiguration config,         
            Func<IEnumerable<TInput>, IEnumerable<TOutput>> transformFunc,
            Func<IEnumerable<TOutput>, Task> publishFunc,
            ILogger logger)
        {
            this._logger = logger;
            this._tokenSource = new CancellationTokenSource();

            // Set up the message transforms         
            this._publishFunc = publishFunc;
            this._transformFunc = transformFunc;         

            // Set up the publisher - publish back into the pipeline
            InitializeFlow(config);
        }

        protected virtual IEnumerable<TOutput> Transform(IEnumerable<TInput> evts)
        {
            return Enumerable.Empty<TOutput>();
        }

        protected virtual Task Publish(IEnumerable<TOutput> evts)
        {
            return Task.FromResult(0);
        }

        protected void InitializeFlow(BatchingWindowConfiguration config)
        {
            // TODO - use the extension method
            var bufferOptions = new ExecutionDataflowBlockOptions()
            {
                BoundedCapacity = config.MaxBacklogSize,
                CancellationToken = _tokenSource.Token
            };
            _buffer = new BufferBlock<TInput>(bufferOptions);

            _batcher = new BatchBlock<TInput>(config.MaxWindowEventCount,
                new GroupingDataflowBlockOptions()
                {
                    BoundedCapacity = config.MaxWindowEventCount,
                    Greedy = true,
                    CancellationToken = _tokenSource.Token
                });

            _aggregator = new TransformBlock<IEnumerable<TInput>, IEnumerable<TOutput>>(
                transform: (e) => _transformFunc(e),
                dataflowBlockOptions: new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = _tokenSource.Token
                });

           

            _publisher = new ActionBlock<IEnumerable<TOutput>>(
                async (e) => await _publishFunc(e),
                new ExecutionDataflowBlockOptions()
                {
                    // Maximum number of concurrent operations being "actioned"
                    MaxDegreeOfParallelism = config.PublishDegreeOfParallelism,

                    // Maximum number of pending items to publish
                    BoundedCapacity = config.MaxPublishBacklogSize,
                    CancellationToken = _tokenSource.Token
                });

            var disp = new List<IDisposable>();
            disp.Add(_buffer.LinkTo(_batcher));
            disp.Add(_batcher.LinkTo(_aggregator));
            disp.Add(_aggregator.LinkTo(_publisher));
            _disposables = disp.ToArray();

            this._windowTimer = new Timer(FlushBuffer, null,
                config.SlidingWindowSize, config.SlidingWindowSize);
        }

      

        private void FlushBuffer(object state)
        {
            if (_batcher != null)
            {
                _batcher.TriggerBatch();
            }
        }

        public bool Enqueue(TInput item)
        {
            if (!_buffer.Post(item))
            {
                // Increase the number of dropped events
                Interlocked.Increment(ref _droppedEvents);
                return false;
            }
            return true;
        }
        

        public void Dispose()
        {
            _windowTimer.Dispose();

            _tokenSource.Cancel();
            _buffer.Completion.Wait();
            _batcher.Completion.Wait();
            _aggregator.Completion.Wait();
            _publisher.Completion.Wait();

            foreach (var d in _disposables)
                d.Dispose();
        }
    }
}
