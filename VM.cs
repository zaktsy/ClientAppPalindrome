using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace ClientApp
{
    internal class VM: INotifyPropertyChanged
    {
        const int port = 7000;

        const string ipAddress = "127.0.0.1";

        //коллекция для представления данных в listBox
        private ObservableCollection<Palindrome> palindromes;
        public ObservableCollection<Palindrome> Palindromes { get { return palindromes; } set { palindromes = value; OnPropertyChanged("Palindromes"); } }

        //коллекция для хранения запросов, которые необходимо обработать
        private ObservableCollection<Palindrome> queuePalindromes;
        public ObservableCollection<Palindrome> QueuePalindromes { get { return queuePalindromes; } set { queuePalindromes = value; OnPropertyChanged("QueuePalindromes"); } }

        //путь до папки с вхлдными данными
        private string folderPath = String.Empty;
        public string FolderPath { get { return folderPath; } set { folderPath = value; OnPropertyChanged("FolderPath"); } }


        public VM()
        {
            Palindromes = new ObservableCollection<Palindrome>();
            QueuePalindromes = new ObservableCollection<Palindrome>();

        }

        //метод для отправки запроса
        void CheckPalindrome(object? obj)
        {
            bool Checked = false;
            Palindrome UserPalindrome = (Palindrome)obj;
            int index = Palindromes.IndexOf(UserPalindrome); 
            //запрос отправляется, пока не будет обработан
            while (!Checked)
            {
                TcpClient client = null;
                try
                {
                    
                    Palindromes[index].Status = "Ожидание...";
                    client = new TcpClient(ipAddress, port);
                    NetworkStream stream = client.GetStream();

                    //отправка запроса
                    byte[] request = Encoding.Unicode.GetBytes(UserPalindrome.Text);
                    stream.Write(request, 0, request.Length);

                    //получения результата
                    byte[] responce = new byte[2];
                    stream.Read(responce, 0, responce.Length);
                    string resp = Encoding.Unicode.GetString(responce);

                    
                    // 0 - не палиндром, 1 - палиндром, 2 - сервер занят
                    //каждому запросу устанавливается соотвествующий статус 
                    if (resp == "0") 
                    { 
                        Checked = true;
                        Palindromes[index].Status = "No";
                        stream.Close();
                        client.Close();
                    }
                    else if(resp == "1")
                    {
                        Checked = true;
                        Palindromes[index].Status = "Yes";
                        stream.Close();
                        client.Close();
                    }
                    else if (resp == "2")
                    {
                        Palindromes[index].Status = "В очереди";
                        Thread.Sleep(1000);
                    }

                }
                catch (Exception ex)
                {
                }
            }
        }


        //Реализация команды для открытия папки с входными данными
        private MyCommand openFolderCommand;
        public MyCommand OpenFolderCommand
        {
            get
            {
                return openFolderCommand ??
                    (openFolderCommand = new MyCommand(async obj =>
                    {
                        //CommonOpenFileDialog позволяет в качестве результата принять папку
                        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                        dialog.IsFolderPicker = true;
                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            FolderPath = dialog.FileName;

                            string[] files = Directory.GetFiles(FolderPath);

                            //на основе всех файлов создаются палиндромы-запросы, предполагается, что все файлы корректные (соотвествуют входным данным)
                            foreach(string file in files)
                            {
                                using (FileStream fstream = new FileStream(file, FileMode.Open))
                                {
                                    byte[] buffer = new byte[fstream.Length];
                                    await fstream.ReadAsync(buffer, 0, buffer.Length);
                                    string text = Encoding.Default.GetString(buffer);

                                    var pal = new Palindrome(text, "Ожидание");
                                    Palindromes.Add(pal);
                                    QueuePalindromes.Add(pal);
                                }
                            }
                            //каждый необработанный запрос в очереди обрабатывается
                            foreach(var pal in QueuePalindromes)
                            {
                                Thread th = new Thread(CheckPalindrome);
                                th.Start(pal);
                            }
                            QueuePalindromes = new ObservableCollection<Palindrome>();
                            FolderPath = String.Empty;
                        }
                    },
                    (obj) => FolderPath == String.Empty));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
