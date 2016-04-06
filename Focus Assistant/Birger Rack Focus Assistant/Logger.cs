using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Birger_Rack_Focus_Assistant
{
    public class Logger
    {
        string _logPath = string.Empty;

        string _fName = string.Format("FocusAssistant-{0:MM-dd-yyyy__hh-mmtt}.log", DateTime.Now);

        string _fileName = string.Empty;

        static System.Threading.Semaphore locking = new System.Threading.Semaphore(1, 1);

        string fullFileName = string.Empty;

        public Logger(string logPath)
        {

            _logPath = logPath;

        }

        public Logger(string logPath, string fileName)
        {

            _logPath = logPath;

            _fileName = fileName;

        }

        public void LogError(string logInformation, EventLogEntryType errorType)
        {

            if (string.IsNullOrEmpty(logInformation)) return;

            if (_fileName.Equals(string.Empty))

                fullFileName = _logPath + _fName;

            else

                fullFileName = _logPath + _fileName;

            StringBuilder errorInfo = new StringBuilder();

            if (errorInfo.Length > 0) errorInfo.Remove(0, errorInfo.Length);

            errorInfo.Append(errorType.ToString() + " : " + DateTime.Now.ToString("dd-MMM-yyyy hh mm ss fff tt") + " : ");

            errorInfo.Append(" Message – " + logInformation);

            try
            {

                locking.WaitOne();

                StreamWriter Txt = null;

                if (File.Exists(fullFileName) == false)
                {

                    Txt = File.CreateText(fullFileName);

                }

                else
                {

                    Txt = File.AppendText(fullFileName);

                    FileInfo into = new FileInfo(fullFileName);

                    long aa = into.Length;

                    if (aa >= (1024 * 1024))
                    {

                        File.Delete(fullFileName);

                        Txt = File.CreateText(fullFileName);

                    }

                }

                Txt.WriteLine(errorInfo.ToString());

                Txt.Close();

            }

            catch (Exception ex)
            {

            }

            finally
            {

                locking.Release();

            }

        }
    }
}
