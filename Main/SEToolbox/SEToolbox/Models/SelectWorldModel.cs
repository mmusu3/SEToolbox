﻿namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Collections.ObjectModel;
    using System.IO;

    public class SelectWorldModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string baseSavePath;

        private bool isValidSaveDirectory;

        private SaveResource selectedWorld;

        private ObservableCollection<SaveResource> worlds;

        #endregion

        #region ctor

        public SelectWorldModel()
        {
            this.SelectedWorld = null;
            this.Worlds = new ObservableCollection<SaveResource>();
        }

        #endregion

        #region Properties

        public string BaseSavePath
        {
            get
            {
                return this.baseSavePath;
            }

            set
            {
                if (value != this.baseSavePath)
                {
                    this.baseSavePath = value;
                    this.RaisePropertyChanged(() => BaseSavePath);
                }
            }
        }

        public bool IsValidSaveDirectory
        {
            get
            {
                return this.isValidSaveDirectory;
            }

            set
            {
                if (value != this.isValidSaveDirectory)
                {
                    this.isValidSaveDirectory = value;
                    this.RaisePropertyChanged(() => IsValidSaveDirectory);
                }
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this.selectedWorld;
            }

            set
            {
                if (value != this.selectedWorld)
                {
                    this.selectedWorld = value;
                    this.RaisePropertyChanged(() => SelectedWorld);
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this.worlds;
            }

            set
            {
                if (value != this.worlds)
                {
                    this.worlds = value;
                    this.RaisePropertyChanged(() => Worlds);
                }
            }
        }

        #endregion

        #region methods

        public void Load(string baseSavePath)
        {
            this.BaseSavePath = baseSavePath;
            this.LoadSaveList();
        }

        #endregion

        #region helpers

        private void LoadSaveList()
        {
            this.Worlds.Clear();

            if (Directory.Exists(this.BaseSavePath))
            {
                var userPaths = Directory.GetDirectories(this.BaseSavePath);

                foreach (var userPath in userPaths)
                {
                    var savePaths = Directory.GetDirectories(userPath);

                    foreach (var savePath in savePaths)
                    {
                        SaveResource saveResource;
                        this.Worlds.Add(saveResource = new SaveResource()
                        {
                            Savename = Path.GetFileName(savePath),
                            Username = Path.GetFileName(userPath),
                            Savepath = savePath
                        });

                        saveResource.LoadCheckpoint();
                    }
                }

                this.IsValidSaveDirectory = true;
            }
            else
            {
                this.IsValidSaveDirectory = false;
            }
        }

        #endregion
    }
}