namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public class SelectCubeModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComponentItemModel> _cubeList;
        private ComponentItemModel _cubeItem;

        #endregion

        #region ctor

        public SelectCubeModel()
        {
            _cubeList = new ObservableCollection<ComponentItemModel>();
        }

        #endregion

        #region Properties

        public ObservableCollection<ComponentItemModel> CubeList
        {
            get
            {
                return _cubeList;
            }

            set
            {
                if (value != _cubeList)
                {
                    _cubeList = value;
                    OnPropertyChanged(nameof(CubeList));
                }
            }
        }

        public ComponentItemModel CubeItem
        {
            get
            {
                return _cubeItem;
            }

            set
            {
                if (value != _cubeItem)
                {
                    _cubeItem = value;
                    OnPropertyChanged(nameof(CubeItem));
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyCubeSize cubeSize, MyObjectBuilderType typeId, string subTypeId)
        {
            CubeList.Clear();
            var list = new SortedList<string, ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var cubeDefinitions = SpaceEngineersCore.Resources.CubeBlockDefinitions.Where(c => c.CubeSize == cubeSize);

            // Fixes "System.OverflowException: 'TimeSpan overflowed because the duration is too long.'"
            var _Time = cubeDefinitions.First().MaxIntegrity / cubeDefinitions.First().IntegrityPointsPerSec;
            var _TimeSpan = TimeSpan.MinValue;
            try {
                _TimeSpan = TimeSpan.FromSeconds(_Time);
            } catch {
                _TimeSpan = TimeSpan.MaxValue;
            }

            foreach (var cubeDefinition in cubeDefinitions)
            {
                var c = new ComponentItemModel
                {
                    Name = cubeDefinition.DisplayNameText,
                    TypeId = cubeDefinition.Id.TypeId,
                    TypeIdString = cubeDefinition.Id.TypeId.ToString(),
                    SubtypeId = cubeDefinition.Id.SubtypeName,
                    TextureFile = (cubeDefinition.Icons == null || cubeDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icons.First(), Path.Combine(contentPath, cubeDefinition.Icons.First())),
                    Time = _TimeSpan,
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeName),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                };

                list.Add(c.FriendlyName + c.TypeIdString + c.SubtypeId, c);
            }

            foreach (var kvp in list)
            {
                CubeList.Add(kvp.Value);
            }

            CubeItem = CubeList.FirstOrDefault(c => c.TypeId == typeId && c.SubtypeId == subTypeId);
        }

        #endregion
    }
}
