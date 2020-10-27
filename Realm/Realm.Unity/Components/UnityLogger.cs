using Mono.Cecil.Cil;

namespace Weaver
{
    public class Logger : WeaverBase.ILogger
    {
        public void Info(string message, SequencePoint sequencePoint = null)
        {
            UnityEngine.Debug.Log(GetMessage(message, sequencePoint));
        }

        public void Warning(string message, SequencePoint sequencePoint = null)
        {
            UnityEngine.Debug.LogWarning(GetMessage(message, sequencePoint));
        }

        public void Error(string message, SequencePoint sequencePoint = null)
        {
            UnityEngine.Debug.LogError(GetMessage(message, sequencePoint));
        }

        public void Debug(string message, SequencePoint sequencePoint = null)
        {
            System.Diagnostics.Debug.WriteLine(GetMessage(message, sequencePoint));
        }

        private static string GetMessage(string message, SequencePoint sp)
        {
            if (sp == null)
            {
                return message;
            }

            return $"{sp.Document.Url}({sp.StartLine}, {sp.StartColumn}): {message}";
        }
    }
}
