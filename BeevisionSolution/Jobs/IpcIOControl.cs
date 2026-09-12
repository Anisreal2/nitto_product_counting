using BeevisionSolution.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cognex.VisionPro.Comm;
using System.Text;
using BeeIOModule;
using BeeIOModule.Models;

namespace BeevisionSolution.Jobs
{
    /// <summary>
    /// High-level facade for IO control
    /// Maintains backward compatibility with existing API
    /// </summary>
    public class IpcIOControl
    {
        public Action<int, bool, IpcIOControl> OnPinTriggered;
        [JsonIgnore]
        private CancellationTokenSource cancel = new CancellationTokenSource();

        [JsonIgnore]
        private IIOCardControl _ioCard; // Abstraction from BeeIOModule

        [JsonIgnore]
        private Task syncTask;

        private bool[] ArrInput;
        private bool[] ArrOutput;

        [JsonIgnore]
        public bool KeepSync { get; internal set; } = true;
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public bool IsInit { get; set; }
        public int Circle_Time { get; set; } = 100;
        public int InputChannels { get; set; } = 8;
        public int OutputChannels { get; set; } = 16;

        // Card type configuration
        public string CardType { get; set; } = "CognexCC24"; // "CognexCC24" or "PcieE2I12O16"
        public int CardId { get; set; } = 0; // For PCIe cards

        [JsonIgnore]
        public string CardName
        {
            get { return _ioCard?.CardName ?? ""; }
        }

        // Constructor
        public IpcIOControl()
        {
        }
        // Initialize IO card based on CardType configuration
        public void InitGPIO()
        {
            try
            {
                // Create IO card base on config
                _ioCard = CreateIOCard();

                if (_ioCard == null)
                {
                    Common.Bug("Failed to create IO card. Check CardType: {0}", CardType);
                    IsInit = false;
                    return;
                }
                _ioCard.OnInputChanged += HandleInputChanged;

                ArrInput = new bool[InputChannels];
                ArrOutput = new bool[OutputChannels];

                // Initialize the card
                bool result = _ioCard.Initialize();
                if (!result)
                {
                    Common.Bug("IO card initialization failed");
                    IsInit = false;
                    return;
                }
                _ioCard.SetAllOutputsLow();

                // Update channel counts from actual card
                InputChannels = _ioCard.InputChannels;
                OutputChannels = _ioCard.OutputChannels;

                IsInit = true;
                Common.Info("GPIO initialized successfully. Card: {0}, Type: {1}",
                    _ioCard.CardName, CardType);
            }
            catch (Exception ex)
            {
                Common.Bug("Init IO exception: {0}", ex.Message);
                IsInit = false;
            }
        }

        // create appropriate IO card instance
        private IIOCardControl CreateIOCard()
        {
            switch (CardType.ToLower())
            {
                case "cognexcc24":
                case "cc24":
                    var cc24 = new CognexCC24IOControl(InputChannels, OutputChannels);
                    cc24.CircleTime = Circle_Time;
                    return cc24;

                case "pciee2i12o16":
                case "pcie":
                    return new PcieE2I12O16IOControl(CardId);

                default:
                    Common.Bug("Unknown CardType: {0}. Using default CognexCC24", CardType);
                    return new CognexCC24IOControl(InputChannels, OutputChannels);
            }
        }

        // Handle input change events from IO card
        private void HandleInputChanged(int pinNo, bool state)
        {
            try
            {
                if (pinNo > 0 && pinNo <= InputChannels)
                {
                    ArrInput[pinNo - 1] = state;
                    OnPinTriggered?.Invoke(pinNo, state, this);
                }
            }
            catch (Exception ex)
            {
                Common.Bug("Error in HandleInputChanged: {0}", ex.Message);
            }
        }

        public void DoSync()
        {
            if (!IsInit)
            {
                Common.Bug("Cannot start sync - GPIO not initialized");
                return;
            }

            Common.Info("Begin sync IPC I/O {0}", Name);

            syncTask = Task.Run(() =>
            {
                try
                {
                    while (KeepSync && !cancel.Token.IsCancellationRequested)
                    {
                        _ioCard?.RefreshIOState();
                        Thread.Sleep(Circle_Time);
                    }
                }
                catch (Exception ex)
                {
                    Common.Bug("Sync loop exception: {0}", ex.Message);
                }
                finally
                {
                    Common.Info("End sync IPC I/O {0}", Name);
                }
            }, cancel.Token);
        }

