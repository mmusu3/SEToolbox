namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System.Collections.Generic;

    public class StructureSafeZoneViewModel : StructureBaseViewModel<StructureSafeZoneModel>
    {
        #region ctor

        public StructureSafeZoneViewModel(BaseViewModel parentViewModel, StructureSafeZoneModel dataModel)
            : base(parentViewModel, dataModel)
        {
            // Optional: Hook für PropertyChanged → automatisch weiterreichen
            DataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        protected new StructureSafeZoneModel DataModel => base.DataModel as StructureSafeZoneModel;


        public bool IsVisible                       //Done
        {
            get { return DataModel.IsVisible; }

            set
            {
                DataModel.IsVisible = value;
                MainViewModel.IsModified = true;
            }
        }

        public bool IsEnabled                       //Done
        {
            get { return DataModel.IsEnabled; }

            set
            {
                DataModel.IsEnabled = value;
                MainViewModel.IsModified = true;
            }
        }

        public float Radius                         //Done
        {
            get { return DataModel.Radius; }
            set { 
                DataModel.Radius = value;
                MainViewModel.IsModified = true;
            }
        }
        public List<string> AvailableShapes { get; } = new() { "Sphere", "Box" };
        public string Shape                         //TODO Dropdown: Sphere/Box
        {
            get { 
                return DataModel.Shape; }
            set
            {
                DataModel.Shape = value;
                MainViewModel.IsModified = true;
            }
        }

        public List<string> AvailableTextures { get; } = new() { "Sphere" }; // TODO
        public string Texture => DataModel.Texture; //Todo enum


        public List<string> AccessTypes { get; } = new List<string> { "Whitelist", "Blacklist" };

        public string AccessTypePlayers             //TODO
        {
            get { return DataModel.AccessTypePlayers; }
            set
            {
                DataModel.AccessTypePlayers = value;
                MainViewModel.IsModified = true;
            }
        }

        public string AccessTypeFactions            //TODO
        {
            get { return DataModel.AccessTypeFactions; }
            set
            {
                DataModel.AccessTypeFactions = value;
                MainViewModel.IsModified = true;
            }
        }

        public string AccessTypeGrids               //TODO
        {
            get { return DataModel.AccessTypeGrids; }
            set
            {
                DataModel.AccessTypeGrids = value;
                MainViewModel.IsModified = true;
            }
        }

        public string AccessTypeFloatingObjects     //TODO
        {
            get { return DataModel.AccessTypeFloatingObjects; }
            set
            {
                DataModel.AccessTypeFloatingObjects = value;
                MainViewModel.IsModified = true;
            }
        }

        public float SizeX => DataModel.Size.X;
        public float SizeY => DataModel.Size.Y;
        public float SizeZ => DataModel.Size.Z;

        public float ColorR => DataModel.ModelColor.X;
        public float ColorG => DataModel.ModelColor.Y;
        public float ColorB => DataModel.ModelColor.Z;

        #endregion
    }
}
