using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ABot2
{
    public static class clsGlobals
    {
        public static List<object[]> _Protocol = new List<object[]>();

        // JAD public static clsAssayCmds Commands = new clsAssayCmds();
        public static clsAMB objAMB = new clsAMB();

        public static bool bGUIStopSign = false;
        public static bool bGUIUpdateRequest = false;
        public static int iPercentDone = 0;

        public static string sProtocolFName = "";
        public static bool bProtocolIsLoadedOKAndReadyToRun = false;

        public static string sSelectedCommandToAdd = "";

    }
}
