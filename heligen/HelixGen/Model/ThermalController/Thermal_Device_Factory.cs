using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System_Defns;

namespace System_Instruments.Thermal_Controller
{
    class CThermal_Device_Factory
    {
        public static CThermal_Device CreateNewThermalDevice(CDigital_PCR_Diagnostic_Instrument instr, uint uiTECIndex, float fDefaultStartTemperature = 20.0F)
        {
            return new CTEC_Thermal_Device(instr, uiTECIndex, fDefaultStartTemperature);
        }
    }
}
