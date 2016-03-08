using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace Kiwoom
{
    public class Log
    {
        public static void Init()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
            
        }

        public static ILog Get(Type type)
        {
            return LogManager.GetLogger(type);
        }
        
    }
}
