namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;

    public class StructureCharacterViewModel : StructureBaseViewModel<StructureCharacterModel>
    {
        private InventoryEditorViewModel _inventory;
        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;

        #region ctor

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
        }

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            _dialogService = dialogService;
            _colorDialogFactory = colorDialogFactory;

            if (dataModel.Inventory == null)
                return;
            Inventory = new InventoryEditorViewModel(this, dataModel.Inventory);

            DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand ResetVelocityCommand
        {
            get { return new DelegateCommand(ResetVelocityExecuted, ResetVelocityCanExecute); }
        }

        public ICommand ReverseVelocityCommand
        {
            get { return new DelegateCommand(ReverseVelocityExecuted, ReverseVelocityCanExecute); }
        }

        public ICommand ChangeColorCommand
        {
            get { return new DelegateCommand(ChangeColorExecuted, ChangeColorCanExecute); }
        }

        #endregion

        #region Properties

        protected new StructureCharacterModel DataModel
        {
            get { return base.DataModel as StructureCharacterModel; }
        }

        public bool IsPilot
        {
            get { return DataModel.IsPilot; }
            set { DataModel.IsPilot = value; }
        }

        public bool IsPlayer
        {
            get { return DataModel.IsPlayer; }
            set { DataModel.IsPlayer = value; }
        }

        public System.Windows.Media.Brush Color
        {
            get { return new System.Windows.Media.SolidColorBrush(DataModel.Color.FromHsvMaskToPaletteMediaColor()); }

            set
            {
                DataModel.Color = ((System.Windows.Media.SolidColorBrush)value).Color.FromPaletteColorToHsvMask();
                MainViewModel.IsModified = true;
            }
        }

        public bool Light
        {
            get { return DataModel.Light; }

            set
            {
                DataModel.Light = value;
                MainViewModel.IsModified = true;
            }
        }

        public bool JetPack
        {
            get { return DataModel.JetPack; }

            set
            {
                DataModel.JetPack = value;
                MainViewModel.IsModified = true;
            }
        }

        public bool Dampeners
        {
            get { return DataModel.Dampeners; }

            set
            {
                DataModel.Dampeners = value;
                MainViewModel.IsModified = true;
            }
        }

        public override double LinearVelocity
        {
            get { return DataModel.LinearVelocity; }
        }

        public float BatteryCapacity
        {
            get { return DataModel.BatteryCapacity * 100000; }
            set { 
                DataModel.BatteryCapacity = value / 100000;
                //MainViewModel.IsModified = true; //TODO : Is this needed?
            }
        }

        public float? Health
        {
            get { return DataModel.Health; }
            set { DataModel.Health = value; }
        }

        public float OxygenLevel
        {
            get { return DataModel.OxygenLevel; }
            set { DataModel.OxygenLevel = value; }
        }

        public float HydrogenLevel
        {
            get { return DataModel.HydrogenLevel; }
            set { DataModel.HydrogenLevel = value; }
        }

        public InventoryEditorViewModel Inventory
        {
            get { return _inventory; }

            set
            {
                if (value != _inventory)
                {
                    _inventory = value;
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        #endregion

        #region methods

        public bool ResetVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f;
        }

        public void ResetVelocityExecuted()
        {
            DataModel.ResetVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            DataModel.ReverseVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ChangeColorCanExecute()
        {
            return true;
        }

        public void ChangeColorExecuted()
        {
            var colorDialog = _colorDialogFactory();
            colorDialog.FullOpen = true;
            colorDialog.BrushColor = Color as System.Windows.Media.SolidColorBrush;
            colorDialog.CustomColors = MainViewModel.CreativeModeColors;

            var result = _dialogService.ShowColorDialog(OwnerViewModel, colorDialog);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Color = colorDialog.BrushColor;
                MainViewModel.IsModified = true;
            }

            MainViewModel.CreativeModeColors = colorDialog.CustomColors;
        }

        #endregion
    }
}
