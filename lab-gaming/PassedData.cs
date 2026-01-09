using System;
using System.Collections.Generic;
using System.Text;

namespace lab_gaming
{
    public class PassedData
    {
        public bool didplayerwin;
        public bool didplayerlose;
        public bool wasitstrict;
        public string howlong;
        public int tries;
        public bool started_online;
        public PassedData()
        {

            didplayerwin = false;
            didplayerlose = false;
            wasitstrict = true;
            howlong = "";
            tries = 0;
        }
    }
}