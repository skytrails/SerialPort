using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System_Instruments.Thermal_Controller
{
    public class CThermal_Adapter_Unit : CThermal_Adapter_Driver
    {
        private static Object m_Lock = new Object();
        private static Dictionary<string, CThermal_Adapter_Unit> m_instances = new Dictionary<string, CThermal_Adapter_Unit>(StringComparer.InvariantCultureIgnoreCase);
        public static CThermal_Adapter_Unit Instance(string strPort)
        {
            lock (m_Lock)
            {
                CThermal_Adapter_Unit inst = null;
                if (!m_instances.TryGetValue(strPort, out inst))
                {
                    inst = new CThermal_Adapter_Unit(strPort);
                    m_instances.Add(strPort, inst);
                }
                return inst;
            }
        }

        private int    _m_iOpenCount;
        private string _m_strPort;
        private CThermal_Adapter_Unit(string strPort)
        {
            _m_iOpenCount = 0;
            _m_strPort = strPort;
            m_instLock = new Object();
        }
        private Object m_instLock;

        public void open()
        {
            lock (m_instLock)
            {
                if (_m_iOpenCount == 0)
                {
                    _open();
                    _m_iOpenCount++;
                }
            }
        }

        public void close()
        {
            lock (m_instLock)
            {
                if (_m_iOpenCount == 0)
                {
                    throw new Exception("Cannot close unopened controller.");
                }
                if (_m_iOpenCount == 1)
                {
                    _close();
                }
                _m_iOpenCount--;
            }
        }

        private void _open()
        {
            // Real open stuff goes here
            Initialize(_m_strPort);
        }

        private void _close()
        {
            // Real close stuff goes here
            DeInitialize();
        }
    }
}
