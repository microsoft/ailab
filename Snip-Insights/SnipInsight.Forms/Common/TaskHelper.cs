using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnipInsight.Forms.Common
{
    public static class TaskHelper
    {
        public static void Run(Task task)
        {
            Task.Run(() => task);
        }
    }
}
