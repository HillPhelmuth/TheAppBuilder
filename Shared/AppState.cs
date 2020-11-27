using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Shared
{
    public class AppState : INotifyPropertyChanged
    {
        private string _codeSnippet;
        private string _language;
        private List<ProjectFile> _projectFiles;
        private List<MetadataReference> _references;

        public string CodeSnippet
        {
            get => _codeSnippet;
            set { _codeSnippet = value; OnPropertyChanged(); }
        }

        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }

        public List<MetadataReference> References
        {
            get => _references;
            set { _references = value; OnPropertyChanged(); }
        }

        public List<ProjectFile> ProjectFiles
        {
            get => _projectFiles;
            set { _projectFiles = value; OnPropertyChanged();}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
