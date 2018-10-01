using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SnipInsight.Forms.Tests.Common
{
    public class CancelationHelper
    {
        public static CancellationToken CancelToken
        {
            get
            {
                return new CancellationToken();
            }
        }
    }
}
