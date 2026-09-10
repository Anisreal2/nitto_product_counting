using System;
using System.Threading;
using System.Threading.Tasks;
using BeeMotionModule.Models;

namespace BeeMotionModule
{
    /// <summary>
    /// Standard interface for Motion Controllers (EtherCAT, PCI Card, Simulation)
    /// </summary>
    public interface IMotionController : IDisposable
    {
        bool IsConnected { get; }
        bool IsMasterOp { get; }
        uint MasterStatus { get; }
        MotionConfig Config { get; }

        event Action<short, AxisState> OnAxisStateUpdated;
        event Action<string> OnLogMessage;
        event Action<uint, string> OnError;

        bool Init(MotionConfig config);
        void Close();
        bool ScanBus();
        
        bool ServoOn(short axis);
        bool ServoOff(short axis);
        bool ClearAlarm(short axis);
        bool SetZero(short axis);
        
        Task<bool> HomeAsync(short axis, CancellationToken ct = default);
        bool MoveJog(short axis, double velocity);
        bool Stop(short axis);
        bool EmergencyStop();
        
        bool MoveAbsolute(short axis, double targetPosUnits, double velocity = 0, double acc = 0, double dec = 0);
        bool MoveRelative(short axis, double distanceUnits, double velocity = 0, double acc = 0, double dec = 0);
        Task<bool> WaitMoveDoneAsync(short axis, uint timeoutMs = 30000, CancellationToken ct = default);
        
        AxisState GetAxisState(short axis);
        bool SetupPositionCompare(short axis, double startPos, double interval, int count);
        
        bool SetDigitalOutput(short doPin, bool state);
        bool GetDigitalInput(short diPin);
        bool GetDigitalOutput(short doPin);

        // Pneumatics & IO convenience helpers
        bool IsCylinderForward { get; }
        bool IsVacuumOn { get; }
        bool SetCylinder(bool forward);
        bool SetVacuum(bool on);
        bool GetCylinderForwardSensor();
        bool GetCylinderBackwardSensor();
        bool GetVacuumSensor();
        bool GetSystemStopSensor();
    }
}