        public void Abort()
        {
            Common.Info("Aborting IPC I/O {0}", Name);

            KeepSync = false;
            cancel.Cancel();

            if (_ioCard != null && IsInit)
            {
                try
                {
                    _ioCard.OnInputChanged -= HandleInputChanged;
                    _ioCard.SetAllOutputsLow();
                }
                catch (Exception ex)
                {
                    Common.Bug("Error during abort: {0}", ex.Message);
                }
            }

            if (syncTask != null && !syncTask.IsCompleted)
            {
                syncTask.Wait(1000);
            }
        }

        // Output Control 
        public bool SetPinOutput(int pinNo, bool value)
        {
            if (_ioCard == null || !IsInit)
            {
                Common.Bug("Cannot set output - GPIO not initialized");
                return false;
            }

            bool result = _ioCard.SetPinOutput(pinNo, value);
            if (result && pinNo > 0 && pinNo <= OutputChannels)
            {
                ArrOutput[pinNo - 1] = value;
            }
            return result;
        }

        //public bool PulseOutput(int pinNo, double durationMs = 10.0)
        //{
        //    // For Cognex CC24, use specific pulse method
        //    if (_ioCard is CognexCC24IOControl cc24)
        //    {
        //        return cc24.PulseOutput(pinNo, durationMs);
        //    }

        //    // For other cards
        //    if (!SetPinOutput(pinNo, true))
        //        return false;

        //    Task.Delay((int)durationMs).ContinueWith(_ => SetPinOutput(pinNo, false));
        //    return true;
        //}

        public void SetAllOutputsLow()
        {
            _ioCard?.SetAllOutputsLow();
            if (ArrOutput != null)
            {
                for (int i = 0; i < OutputChannels; i++)
                {
                    ArrOutput[i] = false;
                }
            }
        }

        public void SetOutputChannels(int outputMask)
        {
            _ioCard?.SetOutputChannels(outputMask);
            if (ArrOutput != null)
            {
                for (int i = 0; i < OutputChannels; i++)
                {
                    ArrOutput[i] = (outputMask & (1 << i)) != 0;
                }
            }
        }

        // Input Methods
        public bool GetInputState(int pinNo)
        {
            if (_ioCard == null || !IsInit)
            {
                Common.Bug("Cannot read input - GPIO not initialized");
                return false;
            }

            bool result = _ioCard.GetInputState(pinNo);
            if (pinNo > 0 && pinNo <= InputChannels)
            {
                ArrInput[pinNo - 1] = result;
            }
            return result;
        }

        public bool GetOutputState(int pinNo)
        {
            if (_ioCard == null || !IsInit)
                return false;

            return _ioCard.GetOutputState(pinNo);
        }

        public bool GetChannelInput(int channel)
        {
            if (_ioCard is PcieE2I12O16IOControl pcie)
            {
                return pcie.GetChannelInput(channel);
            }
            return GetInputState(channel + 1);
        }

        public bool SetChannelOutput(int channel, bool value)
        {
            if (_ioCard is PcieE2I12O16IOControl pcie)
            {
                return pcie.SetChannelOutput(channel, value);
            }
            return SetPinOutput(channel + 1, value);
        }

        public bool GetChannelOutput(int channel)
        {
            if (_ioCard is PcieE2I12O16IOControl pcie)
            {
                return pcie.GetChannelOutput(channel);
            }
            return GetOutputState(channel + 1);
        }

        public void RefreshIOState()
        {
            _ioCard?.RefreshIOState();
        }

        public string GetDiagnosticInfo()
        {
            if (_ioCard == null)
                return "No card initialized";

            StringBuilder info = new StringBuilder();
            info.AppendLine($"Card Name: {CardName}");
            info.AppendLine($"Card Type: {CardType}");
            info.AppendLine($"Is Initialized: {IsInit}");
            info.AppendLine($"Input Channels: {InputChannels}");
            info.AppendLine($"Output Channels: {OutputChannels}");
            info.AppendLine($"Circle Time: {Circle_Time}ms");
            info.AppendLine($"Keep Sync: {KeepSync}");

            return info.ToString();
        }

        public void Dispose()
        {
            Abort();
            _ioCard?.Dispose();
            cancel?.Dispose();
        }
    }
}