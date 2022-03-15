
using System.ComponentModel;

namespace EpicPloyCount.Model
{
    public class INPC : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void MyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
