using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerGasTips
{
    class ResponseJson
    {
        public Analytics analytics { get; set; }
        public StationHours stationHours { get; set; }
    }
    public class Analytics
    {
        public int criteo { get; set; }
        public string ga { get; set; }
        public string localytics { get; set; }
    }

    public class StationHours
    {
        public bool isFetching { get; set; }
        public string byStationId { get; set; }
        public string stationByLocation { get; set; }
    }

    public class Features
    {
        public bool isFetching { get; set; }
        public int MyProperty { get; set; }
    }
}
