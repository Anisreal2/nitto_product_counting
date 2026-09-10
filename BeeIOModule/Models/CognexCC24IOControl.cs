using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cognex.VisionPro.Comm;

namespace BeeIOModule.Models
{
    /// <summary>
    /// Implementation cho Cognex CC24 Communication Card
    /// </summary>
    public class CognexCC24IOControl : IOCardControlBase
    {
        private CogCommCards mCards;
        private CogCommCard mCard;
        private CogPrio mPrio;
        private CogPrioState mT0;
        private bool[] ArrInput;
        private bool[] ArrOutput;
        private Task syncTask;
        private CancellationTokenSource cancel = new CancellationTokenSource();
        private int circleTime = 100; // ms

        public override string CardName => mCard?.Name ?? "Cognex CC24";
        public override int InputChannels { get; }
        public override int OutputChannels { get; }

        public int CircleTime
        {
            get => circleTime;
            set => circleTime = value;
        }

        public CognexCC24IOControl(int inputChannels = 8, int outputChannels = 16)
        {
            InputChannels = inputChannels;
            OutputChannels = outputChannels;
            ArrInput = new bool[InputChannels];
            ArrOutput = new bool[OutputChannels];
        }

        public override bool Initialize()
        {
            try
            {
                // Enumerate all Comm Cards
                mCards = new CogCommCards();
                if (mCards.Count == 0)
                {
                    return false;
                }

                // Use the first Comm Card
                mCard = mCards[0];

                // Check for Discrete I/O support
                if (mCard.DiscreteIOAccess == null)
                {
                    return false;
                }

                // Create Precision I/O interface
                mPrio = mCard.DiscreteIOAccess.CreatePrecisionIO();
                mPrio.DisableEvents();

                // Configure events
                ConfigureDefaultEvents();

                // Read initial state
                mT0 = mPrio.ReadState();

                // Initialize output state
                int numOutputs = mPrio.GetNumLines(CogPrioBankConstants.OutputBank0);
                for (int i = 0; i < OutputChannels && i < numOutputs; i++)
                {
                    ArrOutput[i] = mT0[CogPrioBankConstants.OutputBank0, i];
                }

                // Set all outputs to low
                SetAllOutputsLow();

                // Enable events
                mPrio.EnableEvents();

                IsInitialized = true;
                return true;
            }
            catch (Exception)
            {
                IsInitialized = false;
                return false;
            }
        }

        public override bool IsConnected()
        {
            return IsInitialized && mPrio != null && mCard != null;
        }

