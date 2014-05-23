using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.KRunner.AppVeyor
{
    public enum Outcome
    {
        None,
        Running,
        Passed,
        Failed,
        Ignored,
        Skipped,
        Inconclusive,
        NotFound,
        Cancelled,
        NotRunnable
    }
}
