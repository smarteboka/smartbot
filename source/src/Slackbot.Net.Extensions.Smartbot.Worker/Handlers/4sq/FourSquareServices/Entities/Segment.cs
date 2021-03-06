﻿namespace Smartbot.Utilities.Handlers._4sq.FourSquareServices.Entities
{
    public class Segment : FourSquareEntity
    {
        /// <summary>
        /// The name of the named segment.
        /// </summary>
        public string lable
        {
            get;
            set;
        }

        /// <summary>
        /// The time as HHMM (24hr) at which the segment begins.
        /// </summary>
        public string start
        {
            get;
            set;
        }

        /// <summary>
        /// The time as HHMM (24hr) at which the segment ends.
        /// </summary>
        public string end
        {
            get;
            set;
        }
    }
}