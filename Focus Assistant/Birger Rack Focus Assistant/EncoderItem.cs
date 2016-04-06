using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization; 

namespace Birger_Rack_Focus_Assistant
{
    [Serializable]
    public class EncoderItem
    {
        public double Dist;
        public double DistStep;
        public double Near;
        public double Far;
        public int Encoder;
        public double TrackBar;
        public double MediumRatio;
        public double LargeRatio;
        public int TrackBarInt
        {
            get
            {
                return (int)Math.Round(TrackBar);
            }
        }

        public EncoderItem()
        {

        }

        public EncoderItem(double dist, double distStep, double near, double far, int encoder, double trackBar, double mediumRatio, double largeRatio)
        {
            Dist = dist;
            DistStep = distStep;
            Near = near;
            Far = far;
            Encoder = encoder;
            TrackBar = trackBar;
            MediumRatio = mediumRatio;
            LargeRatio = largeRatio;
        }
    }
}
