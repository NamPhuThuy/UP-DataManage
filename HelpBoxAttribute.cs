using UnityEngine;

namespace NamPhuThuy.DataManage
{
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public HelpBoxMessageType MessageType { get; }

        public HelpBoxAttribute(string message, HelpBoxMessageType messageType = HelpBoxMessageType.Info)
        {
            Message = message;
            MessageType = messageType;
        }
    }

    public enum HelpBoxMessageType
    {
        None,
        Info,
        Warning,
        Error
    }
}