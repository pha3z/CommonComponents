using NodaTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    //Comment out this class if you don't want to rely on NodaTime
    public static class Now
    {
        public static LocalDateTime Local()
        {
            return LocalDateTime.FromDateTime(DateTime.Now);
        }
    }
}
