using AvePoint.GCommon;
using AvePoint.RA.Common.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystemCore.ApiClient
{
    public partial class HyperHybridAPIClient : HybridSdk
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(HyperHybridAPIClient));

        private static readonly object s_locker = new object();

        private static HyperHybridAPIClient s_instance;

        public static HyperHybridAPIClient Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_locker)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new HyperHybridAPIClient();
                        }
                    }
                }

                return s_instance;
            }
        }
    }
}
