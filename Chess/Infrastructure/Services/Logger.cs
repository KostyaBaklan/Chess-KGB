using System;
using System.IO;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Position;

namespace Infrastructure.Services
{
    public class Logger:ILog
    {
        private readonly StreamWriter _writer;

        public Logger()
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }
            var file = Path.Combine("Log", $"Positions_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");
            _writer = new StreamWriter(file){AutoFlush=true};
        }

        #region Implementation of ILog

        public void Log(IPosition position)
        {
            _writer.WriteLine($"Value = {position.GetValue()}");
            _writer.WriteLine(position.GetHistory());
            _writer.WriteLine(position);
            _writer.WriteLine();
        }

        #endregion
    }
}
