using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeIOModule
{

    public interface IIOCardControl : IDisposable
    {
        // Connection Management
        bool Initialize();
        bool IsConnected();
        bool Reset();
        string CardName { get; }
        int InputChannels { get; }
        int OutputChannels { get; }

        // GPIO Operations
        bool SetPinOutput(int pinNo, bool value);
        bool GetInputState(int pinNo);
        bool GetOutputState(int pinNo);
        void SetAllOutputsLow();
        void SetOutputChannels(int outputMask);
        void RefreshIOState();

        // Event Support
        event Action<int, bool> OnInputChanged; // pinNo (1-based), state
    }
}
