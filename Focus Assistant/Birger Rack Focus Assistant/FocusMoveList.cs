using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Birger_Rack_Focus_Assistant
{
    [Serializable]
    public class FocusMoveList
    {
        public FocusMoveList()
        {
            this.FocusMoves = new List<FocusMove>();
        }

        public List<FocusMove> FocusMoves
        {
            get;

            set;
        }
    }
}
