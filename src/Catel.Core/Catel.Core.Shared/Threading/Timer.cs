﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Timer.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if WIN80 || PCL

namespace System.Threading
{
    using Tasks;

    /// <summary>
    /// Timer callback delegate.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    public delegate void TimerCallback(object state);

#if !PCL
    /// <summary>
    /// The timeout class.
    /// </summary>
    public static class Timeout
    {
        /// <summary>
        /// A constant used to specify an infinite waiting period, for threading methods that accept an Int32 parameter.
        /// </summary>
        public const int Infinite = -1;
    }
#endif

    /// <summary>
    /// Timer for WinRT since WinRT only provides the DispatcherTimer which cannot be used outside the UI thread.
    /// </summary>
    public class Timer : IDisposable
    {
        #region Fields
        private readonly TimerCallback _timerCallback;
        private readonly object _timerState;

        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        public Timer()
            : this(100)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        /// <param name="interval">The interval in milliseconds.</param>
        public Timer(int interval)
        {
            Interval = interval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public Timer(TimerCallback callback)
            : this(callback, null, Timeout.Infinite, Timeout.Infinite)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer" /> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="interval">The interval.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="callback" /> is <c>null</c>.</exception>
        public Timer(TimerCallback callback, object state, int dueTime, int interval)
            : this(callback, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(interval))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer" /> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="interval">The interval.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="callback" /> is <c>null</c>.</exception>
        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan interval)
        {
            _timerCallback = callback;
            _timerState = state;

            Change(dueTime, interval);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval. The default is 100 milliseconds.</value>
        public int Interval { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event EventHandler<EventArgs> Elapsed;
        #endregion

        #region Methods

        /// <summary>
        /// Changes the specified interval.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="interval">The interval.</param>
        public void Change(int dueTime, int interval)
        {
            Change(TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(interval));
        }

        /// <summary>
        /// Changes the specified interval.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="interval">The interval.</param>
        public void Change(TimeSpan dueTime, TimeSpan interval)
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Interval = (int)interval.TotalMilliseconds;

            if (dueTime < TimeSpan.Zero)
            {
                // Never invoke initial one
            }
            else if (dueTime == TimeSpan.Zero)
            {
                // Invoke immediately
                TimerElapsed();

                Start(cancellationToken);
            }
            else
            {
                // Invoke after due time
                Task.Delay(dueTime, cancellationToken).ContinueWith(ContinueTimer, cancellationToken, cancellationToken);
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private void Start(CancellationToken cancellationToken)
        {
            if (Interval <= 0)
            {
                // Never start a timer
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Task.Delay(Interval, cancellationToken).ContinueWith(ContinueTimer, cancellationToken, cancellationToken);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Continues the timer.
        /// </summary>
        /// <param name="t">The task.</param>
        /// <param name="state">The state which must be the cancellation token.</param>
        private void ContinueTimer(Task t, object state)
        {
            var cancellationToken = (CancellationToken)state;
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            TimerElapsed();

            if (!cancellationToken.IsCancellationRequested)
            {
                Start(cancellationToken);
            }
        }

        /// <summary>
        /// Called when the interval elapses.
        /// </summary>
        private void TimerElapsed()
        {
            var elapsed = Elapsed;
            if (elapsed != null)
            {
                elapsed(this, EventArgs.Empty);
            }

            if (_timerCallback != null)
            {
                _timerCallback(_timerState);
            }
        }
        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
#endif