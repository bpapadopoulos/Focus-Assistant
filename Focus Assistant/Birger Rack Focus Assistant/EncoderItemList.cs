using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Birger_Rack_Focus_Assistant
{
    [Serializable]
    public class EncoderItemList
    {
        private int smallDelay = 2000;
        private int medDelay = 2000;
        private int lrgDelay = 2000;

        private double lensPlay95 = 0;
        private double lensPlaySevenEigths = 0;
        private double lensPlayThreeFourths = 0;
        private double lensPlayMid = 0;
        private double lensPlayQuarter = 0;
        private double lensPlayOneEighth = 0;

        private double lensPlay5ToOneEighth = 0;
        private double lensPlay5ToQuarter = 0;
        private double lensPlay5ToMid = 0;
        private double lensPlay5ToThreeFourths = 0;
        private double lensPlay5ToSevenEigths = 0;
        private double lensPlay5To95 = 0;

        private double lensPlayOneEighthToQuarter = 0;
        private double lensPlayOneEighthToMid = 0;
        private double lensPlayOneEighthToThreeFourths = 0;
        private double lensPlayOneEighthToSevenEigths = 0;
        private double lensPlayOneEighthTo95 = 0;

        private double lensPlayQuarterToMid = 0;
        private double lensPlayQuarterToThreeFourths = 0;
        private double lensPlayQuarterToSevenEigths = 0;
        private double lensPlayQuarterTo95 = 0;

        private double lensPlayMidToThreeFourths = 0;
        private double lensPlayMidToSevenEigths = 0;
        private double lensPlayMidTo95 = 0;

        private double lensPlayThreeFourthsToSevenEigths = 0;
        private double lensPlayThreeFourthsTo95 = 0;

        private double lensPlaySevenEigthsTo95 = 0;

        private double lensPlayCompensationRatio = 0;

        public EncoderItemList()
        {
            this.EncoderItems = new List<EncoderItem>();
        }

        public int SmallDelay
        {
            get
            {
                return smallDelay;
            }

            set
            {
                smallDelay = value;
            }
        }

        public int MedDelay
        {
            get
            {
                return medDelay;
            }

            set
            {
                medDelay = value;
            }
        }

        public int LrgDelay
        {
            get
            {
                return lrgDelay;
            }

            set
            {
                lrgDelay = value;
            }
        }

        public double LensPlayCompensationRatio
        {
            get
            {
                return lensPlayCompensationRatio;
            }

            set
            {
                lensPlayCompensationRatio = value;
            }
        }

        public double LensPlayOneEighth
        {
            get
            {
                return lensPlayOneEighth;
            }

            set
            {
                lensPlayOneEighth = lensPlay5ToOneEighth;
            }
        }

        public double LensPlay5ToOneEighth
        {
            get
            {
                return lensPlay5ToOneEighth;
            }
            set
            {
                lensPlay5ToOneEighth = value;
            }
        }

        public double LensPlay5ToQuarter
        {
            get
            {
                return lensPlay5ToQuarter;
            }
            set
            {
                lensPlay5ToQuarter = value;
            }
        }

        public double LensPlay5ToMid
        {
            get
            {
                return lensPlay5ToMid;
            }
            set
            {
                lensPlay5ToMid = value;
            }
        }

        public double LensPlay5ToThreeFourths
        {
            get
            {
                return lensPlay5ToThreeFourths;
            }
            set
            {
                lensPlay5ToThreeFourths = value;
            }
        }

        public double LensPlay5ToSevenEigths
        {
            get
            {
                return lensPlay5ToSevenEigths;
            }
            set
            {
                lensPlay5ToSevenEigths = value;
            }
        }

        public double LensPlay5To95
        {
            get
            {
                return lensPlay5To95;
            }
            set
            {
                lensPlay5To95 = value;
            }
        }

        public double LensPlayQuarter
        {
            get
            {
                return lensPlayQuarter;
            }

            set
            {
                lensPlayQuarter = lensPlayOneEighthToQuarter;
            }
        }

        public double LensPlayOneEighthToQuarter
        {
            get
            {
                return lensPlayOneEighthToQuarter;
            }

            set
            {
                lensPlayOneEighthToQuarter = value;
            }
        }

        public double LensPlayOneEighthToMid
        {
            get
            {
                return lensPlayOneEighthToMid;
            }

            set
            {
                lensPlayOneEighthToMid = value;
            }
        }

        public double LensPlayOneEighthToThreeFourths
        {
            get
            {
                return lensPlayOneEighthToThreeFourths;
            }

            set
            {
                lensPlayOneEighthToThreeFourths = value;
            }
        }

        public double LensPlayOneEighthToSevenEigths
        {
            get
            {
                return lensPlayOneEighthToSevenEigths;
            }

            set
            {
                lensPlayOneEighthToSevenEigths = value;
            }
        }

        public double LensPlayOneEighthTo95
        {
            get
            {
                return lensPlayOneEighthTo95;
            }

            set
            {
                lensPlayOneEighthTo95 = value;
            }
        }

        public double LensPlayMid
        {
            get
            {
                return lensPlayMid;
            }

            set
            {
                lensPlayMid = lensPlayQuarterToMid;
            }
        }

        public double LensPlayQuarterToMid
        {
            get
            {
                return lensPlayQuarterToMid;
            }

            set
            {
                lensPlayQuarterToMid = value;
            }
        }

        public double LensPlayQuarterToThreeFourths
        {
            get
            {
                return lensPlayQuarterToThreeFourths;
            }

            set
            {
                lensPlayQuarterToThreeFourths = value;
            }
        }

        public double LensPlayQuarterToSevenEigths
        {
            get
            {
                return lensPlayQuarterToSevenEigths;
            }

            set
            {
                lensPlayQuarterToSevenEigths = value;
            }
        }

        public double LensPlayQuarterTo95
        {
            get
            {
                return lensPlayQuarterTo95;
            }

            set
            {
                lensPlayQuarterTo95 = value;
            }
        }

        public double LensPlayThreeFourths
        {
            get
            {
                return lensPlayThreeFourths;
            }

            set
            {
                lensPlayThreeFourths = lensPlayMidToThreeFourths;
            }
        }

        public double LensPlayMidToThreeFourths
        {
            get
            {
                return lensPlayMidToThreeFourths;
            }

            set
            {
                lensPlayMidToThreeFourths = value;
            }
        }

        public double LensPlayMidToSevenEigths
        {
            get
            {
                return lensPlayMidToSevenEigths;
            }

            set
            {
                lensPlayMidToSevenEigths = value;
            }
        }

        public double LensPlayMidTo95
        {
            get
            {
                return lensPlayMidTo95;
            }

            set
            {
                lensPlayMidTo95 = value;
            }
        }

        public double LensPlaySevenEigths
        {
            get
            {
                return lensPlaySevenEigths;
            }

            set
            {
                lensPlaySevenEigths = lensPlayThreeFourthsToSevenEigths;
            }
        }

        public double LensPlayThreeFourthsToSevenEigths
        {
            get
            {
                return lensPlayThreeFourthsToSevenEigths;
            }

            set
            {
                lensPlayThreeFourthsToSevenEigths = value;
            }
        }

        public double LensPlayThreeFourthsTo95
        {
            get
            {
                return lensPlayThreeFourthsTo95;
            }

            set
            {
                lensPlayThreeFourthsTo95 = value;
            }
        }

        public double LensPlay95
        {
            get
            {
                return lensPlay95;
            }
            set
            {
                lensPlay95 = lensPlaySevenEigthsTo95;
            }
        }

        public double LensPlaySevenEigthsTo95
        {
            get
            {
                return lensPlaySevenEigthsTo95;
            }

            set
            {
                lensPlaySevenEigthsTo95 = value;
            }
        }

        public List<EncoderItem> EncoderItems
        {
            get;

            set;
        }
    }
}
