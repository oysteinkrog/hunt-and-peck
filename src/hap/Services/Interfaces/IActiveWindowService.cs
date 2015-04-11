using System;
using Caliburn.Micro;
using Action = System.Action;

namespace hap.Services.Interfaces
{
    public interface IActiveWindowService
    {
        event Action ActiveWindowChanged;
    }
}