// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections;
using System.Collections.Generic;

namespace SnipInsight.Package
{
    /// <summary>
    /// Represents a Sorter for SnipInsightLink. This dramatically improves sorting speed in WPF.
    /// </summary>
    public class SnipInsightLinkTimeSorter : IComparer, IComparer<SnipInsightLink>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as SnipInsightLink, y as SnipInsightLink);
        }

        public int Compare(SnipInsightLink x, SnipInsightLink y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;
                else
                    return 1;
            }
            else if (y == null)
            {
                return -1;
            }
            else
            {
                return y.LastWriteTime.CompareTo(x.LastWriteTime);
            }
        }
    }
}
