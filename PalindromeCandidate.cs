using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ClientApp
{
    //класс палиндрома релизует интерфейс INotifyPropertyChanged для обновления информации о статусе запроса
    public class PalindromeСandidate : INotifyPropertyChanged
    {
        public string Text { get; set; }

        private string status;

        public string Status 
        { 
            get {  return status;} 
            set 
            { 
                status = value; 
                OnPropertyChanged("Status"); 
            } 
        }

        public PalindromeСandidate() { }

        public PalindromeСandidate(string text, string status)
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
