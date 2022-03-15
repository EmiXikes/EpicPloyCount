using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicPloyCount.Model
{
    public class PolyCountModel :INPC
    {
        private double staticMeasurement;
        private double activeMeasurement;

        public double StaticMeasurement
        {
            get => staticMeasurement; set
            {
                staticMeasurement = value;
                MyPropertyChanged(nameof(StaticMeasurement));
            }
        }
        public double ActiveMeasurement
        {
            get => activeMeasurement; set
            {
                activeMeasurement = value;
                MyPropertyChanged(nameof(ActiveMeasurement));
            }
        }
    }

    
}
