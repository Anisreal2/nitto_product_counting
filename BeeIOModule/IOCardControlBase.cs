using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeIOModule
{
    /// <summary>
    /// Abstract base class forr IO card implementations
    /// </summary>
    [Serializable]
    public abstract class IOCardControlBase : IIOCardControl
    {
        public abstract string CardName { get; }
        public abstract int InputChannels { get; }
        public abstract int OutputChannels { get; }

        public bool IsInitialized { get; protected set; }

        // Events
        public event Action<int, bool> OnInputChanged;

        protected void RaiseInputChanged(int pinNo, bool state)
        {
            OnInputChanged?.Invoke(pinNo, state);
        }

        // Abstract methods - must be implemented by derived classes
        public abstract bool Initialize();
        public abstract bool IsConnected();
        public abstract bool Reset();
        public abstract bool SetPinOutput(int pinNo, bool value);
        public abstract bool GetInputState(int pinNo);
        public abstract bool GetOutputState(int pinNo);
        public abstract void SetAllOutputsLow();
        public abstract void SetOutputChannels(int outputMask);
        public abstract void RefreshIOState();

        // Common validation
        protected bool ValidatePinNumber(int pinNo, bool isInput)
        {
            int maxChannels = isInput ? InputChannels : OutputChannels;
            if (pinNo < 1 || pinNo > maxChannels)
            {
                return false;
            }
            return true;
        }

        public virtual void Dispose()
        {
            if (IsInitialized)
            {
                SetAllOutputsLow();
                IsInitialized = false;
            }
        }
    }
}
