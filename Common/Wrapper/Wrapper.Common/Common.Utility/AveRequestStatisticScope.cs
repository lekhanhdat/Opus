using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public struct AveRequestStatisticScope : IDisposable
    {
        private AveRequestTimer mTimer;

        public AveRequestStatisticScope(string scopeName)
        {
            mTimer = AveRequestStatisticMonitor.Start(scopeName);
        }

        public void Dispose()
        {
            AveRequestStatisticMonitor.Stop(mTimer);
        }
    }
}
