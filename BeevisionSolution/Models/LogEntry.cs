using BeevisionSolution.Utils;
using System.Collections.Generic;

namespace BeevisionSolution.Models
{
    public class LogEntry : PropertyChangedAbstract
    {
        private int _CamId;
        private string _DateTime;
        private string _Source;
        private string _Message;

        public int CamId
        {
            get => _CamId;
            internal set
            {
                _CamId = value;
                Notify();
            }
        }

        public string DateTime
        {
            get => _DateTime;
            internal set
            {
                _DateTime = value;
                Notify();
            }
        }

        public string Source
        {
            get => _Source;
            internal set
            {
                _Source = value;
                Notify();
            }
        }

        public string Message
        {
            get => _Message;
            internal set
            {
                _Message = value;
                Notify();
            }
        }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }
}
