using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Birger_Rack_Focus_Assistant
{
    [Serializable]
    public class FocusMove
    {
        private int _startingFocusThumb = 0;
        private int _startingIrisThumb = 0;
        private int _endingFocusThumb = 0;
        private int _endingIrisThumb = 0;
        private string _startingFocusDistance;
        private string _endingFocusDistance;
        private string _startingFstop;
        private string _endingFstop;
        private TimeSpan _focusDuration = new TimeSpan();

        public FocusMove()
        {

        }

        public FocusMove(int startingFocusThumb, int endingFocusThumb, int startingIrisThumb, int endingIrisThumb, string startingFocusDistance, string endingFocusDistance,
            string startingFstop, string endingFstop, TimeSpan focusDuration)
        {
            _startingFocusThumb = startingFocusThumb;
            _startingIrisThumb = startingIrisThumb;
            _endingFocusThumb = endingFocusThumb;
            _endingIrisThumb = endingIrisThumb;
            _startingFocusDistance = startingFocusDistance;
            _endingFocusDistance = endingFocusDistance;
            _focusDuration = focusDuration;
            _startingFstop = startingFstop;
            _endingFstop = endingFstop;
        }

        public bool IsValid()
        {
            bool retval = true;

            if (!((_startingFocusThumb >= 0) && (_startingFocusThumb <= 16383)))
            {
                retval = false;
            }
            else if (!((_endingFocusThumb >= 0) && (_endingFocusThumb <= 16383)))
            {
                retval = false;
            }
            else if (_focusDuration.Ticks < 0)
            {
                retval = false;
            }

            return retval;
        }
      
        public override string ToString()
        {
            return StartingFocusDistance + ", " + EndingFocusDistance + ", " + FocusDuration.TotalSeconds.ToString() + "s";
        }

        public int StartingFocusThumb
        {
            get
            {
                return _startingFocusThumb;
            }
            set
            {
                _startingFocusThumb = value;
            }
        }

        public int StartingIrisThumb
        {
            get
            {
                return _startingIrisThumb;
            }
            set
            {
                _startingIrisThumb = value;
            }
        }

        public int EndingIrisThumb
        {
            get
            {
                return _endingIrisThumb;
            }
            set
            {
                _endingIrisThumb = value;
            }
        }

        public int EndingFocusThumb
        {
            get
            {
                return _endingFocusThumb;
            }
            set
            {
                _endingFocusThumb = value;
            }
        }

        public string StartingFocusDistance
        {
            get
            {
                return _startingFocusDistance;
            }
            set
            {
                _startingFocusDistance = value;
            }
        }

        public string EndingFocusDistance
        {
            get
            {
                return _endingFocusDistance;
            }
            set
            {
                _endingFocusDistance = value;
            }
        }

        public string StartingFstop
        {
            get
            {
                return _startingFstop;
            }
            set
            {
                _startingFstop = value;
            }
        }

        public string EndingFstop
        {
            get
            {
                return _endingFstop;
            }
            set
            {
                _endingFstop = value;
            }
        }

        public TimeSpan FocusDuration
        {
            get
            {
                return _focusDuration;
            }
            set
            {
                _focusDuration = value;
            }
        }

        public string FocusDurationString
        {
            get
            {
                return _focusDuration.ToString();
            }
            set
            {
                _focusDuration = TimeSpan.Parse(value);
            }
        }
    }
}