        public override bool Reset()
        {
            try
            {
                if (mPrio != null && IsInitialized)
                {
                    mPrio.DisableEvents();
                    SetAllOutputsLow();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ConfigureDefaultEvents()
        {
            CogPrioEventCollection prioEvents = new CogPrioEventCollection();

            // Configure input events
            int numInputs = mPrio.GetNumLines(CogPrioBankConstants.InputBank0);
            for (int i = 0; i < InputChannels && i < numInputs; i++)
            {
                CogPrioEvent prioEvent = new CogPrioEvent()
                {
                    Name = String.Format("InputChanged_{0}", i),
                    CausesLine = new CogPrioEventCauseLineCollection()
                    {
                        new CogPrioEventCauseLine()
                        {
                            LineBank = CogPrioBankConstants.InputBank0,
                            LineNumber = i,
                            LineTransition = CogPrioLineTransitionConstants.Any
                        }
                    }
                };

                prioEvent.HostNotification += (sender, e) => InputChanged_HostNotification(sender, e);
                prioEvents.Add(prioEvent);
            }

            // Configure output pulse events
            int numOutputs = mPrio.GetNumLines(CogPrioBankConstants.OutputBank0);
            for (int i = 0; i < OutputChannels && i < numOutputs; i++)
            {
                CogPrioEvent prioEvent = new CogPrioEvent()
                {
                    Name = String.Format("PulseOutput_{0}", i),
                    ResponsesLine = new CogPrioEventResponseLineCollection()
                    {
                        new CogPrioEventResponseLine()
                        {
                            OutputLineBank = CogPrioBankConstants.OutputBank0,
                            OutputLineNumber = i,
                            OutputLineValue = CogPrioOutputLineValueConstants.SetHigh,
                            PulseDuration = 10.0,
                            DelayType = CogPrioDelayTypeConstants.None,
                            DelayValue = 0.0
                        }
                    }
                };
                prioEvents.Add(prioEvent);
            }

            mPrio.Events.Clear();
            mPrio.Events = prioEvents;

            if (!mPrio.Valid)
            {
                throw new Exception("PRIO configuration invalid: " + mPrio.ValidationErrorMsg[0]);
            }
        }

        private void InputChanged_HostNotification(object sender, CogPrioEventArgs e)
        {
            try
            {
                string[] parts = e.EventName.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lineNum))
                {
                    bool currentState = e.State[CogPrioBankConstants.InputBank0, lineNum];
                    if (lineNum < InputChannels && currentState != ArrInput[lineNum])
                    {
                        ArrInput[lineNum] = currentState;
                        RaiseInputChanged(lineNum + 1, currentState); // 1-based pin number
                    }
                }
            }
            catch
            {
                // Silent fail for event handler
            }
        }

        public override bool SetPinOutput(int pinNo, bool value)
        {
            if (!ValidatePinNumber(pinNo, false))
                return false;

            if (mPrio == null || !IsInitialized)
                return false;

            try
            {
                int lineNum = pinNo - 1;
                CogPrioOutputLineValueConstants valueToSet = value ?
                    CogPrioOutputLineValueConstants.SetHigh :
                    CogPrioOutputLineValueConstants.SetLow;

                mPrio.SetOutput(CogPrioBankConstants.OutputBank0, lineNum, valueToSet);
                ArrOutput[lineNum] = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool GetInputState(int pinNo)
        {
            if (!ValidatePinNumber(pinNo, true))
                return false;

            if (mPrio == null || !IsInitialized)
                return false;

            try
            {
                int lineNum = pinNo - 1;
                CogPrioState state = mPrio.ReadState();
                bool result = state[CogPrioBankConstants.InputBank0, lineNum];
                ArrInput[lineNum] = result;
                return result;
            }
            catch
            {
                return false;
            }
        }

        public override bool GetOutputState(int pinNo)
        {
            if (!ValidatePinNumber(pinNo, false))
                return false;

            int lineNum = pinNo - 1;
            return ArrOutput[lineNum];
        }

        public override void SetAllOutputsLow()
        {
            if (mPrio == null || !IsInitialized)
                return;

            try
            {
                for (int i = 0; i < OutputChannels; i++)
                {
                    mPrio.SetOutput(CogPrioBankConstants.OutputBank0, i,
                        CogPrioOutputLineValueConstants.SetLow);
                    ArrOutput[i] = false;
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public override void SetOutputChannels(int outputMask)
        {
            if (mPrio == null || !IsInitialized)
                return;

            try
            {
                for (int i = 0; i < OutputChannels; i++)
                {
                    bool value = (outputMask & (1 << i)) != 0;
                    CogPrioOutputLineValueConstants valueToSet = value ?
                        CogPrioOutputLineValueConstants.SetHigh :
                        CogPrioOutputLineValueConstants.SetLow;

                    mPrio.SetOutput(CogPrioBankConstants.OutputBank0, i, valueToSet);
                    ArrOutput[i] = value;
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public override void RefreshIOState()
        {
            if (mPrio == null || !IsInitialized)
                return;

            try
            {
                CogPrioState state = mPrio.ReadState();

                for (int i = 0; i < InputChannels; i++)
                {
                    bool oldState = ArrInput[i];
                    ArrInput[i] = state[CogPrioBankConstants.InputBank0, i];

                    if (oldState != ArrInput[i])
                    {
                        RaiseInputChanged(i + 1, ArrInput[i]);
                    }
                }

                for (int i = 0; i < OutputChannels; i++)
                {
                    ArrOutput[i] = state[CogPrioBankConstants.OutputBank0, i];
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public bool PulseOutput(int pinNo, double durationMs = 10.0)
        {
            if (!ValidatePinNumber(pinNo, false))
                return false;

            if (mPrio == null || !IsInitialized)
                return false;

            try
            {
                int lineNum = pinNo - 1;
                string eventName = String.Format("PulseOutput_{0}", lineNum);

                if (mPrio.Events[eventName] != null)
                {
                    mPrio.Events[eventName].Schedule();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override void Dispose()
        {
            try
            {
                cancel?.Cancel();
                if (mPrio != null && IsInitialized)
                {
                    mPrio.DisableEvents();
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
