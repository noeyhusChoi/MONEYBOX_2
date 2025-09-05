using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels
{
    // TODO: 뷰모델 공통 인터페이스 재정의 필요
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
        Func<bool?, Task>? OnStepNext { get; set; }
    }

    public interface IStepPrevious
    {
        Func<Task>? OnStepPrevious { get; set; }
        }
}
