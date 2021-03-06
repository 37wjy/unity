using UnityEngine;
using System.Collections.Generic;
using Puerts;
using System;

public class LoggerHelper : MonoSingleton<LoggerHelper>
{
    public enum LOG_TYPE
    {
        LOG = 0,
        LOG_ERR,
    }

    struct log_info
    {
        public LOG_TYPE type;
        public string msg;

        public log_info(LOG_TYPE type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }

    private static LoggerHelper _instance = null;
    private List<log_info> logList = new List<log_info>(100);
    private List<log_info> tmpLogList = new List<log_info>(100);

    public System.Action<string> mOnShowLoggerError = null;

    protected override void Init()
    {
        if (!Application.isEditor)
        {
            Application.logMessageReceived += (LogHandler);

            InvokeRepeating("CheckReport", 1f, 1f);
        }
    }

    private void LogHandler(string condition, string stackTrace, LogType type)
    {
        if (Application.isEditor)
        {
            return;
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            Logger.LogError(condition + " \n" + stackTrace);
        }
    }

    private void CheckReport()
    {
        Logger.CheckReportError();
    }
    
    private void Update()
    {
        lock (logList)
        {
            if (logList.Count > 0)
            {
                tmpLogList.Clear();
                for (int i = 0; i < logList.Count; i++)
                {
                    tmpLogList.Add(logList[i]);
                }
                logList.Clear();
            }
        }

        if (tmpLogList.Count > 0)
        {
            for (int i = 0; i < tmpLogList.Count; i++)
            {
                var logInfo = tmpLogList[i];
                switch (logInfo.type)
                {
                    case LOG_TYPE.LOG:
                        {
                            Logger.Log(logInfo.msg, null);
                            break;
                        }
                    case LOG_TYPE.LOG_ERR:
                        {
                            Logger.LogError(logInfo.msg, null);
                            break;
                        }
                }
            }
            tmpLogList.Clear();
        }
    }

    public override void Dispose()
    {
        lock (logList)
        {
            logList.Clear();
        }
        tmpLogList.Clear();
        base.Dispose();
    }

    public void LogToCustomer(string s, params object[] p)
    {
        var error = (p != null && p.Length > 0 ? string.Format(s, p) : s);
        
        if (mOnShowLoggerError != null)
        {
            mOnShowLoggerError(error);
        }
    }

    public void RegisterCustomerLoggerError(System.Action<string> handler)
    {
        mOnShowLoggerError = null;
        if (handler != null)
        {
            mOnShowLoggerError = handler;
        }
    }

    public void LogToMainThread(LOG_TYPE type, string msg)
    {
        lock (logList)
        {
            logList.Add(new log_info(type, msg));
        }
    }
    
    
    public bool GetKeyDownString(string keycode)
    {
        return Input.GetKeyDown(keycode);
    }
        
    
    public bool GetKeyDownInt(int keycode)
    {
        return Input.GetKeyDown((KeyCode)keycode);
    }   
        
    
    public bool GetKeyDownKeyCode(KeyCode keycode)
    {
        return Input.GetKeyDown((KeyCode)keycode);
    }
}

#if UNITY_EDITOR
public static class LoggerHelperExporter
{
    
    public static List<Type> LuaCallCSharp = new List<Type>()
        {
            typeof(LoggerHelper),
        };
}
#endif
