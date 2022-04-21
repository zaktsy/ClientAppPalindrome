using ClientApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;

namespace ClientApp
{
    internal class VM: INotifyPropertyChanged
    {
        //коллекция для представления данных в listBox
        private ObservableCollection<PalindromeСandidate> palindromesCandidates;
        public ObservableCollection<PalindromeСandidate> PalindromeCandidates 
        { 
            get { return palindromesCandidates; } 
            set 
            { 
                palindromesCandidates = value; 
                OnPropertyChanged("PalindromeCandidates"); 
            } 
        }


        //путь до папки с входными данными
        private string folderPath = String.Empty;
        public string FolderPath 
        { 
            get { return folderPath; } 
            set 
            { 
                folderPath = value; 
                OnPropertyChanged("FolderPath"); 
            } 
        }


        public VM()
        {
            PalindromeCandidates = new ObservableCollection<PalindromeСandidate>();

        }


        private MyCommand checkPalindromeCommand;
        public MyCommand CheckPalindromeCommand
        {
            get
            {
                return checkPalindromeCommand ??
                    (checkPalindromeCommand = new MyCommand(async obj =>
                    {
                        foreach (var pc in PalindromeCandidates)
                        {
                            PalindromeChecker checker = new PalindromeChecker();
                            Task t = Task.Run(async () =>
                            {
                                await checker.CheckPalindromeCandidate(pc);
                            });
                        }
                    },
                    (obj) => PalindromeCandidates.Count != 0));
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
                            foreach (string file in files)
                            {
                                using (FileStream fstream = new FileStream(file, FileMode.Open))
                                {
                                    byte[] buffer = new byte[fstream.Length];
                                    await fstream.ReadAsync(buffer, 0, buffer.Length);
                                    string text = Encoding.Default.GetString(buffer);

                                    var pal = new PalindromeСandidate(text, Status.Processing.ToString());
                                    PalindromeCandidates.Add(pal);
                                }
                            }
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
