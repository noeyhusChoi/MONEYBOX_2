using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels
{
    public interface IStepError
    {
        Action<Exception>? OnStepError { get; set; }
    }

    public interface IStepMain
    {
        Func<Task>? OnStepMain { get; set; }
    }

    public interface IStepNext
    {
        Func<Task>? OnStepNext { get; set; }
    }

    public interface IStepPrevious
    {
        Func<Task>? OnStepPrevious { get; set; }
        }
}
