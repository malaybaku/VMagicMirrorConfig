using System;
using System.IO;

namespace Baku.VMagicMirrorConfig
{
    public class LogOutput
    {
        //なんとなくシングルトン
        private static LogOutput? _instance = null;
        public static LogOutput Instance
            => _instance ??= new LogOutput();
        private LogOutput()
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
            if (Directory.Exists(LogFileDir))
            {
                File.WriteAllText(LogFilePath, "");
            }
        }

        private string LogFileDir => SpecialFilePath.LogFileDir;
        private string LogFilePath => SpecialFilePath.LogFilePath;
        //指針: log.txtというテキストがあれば随時appendする(安全重視で毎回書き込んでファイル閉じる)
        private readonly object _writeLock = new object();


        public void Write(string text)
        {
            if (!File.Exists(LogFilePath)) { return; }

            lock (_writeLock)
            {
                try
                {
                    using (var sw = new StreamWriter(LogFilePath, true))
                    {
                        sw.WriteLine(text);
                    }
                }
                catch (Exception)
                {
                    //諦める
                }
            }
        }

        public void Write(Exception ex)
        {
            if (ex == null) { return; }

            Write(
                DateTime.Now.ToString("yyyyMMdd_HHmmss") + "\n" +
                ex.GetType().Name + "\n" +
                ex.Message + "\n" +
                ex.StackTrace
                );

        }

    }
}
