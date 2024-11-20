using UnityEngine;
namespace TCS.PhysicsDropper {
    internal static class Logger {
        const string CLASS_NAME = "PhysicsDropper";
        const string CLASS_COLOR = "cyan";
        const string CONTEXT_COLOR = "white";
        
        const string COLOR_LOG = "green";
        const string TEXT_LOG ="LOG:";
        
        const string COLOR_WARNING = "yellow";
        const string TEXT_WARNING = "WARNING:";
        
        const string COLOR_ERROR = "red";
        const string TEXT_ERROR = "ERROR:";
        
        const string COLOR_ASSERT = "cyan";
        const string TEXT_ASSERT = "ASSERT:";
        
        const string COLOR_TODO = "orange";
        const string TEXT_TODO = "TODO:";

        static string SetLogPrefix(this object newString, LogType logType) {
            string color = logType switch {
                LogType.Log => COLOR_LOG,
                LogType.Warning => COLOR_WARNING,
                LogType.Error => COLOR_ERROR,
                LogType.Assert => COLOR_ASSERT,
                LogType.TODO => COLOR_TODO,
                _ => COLOR_LOG,
            };
            return $"<b><color={color}>{newString}</color></b>";
        }   
        
        static string SetClassPrefix(this object newString)
            => $"<color=white><b>[</b><color={CLASS_COLOR}>{newString}</color><b>]</b></color>";

        static string SetMessagePrefix(this object newString)   
            => $"<color={CONTEXT_COLOR}>{newString}</color>";

        static string FormatMessage(object message, LogType logType) {
            // [Classname/No color] [LogType/Color] [Message/No color]
            string type = logType switch {
                LogType.Log => TEXT_LOG,
                LogType.Warning => TEXT_WARNING,
                LogType.Error => TEXT_ERROR,
                LogType.Assert => TEXT_ASSERT,
                LogType.TODO => TEXT_TODO,
                _ => "",
            };
            
            return $"{CLASS_NAME.SetClassPrefix()}" +
                   $" {type.SetLogPrefix(logType)} " +
                   $"{message.SetMessagePrefix()}";
        }

        static void LogInternal(object message, LogType logType, Object context = null) {
            
            string fixedMessage = FormatMessage(message, logType);
            
            switch (logType) {
                case LogType.Warning:
                    if (!context) {
                        Debug.LogWarning(fixedMessage);
                        break;
                    }
                    Debug.LogWarning(fixedMessage, context);
                    break;
                
                case LogType.Error:
                    if (!context) {
                        Debug.LogError(fixedMessage);
                        break;
                    }
                    Debug.LogError(fixedMessage, context);
                    break;
                
                case LogType.Assert:
                    if (!context) {
                        Debug.LogAssertion(fixedMessage);
                        break;
                    }
                    Debug.LogAssertion(fixedMessage, context);
                    break;
                
                case LogType.Exception:
                    string exception = message.SetMessagePrefix();
                    if (!context) {
                        Debug.LogException
                        (
                            new System.Exception(exception)
                        );
                        break;
                    }
                    Debug.LogException(new System.Exception(exception), context);
                    break;
                
                case LogType.TODO:
                case LogType.Log:
                default:
                    if (!context) {
                        Debug.Log(fixedMessage);
                        break;
                    }
                    Debug.Log(fixedMessage, context);
                    break;

            }
        }

        //Without context
        public static void Log(object message) => LogInternal(message, LogType.Log);
        public static void LogWarning(object message) => LogInternal(message, LogType.Warning);
        public static void LogError(object message) => LogInternal(message, LogType.Error);
        public static void LogAssert(object message) => LogInternal(message, LogType.Assert);
        public static void LogException(object message) => LogInternal(message, LogType.Exception);
        public static void LogTODO(object message) => LogInternal(message, LogType.TODO);

        //With context
        public static void Log(object message, Object ctx) => LogInternal(message, LogType.Log, ctx);
        public static void LogWarning(object message, Object ctx) => LogInternal(message, LogType.Warning, ctx);
        public static void LogError(object message, Object ctx) => LogInternal(message, LogType.Error, ctx);
        public static void LogAssert(object message, Object ctx) => LogInternal(message, LogType.Assert, ctx);
        public static void LogException(object message, Object ctx) => LogInternal(message, LogType.Exception, ctx);
        public static void LogTODO(object message, Object ctx) => LogInternal(message, LogType.TODO, ctx);

        enum LogType {
            Log,
            Warning,
            Error,
            Assert,
            Exception,
            TODO,
        }
    }
}