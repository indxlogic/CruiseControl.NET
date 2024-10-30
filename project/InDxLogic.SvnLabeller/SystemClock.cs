using System;

namespace InDxLogic.SvnLabeller
{
    public interface ISystemClock
    {
        DateTime Now { get; }
        DateTime Today { get; }
    }

    public class SystemClock : ISystemClock
    {
        /// <summary>
        /// Gets the current date and time from the system clock.
        /// </summary>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// Gets the current date from the system clock.
        /// </summary>
        public DateTime Today
        {
            get { return DateTime.Today; }
        }
    }
}
