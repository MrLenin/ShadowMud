using System;
using System.Threading;

namespace ShadowMud.Cache
{
    public enum RWLockStatus
    {
        Unlocked,
        ReadLock,
        WriteLock
    }

    public class RWLock : IDisposable
    {
        #region Delegates

        public delegate TResultType DoWorkFunc<out TResultType>();

        #endregion

        public static int DefaultTimeout = 30000;
        private readonly ReaderWriterLock _lockObj;
        private readonly int _timeout;
        private LockCookie _cookie;
        private RWLockStatus _status = RWLockStatus.Unlocked;
        private bool _upgraded;

        #region delegate based methods 

        public static TResultType GetWriteLock<TResultType>(ReaderWriterLock lockObj, int timeout,
                                                            DoWorkFunc<TResultType> doWork)
        {
            RWLockStatus status = (lockObj.IsWriterLockHeld
                                       ? RWLockStatus.WriteLock
                                       : (lockObj.IsReaderLockHeld
                                              ? RWLockStatus.ReadLock
                                              : RWLockStatus.Unlocked));

            LockCookie writeLock = default(LockCookie);
            switch (status)
            {
                case RWLockStatus.ReadLock:
                    writeLock = lockObj.UpgradeToWriterLock(timeout);
                    break;

                case RWLockStatus.Unlocked:
                    lockObj.AcquireWriterLock(timeout);
                    break;
            }
            try
            {
                return doWork();
            }
            finally
            {
                switch (status)
                {
                    case RWLockStatus.ReadLock:
                        lockObj.DowngradeFromWriterLock(ref writeLock);
                        break;

                    case RWLockStatus.Unlocked:
                        lockObj.ReleaseWriterLock();
                        break;
                }
            }
        }

        public static TResultType GetReadLock<TResultType>(ReaderWriterLock lockObj, int timeout,
                                                           DoWorkFunc<TResultType> doWork)
        {
            bool releaseLock = false;

            if (!lockObj.IsWriterLockHeld && !lockObj.IsReaderLockHeld)
            {
                lockObj.AcquireReaderLock(timeout);
                releaseLock = true;
            }

            try
            {
                return doWork();
            }
            finally
            {
                if (releaseLock)
                    lockObj.ReleaseReaderLock();
            }
        }

        #endregion

        #region disposable based methods

        public RWLock(ReaderWriterLock lockObj, RWLockStatus status, int timeoutMs)
        {
            _lockObj = lockObj;
            _timeout = timeoutMs;
            Status = status;
        }

        public RWLockStatus Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;

                if (_status == RWLockStatus.Unlocked)
                {
                    _upgraded = false;

                    switch (value)
                    {
                        case RWLockStatus.ReadLock:
                            _lockObj.AcquireReaderLock(_timeout);
                            break;

                        case RWLockStatus.WriteLock:
                            _lockObj.AcquireWriterLock(_timeout);
                            break;
                    }
                }
                else
                    switch (value)
                    {
                        case RWLockStatus.Unlocked:
                            _lockObj.ReleaseLock();
                            break;

                        case RWLockStatus.WriteLock:
                            _cookie = _lockObj.UpgradeToWriterLock(_timeout);
                            _upgraded = true;
                            break;

                        default:
                            if (_upgraded) // value==RWLockStatus.ReadLock && status==RWLockStatus.WriteLock
                            {
                                _lockObj.DowngradeFromWriterLock(ref _cookie);
                                _upgraded = false;
                            }
                            else
                            {
                                _lockObj.ReleaseLock();
                                _status = RWLockStatus.Unlocked;
                                _lockObj.AcquireReaderLock(_timeout);
                            }
                            break;
                    }

                _status = value;
            }
        }

        public void Dispose()
        {
            Status = RWLockStatus.Unlocked;
        }

        public static RWLock GetReadLock(ReaderWriterLock lockObj)
        {
            return new RWLock(lockObj, RWLockStatus.ReadLock, DefaultTimeout);
        }

        public static RWLock GetWriteLock(ReaderWriterLock lockObj)
        {
            return new RWLock(lockObj, RWLockStatus.WriteLock, DefaultTimeout);
        }

        #endregion
    }
}