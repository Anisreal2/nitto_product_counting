using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeIOModule.Models
{
    /// <summary>
    /// Adapter/Wrapper for PCIeE2I12O16 card
    /// </summary>
    public class PcieE2I12O16IOControl : IOCardControlBase
    {
        private int cardId;
        private bool[] ArrInput;
        private bool[] ArrOutput;
        private int circleTime = 100;

        public override string CardName => "PCIE-E2I12O16";
        public override int InputChannels => 12;
        public override int OutputChannels => 16;

        public int CircleTime
        {
            get => circleTime;
            set => circleTime = value;
        }

        public int CardId => cardId;

        public PcieE2I12O16IOControl(int cardId = 0)
        {
            this.cardId = cardId;
            ArrInput = new bool[InputChannels];
            ArrOutput = new bool[OutputChannels];
        }

        public override bool Initialize()
        {
            try
            {
                // Init card 
                int result = MiniPcieLib.MP_InitMiniPcie(cardId);
                if (result != 0)
                {
                    return false;
                }

                // Verify card by getting version
                int version = 0;
                result = MiniPcieLib.MP_E2I12O16_GetVersion(cardId, ref version);
                if (result == 0)
                {
                    IsInitialized = true;
                    MiniPcieLib.MP_E2I12O16_SetOpModeGPIO(cardId);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public override bool IsConnected()
        {
            if (!IsInitialized)
                return false;

            try
            {
                int mode = 0;
                int result = MiniPcieLib.MP_E2I12O16_GetOpMode(cardId, ref mode);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        public override bool Reset()
        {
            try
            {
                if (!IsInitialized)
                    return false;

                int result = MiniPcieLib.MP_E2I12O16_ResetCard(cardId);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        public override bool SetPinOutput(int pinNo, bool value)
        {
            if (pinNo == 0) pinNo = 1; // Hỗ trợ fallback nếu truyền 0-based bit index
            if (!ValidatePinNumber(pinNo, false))
                return false;

            if (!IsConnected())
                return false;

            try
            {
                int channel = pinNo - 1; // Convert to 0-based

                // Get current state
                int currentState = 0;
                int result = MiniPcieLib.MP_E2I12O16_GetGPIOOutput(cardId, ref currentState);
                if (result != 0)
                    return false;

                // Set or clear bit
                int newState;
                if (value)
                    newState = currentState | (1 << channel);
                else
                    newState = currentState & ~(1 << channel);

                // Set new state
                result = MiniPcieLib.MP_E2I12O16_SetGPIOOutput(cardId, newState);
                if (result == 0)
                {
                    ArrOutput[channel] = value;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override bool GetInputState(int pinNo)
        {
            if (pinNo == 0) pinNo = 1; // Hỗ trợ fallback nếu truyền 0-based bit index
            if (!ValidatePinNumber(pinNo, true))
                return false;

            if (!IsConnected())
                return false;

            try
            {
                int channel = pinNo - 1;
                int state = 0;
                int result = MiniPcieLib.MP_E2I12O16_GetGPIOInput(cardId, ref state);

                if (result != 0)
                    return false;

                bool newState = (state & (1 << channel)) != 0;
                bool oldState = ArrInput[channel];
                ArrInput[channel] = newState;

                // Raise event if state changed
                if (oldState != newState)
                {
                    RaiseInputChanged(pinNo, newState);
                }

                return newState;
            }
            catch
            {
                return false;
            }
        }

        public override bool GetOutputState(int pinNo)
        {
            if (pinNo == 0) pinNo = 1; // Hỗ trợ fallback nếu truyền 0-based bit index
            if (!ValidatePinNumber(pinNo, false))
                return false;

            if (!IsConnected())
                return false;

            try
            {
                int channel = pinNo - 1;
                int state = 0;
                int result = MiniPcieLib.MP_E2I12O16_GetGPIOOutput(cardId, ref state);

                if (result != 0)
                    return ArrOutput[channel]; // Return cached value

                bool newState = (state & (1 << channel)) != 0;
                ArrOutput[channel] = newState;
                return newState;
            }
            catch
            {
                return ArrOutput[pinNo - 1]; // Return cached value on error
            }
        }

        public bool GetChannelInput(int channel)
        {
            if (channel < 0 || channel >= InputChannels) return false;
            return GetInputState(channel + 1);
        }

        public bool SetChannelOutput(int channel, bool value)
        {
            if (channel < 0 || channel >= OutputChannels) return false;
            return SetPinOutput(channel + 1, value);
        }

        public bool GetChannelOutput(int channel)
        {
            if (channel < 0 || channel >= OutputChannels) return false;
            return GetOutputState(channel + 1);
        }

        public override void SetAllOutputsLow()
        {
            if (!IsConnected())
                return;

            try
            {
                int result = MiniPcieLib.MP_E2I12O16_SetGPIOOutput(cardId, 0);
                if (result == 0)
                {
                    for (int i = 0; i < OutputChannels; i++)
                    {
                        ArrOutput[i] = false;
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public override void SetOutputChannels(int outputMask)
        {
            if (!IsConnected())
                return;

            try
            {
                int result = MiniPcieLib.MP_E2I12O16_SetGPIOOutput(cardId, outputMask);
                if (result == 0)
                {
                    for (int i = 0; i < OutputChannels; i++)
                    {
                        ArrOutput[i] = (outputMask & (1 << i)) != 0;
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public override void RefreshIOState()
        {
            if (!IsConnected())
                return;

            try
            {
                // Read input state
                int inputState = 0;
                if (MiniPcieLib.MP_E2I12O16_GetGPIOInput(cardId, ref inputState) == 0)
                {
                    for (int i = 0; i < InputChannels; i++)
                    {
                        bool oldState = ArrInput[i];
                        ArrInput[i] = (inputState & (1 << i)) != 0;

                        if (oldState != ArrInput[i])
                        {
                            RaiseInputChanged(i + 1, ArrInput[i]);
                        }
                    }
                }

                // Read output state
                int outputState = 0;
                if (MiniPcieLib.MP_E2I12O16_GetGPIOOutput(cardId, ref outputState) == 0)
                {
                    for (int i = 0; i < OutputChannels; i++)
                    {
                        ArrOutput[i] = (outputState & (1 << i)) != 0;
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        // Expose additional PCIeE2I12O16 specific methods if needed
        public int GetVersion()
        {
            try
            {
                if (!IsConnected())
                    return -1;

                int version = 0;
                int result = MiniPcieLib.MP_E2I12O16_GetVersion(cardId, ref version);
                return result == 0 ? version : -1;
            }
            catch
            {
                return -1;
            }
        }

        public bool SetOperationMode(OperationMode mode)
        {
            try
            {
                if (!IsConnected())
                    return false;

                int result;
                if (mode == OperationMode.GPIO)
                    result = MiniPcieLib.MP_E2I12O16_SetOpModeGPIO(cardId);
                else
                    result = MiniPcieLib.MP_E2I12O16_SetOpModePulse(cardId);

                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        public enum OperationMode
        {
            GPIO = 0,
            Pulse = 1
        }

        public override void Dispose()
        {
            try
            {
                if (IsInitialized)
                {
                    SetAllOutputsLow();
                }
            }
            catch
            {
                // Silent fail
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
