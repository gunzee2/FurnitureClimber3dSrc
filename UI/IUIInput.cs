using System;
using UniRx;

namespace UI
{
    public interface IUIInput
    {
        public IObservable<Unit> OnCloseMenuPressed { get; }
    }
}
