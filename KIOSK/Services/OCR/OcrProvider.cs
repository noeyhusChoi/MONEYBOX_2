using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.NewFolder
{
    public interface IOcrProvider
    {
        Task<OcrOutcome> RunAsync(Pr22.Processing.Page page, CancellationToken ct);
    }
}
