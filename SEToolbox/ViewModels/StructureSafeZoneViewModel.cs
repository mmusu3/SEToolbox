namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;

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

        public bool IsVisible
        {
            get => DataModel.IsVisible;
            set => DataModel.IsVisible = value;
        }

        public bool IsEnabled
        {
            get => DataModel.IsEnabled;
            set => DataModel.IsEnabled = value;
        }

        public float Radius
        {
            get => DataModel.Radius;
            set => DataModel.Radius = value;
        }

        public string Shape
        {
            get => DataModel.Shape;
            set => DataModel.Shape = value;
        }

        public string Texture => DataModel.Texture;

        public string AccessTypePlayers
        {
            get => DataModel.AccessTypePlayers;
            set => DataModel.AccessTypePlayers = value;
        }

        public string AccessTypeFactions
        {
            get => DataModel.AccessTypeFactions;
            set => DataModel.AccessTypeFactions = value;
        }

        public string AccessTypeGrids
        {
            get => DataModel.AccessTypeGrids;
            set => DataModel.AccessTypeGrids = value;
        }

        public string AccessTypeFloatingObjects
        {
            get => DataModel.AccessTypeFloatingObjects;
            set => DataModel.AccessTypeFloatingObjects = value;
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
