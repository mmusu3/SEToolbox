﻿namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;

    public class SelectWorldViewModel : BaseViewModel
    {
        #region Fields

        private SelectWorldModel dataModel;
        private bool? closeResult;

        #endregion

        #region Constructors

        public SelectWorldViewModel(BaseViewModel parentViewModel, SelectWorldModel dataModel)
            : base(parentViewModel)
        {
            this.dataModel = dataModel;
            this.dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region Properties

        public ICommand LoadCommand
        {
            get
            {
                return new DelegateCommand(new Action(LoadExecuted), new Func<bool>(LoadCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
            }
        }

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this.dataModel.SelectedWorld;
            }
            set
            {
                if (value != this.dataModel.SelectedWorld)
                {
                    this.dataModel.SelectedWorld = value;
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this.dataModel.Worlds;
            }
        }

        #endregion

        #region methods

        public bool LoadCanExecute()
        {
            return this.SelectedWorld != null && this.SelectedWorld.IsValid;
        }

        public void LoadExecuted()
        {
            this.CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            this.CloseResult = false;
        }

        #endregion
    }
}