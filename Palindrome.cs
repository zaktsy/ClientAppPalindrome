using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    //класс палиндрома релизует интерфейс INotifyPropertyChanged для обновления информации о статусе запроса
    public class Palindrome: INotifyPropertyChanged
    {

        public string Text { get; set; }

        private string status;
        public string Status { get { return status; } set { status = value; OnPropertyChanged("Status"); } }

        public Palindrome() { }

        public Palindrome(string text, string status)
        {
            Text = text;
            Status = status;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
