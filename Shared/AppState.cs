using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.CodeAnalysis;
using Microsoft.JSInterop;

namespace AppBuilder.Shared
{
    public class AppState : INotifyPropertyChanged
    {
        private string _codeSnippet;
        private string _language;
        private List<ProjectFile> _projectFiles;
        private List<MetadataReference> _assemblyReferences;
        private string _currentOutput;
        private ProjectFile _activeProjectFile;
        private UserProject _activeProject;
        private bool _isOnline;
        private List<UserProject> _userProjects;
        private string _currentUser;
        private bool _isAuthUser;
        private List<string> _assemblyNames;
        private string _themeColor;

        public string ThemeColor
        {
            get => _themeColor;
            set { _themeColor = value; OnPropertyChanged();}
        }

        public string CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public bool IsAuthUser
        {
            get => _isAuthUser;
            set { _isAuthUser = value; OnPropertyChanged(); }
        }
        public List<UserProject> UserProjects
        {
            get => _userProjects;
            set { _userProjects = value; OnPropertyChanged(); }
        }

        public bool IsOnline
        {
            get => _isOnline;
            set { _isOnline = value; OnPropertyChanged(); }
        }

        public string CodeSnippet
        {
            get => ActiveProjectFile?.Content ?? _codeSnippet;
            set { _codeSnippet = value; OnPropertyChanged(); }
        }
        [JsonIgnore]
        public string Language
        {
            get
            {
                if (ActiveProject == null) return _language;
                if (ActiveProject.Name == null) return _language;
                if (ActiveProjectFile.Name.EndsWith(".cs"))
                    return "csharp";
                return ActiveProjectFile.Name.EndsWith(".razor") ? "razor" : _language;
            }
            set { _language = value; OnPropertyChanged(); }
        }
        [JsonIgnore]
        public List<MetadataReference> AssemblyReferences
        {
            get => _assemblyReferences;
            set { _assemblyReferences = value; OnPropertyChanged(); }
        }

        public List<string> AssemblyNames
        {
            get => _assemblyNames;
            set { _assemblyNames = value; OnPropertyChanged(); }
        }

        public List<ProjectFile> ProjectFiles
        {
            get => _projectFiles;
            set { _projectFiles = value; OnPropertyChanged(); }
        }
        public string CurrentOutput
        {
            get => _currentOutput;
            set { _currentOutput = value; OnPropertyChanged(); }
        }

        public ProjectFile ActiveProjectFile
        {
            get => _activeProjectFile;
            set { _activeProjectFile = value; OnPropertyChanged(); }
        }

        public UserProject ActiveProject
        {
            get => _activeProject;
            set { _activeProject = value; OnPropertyChanged(); }
        }

        public void SaveCode(ProjectFile projectFile)
        {
            ProjectFiles ??= new List<ProjectFile>();
            if (ProjectFiles.All(x => x.Name != projectFile.Name))
            {
                ProjectFiles?.Add(projectFile);
                OnPropertyChanged(nameof(ProjectFiles));
                return;
            }
            if (ActiveProject.Files.All(x => x.Name != projectFile.Name))
            {
                ActiveProject?.Files?.Add(projectFile);
            }

            foreach (var file in ProjectFiles.Where(file => file.Name == projectFile.Name))
            {
                file.Content = projectFile.Content;
            }
        }
        public void CreateProjectFile(string filename, string fileContent, FileType fileType)
        {
            Language = fileType == FileType.Class ? "csharp" : "razor";
            //CodeSnippet = fileContent;
            ActiveProjectFile = new ProjectFile { Name = filename, Content = fileContent, FileType = fileType };
            ActiveProject.Files.Add(ActiveProjectFile);
            OnPropertyChanged(nameof(ActiveProject));
        }

        public void CreateProject(UserProject project)
        {

        }
        public void ChangeFileName(string filename)
        {
            ActiveProjectFile.Name = filename;
            OnPropertyChanged(nameof(ActiveProjectFile));
        }

        public void SetStateFromStorage(AppState storedState)
        {
            ActiveProject = storedState.ActiveProject;
            ActiveProjectFile = storedState.ActiveProjectFile;
            CodeSnippet = storedState.CodeSnippet;
            CurrentOutput = storedState.CurrentOutput;
            ProjectFiles = storedState.ProjectFiles;
        }
        public bool HasActiveProject => ActiveProject != null || _activeProject != null;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Console.WriteLine($"{propertyName} Changed");
        }
    }
}
