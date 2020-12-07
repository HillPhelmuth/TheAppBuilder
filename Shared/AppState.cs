using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace AppBuilder.Shared
{
    public class AppState : INotifyPropertyChanged
    {
        private string _codeSnippet;
        private string _language;
        private List<ProjectFile> _projectFiles;
        private List<MetadataReference> _references;
        private string _currentOutput;
        private ProjectFile _activeProjectFile;
        private UserProject _activeProject;

        public string CodeSnippet
        {
            get => _codeSnippet;
            set { _codeSnippet = value; OnPropertyChanged(); }
        }

        public string Language
        {
            get
            {
                if (ActiveProject == null) return _language;
                if (ActiveProjectFile.Name.EndsWith(".cs"))
                    return "csharp";
                else if (ActiveProjectFile.Name.EndsWith(".razor"))
                    return "razor";
                else
                    return _language;
            }
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
                ProjectFiles.Add(projectFile);
                OnPropertyChanged(nameof(ProjectFiles));
                return;
            }

            foreach (var file in ProjectFiles.Where(file => file.Name == projectFile.Name))
            {
                file.Content = projectFile.Content;
            }
        }
        public void CreateProjectFile(string filename, string fileContent, FileType fileType)
        {
            Language = fileType == FileType.Class ? "csharp" : "razor";
            CodeSnippet = fileContent;
            ActiveProjectFile = new ProjectFile { Name = filename, Content = fileContent, FileType = fileType };
        }
        public void ChangeFileName(string filename)
        {
            ActiveProjectFile.Name = filename;
            OnPropertyChanged(nameof(ActiveProjectFile));
        }
        public bool HasActiveProject => ActiveProject != null || _activeProject != null;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
