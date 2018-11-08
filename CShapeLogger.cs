using System;
using System.Diagnostics;
using System.IO;

namespace Com.Github.CShapeLogger
{
    #region

    // 요약:
    //     Specifies the levels of trace messages filtered by the source switch and
    //     event type filter.
    [Flags]
    public enum SourceLevels
    {
        // 요약:
        //     Allows all events through.
        All = -1,
        //
        // 요약:
        //     Does not allow any events through.
        Off = 0,
        //
        // 요약:
        //     Allows only System.Diagnostics.TraceEventType.Critical events through.
        Fatal = 1,
        //
        // 요약:
        //     Allows System.Diagnostics.TraceEventType.Critical and System.Diagnostics.TraceEventType.Error
        //     events through.
        Error = 3,
        //
        // 요약:
        //     Allows System.Diagnostics.TraceEventType.Critical, System.Diagnostics.TraceEventType.Error,
        //     and System.Diagnostics.TraceEventType.Warning events through.
        Warning = 7,
        //
        // 요약:
        //     Allows System.Diagnostics.TraceEventType.Critical, System.Diagnostics.TraceEventType.Error,
        //     System.Diagnostics.TraceEventType.Warning, and System.Diagnostics.TraceEventType.Information
        //     events through.
        Information = 15,
        //
        // 요약:
        //     Allows System.Diagnostics.TraceEventType.Critical, System.Diagnostics.TraceEventType.Error,
        //     System.Diagnostics.TraceEventType.Warning, System.Diagnostics.TraceEventType.Information,
        //     and System.Diagnostics.TraceEventType.Verbose events through.
        Debug = 31,
    }
    #endregion

    class CShapeLogger
    {
        string __TRACE_LISTENER_NAME
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(
                    System.AppDomain.CurrentDomain.FriendlyName);
            }
        }

        TraceSource __TraceSource = null;
        static System.Diagnostics.TextWriterTraceListener __TraceListener = null;
        static void traceListenerClose(TraceListener traceListener)
        {
            traceListener.Close();
            traceListener.Dispose();
            traceListener = null;
        }

        ~CShapeLogger()
        {
            __TraceSource.Close();
            traceListenerClose(__TraceListener);
        }


        public CShapeLogger(string LogFileName)
        {
            __TraceSource = new TraceSource(__TRACE_LISTENER_NAME);

            if (__TraceListener != null)
                traceListenerClose(__TraceListener); // 리스너가 이미 있으면 종료

            //로그파일 출력 리스너 할당
            __TraceListener = new System.Diagnostics.TextWriterTraceListener(LogFileName);
            StreamWriter sw = __TraceListener.Writer as StreamWriter;
            sw.AutoFlush = true;

            //콘솔 출력 리스너 할당
            var console = new System.Diagnostics.TextWriterTraceListener(Console.Out);
            console.Filter = new System.Diagnostics.EventTypeFilter(System.Diagnostics.SourceLevels.Information);

            // 리스너 추가
            __TraceSource.Listeners.Clear();
            traceSourceListenersAdd(__TraceSource, console);
            traceSourceListenersAdd(__TraceSource, __TraceListener);


            TraceSourceSwitchLevel(SourceLevels.All);

        }

        string format(string message)
        {
            return message;
        }

        public void Debug(string message)
        {
            traceListenerCollectionOutputOptions(__TraceSource.Listeners,
                TraceOptions.DateTime);
            __TraceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }
        public void Information(string message)
        {
            traceListenerCollectionOutputOptions(__TraceSource.Listeners,
                TraceOptions.DateTime);
            __TraceSource.TraceEvent(TraceEventType.Information, 0, message);
        }
        public void Warning(string message)
        {
            traceListenerCollectionOutputOptions(__TraceSource.Listeners,
                TraceOptions.DateTime);
            __TraceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }
        public void Error(string message)
        {
            traceListenerCollectionOutputOptions(__TraceSource.Listeners,
                TraceOptions.DateTime | TraceOptions.Callstack);
            __TraceSource.TraceEvent(TraceEventType.Error, 0, message);
        }
        public void Fatal(string message)
        {
            traceListenerCollectionOutputOptions(__TraceSource.Listeners,
                TraceOptions.DateTime | TraceOptions.Callstack);
            __TraceSource.TraceEvent(TraceEventType.Critical, 0, message);
        }

        public void Debugf(string format, params object[] agrs)
        {
            Debug(string.Format(format, agrs));
        }
        public void Informationf(string format, params object[] agrs)
        {
            Information(string.Format(format, agrs));
        }
        public void Warningf(string format, params object[] agrs)
        {
            Warning(string.Format(format, agrs));
        }
        public void Errorf(string format, params object[] agrs)
        {
            Error(string.Format(format, agrs));
        }
        public void Fatalf(string format, params object[] agrs)
        {
            Fatal(string.Format(format, agrs));
        }

        public void TraceSourceSwitchLevel(SourceLevels level)
        {
            traceSourceSwitchLevel(__TraceSource, (System.Diagnostics.SourceLevels)level);
        }

        public void TraceSourceListenersAdd(TraceListener traceListener)
        {
            traceSourceListenersAdd(__TraceSource, traceListener);
        }

        public void TraceSourceListenersClear()
        {
            traceSourceListenersClear(__TraceSource);
        }

        #region private static
        static void traceSourceListenersAdd(TraceSource traceSource, TraceListener traceListener)
        {
            foreach (TraceListener item in traceSource.Listeners)
                if (item.Equals(traceListener))
                    return;

            traceSource.Listeners.Add(traceListener);
        }

        static void traceSourceListenersClear(TraceSource traceSource)
        {
            traceSource.Listeners.Clear();
        }

        static void traceSourceSwitchLevel(
            TraceSource traceSource,
            System.Diagnostics.SourceLevels level)
        {
            traceSource.Switch.Level = level;
        }

        static void traceListenerCollectionOutputOptions(
            TraceListenerCollection traceListenerCollection,
            TraceOptions traceOptions)
        {
            foreach (TraceListener item in traceListenerCollection)
                item.TraceOutputOptions = traceOptions;
        }
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            CShapeLogger l = new CShapeLogger(System.IO.Path.GetFileNameWithoutExtension(
                    System.AppDomain.CurrentDomain.FriendlyName) + ".log");

            l.TraceSourceSwitchLevel(SourceLevels.Debug);
            l.Debug("Debug");
            l.Information("Debug");
            l.Warning("Debug");
            l.Error("Debug");
            l.Fatal("Debug");

            l.TraceSourceSwitchLevel(SourceLevels.Information);
            l.Debug("Information");
            l.Information("Information");
            l.Warning("Information");
            l.Error("Information");
            l.Fatal("Information");

            l.TraceSourceSwitchLevel(SourceLevels.Warning);
            l.Debug("Warning");
            l.Information("Warning");
            l.Warning("Warning");
            l.Error("Warning");
            l.Fatal("Warning");

            l.TraceSourceSwitchLevel(SourceLevels.Error);
            l.Debug("Error");
            l.Information("Error");
            l.Warning("Error");
            l.Error("Error");
            l.Fatal("Error");

            l.TraceSourceSwitchLevel(SourceLevels.Fatal);
            l.Debug("Fatal");
            l.Information("Fatal");
            l.Warning("Fatal");
            l.Error("Fatal");
            l.Fatal("Fatal");
        }
    }
}
