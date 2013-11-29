﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interop;

    public class StructureBaseModel : BaseModel, IStructureBase
    {
        #region fields

        private MyObjectBuilder_EntityBase entityBase;
        private ClassType classType;
        private string description;

        #endregion

        #region ctor

        public StructureBaseModel()
        {
        }

        public StructureBaseModel(MyObjectBuilder_EntityBase entityBase)
        {
            this.EntityBase = entityBase;
        }

        #endregion

        #region Properties

        public virtual MyObjectBuilder_EntityBase EntityBase
        {
            get
            {
                return this.entityBase;
            }
            set
            {
                if (value != this.entityBase)
                {
                    this.entityBase = value;
                    this.UpdateFromEntityBase();
                    this.RaisePropertyChanged(() => EntityBase);
                }
            }
        }

        public long EntityId
        {
            get
            {
                return this.entityBase.EntityId;
            }

            set
            {
                if (value != this.entityBase.EntityId)
                {
                    this.entityBase.EntityId = value;
                    this.RaisePropertyChanged(() => EntityId);
                }
            }
        }

        public MyPositionAndOrientation? PositionAndOrientation
        {
            get
            {
                return this.entityBase.PositionAndOrientation;
            }

            set
            {
                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, this.entityBase.PositionAndOrientation))
                //if (value != this.entityBase.PositionAndOrientation)
                {
                    this.entityBase.PositionAndOrientation = value;
                    this.RaisePropertyChanged(() => PositionAndOrientation);
                }
            }
        }

        public ClassType ClassType
        {
            get
            {
                return this.classType;
            }

            set
            {
                if (value != this.classType)
                {
                    this.classType = value;
                    this.RaisePropertyChanged(() => ClassType);
                }
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                if (value != this.description)
                {
                    this.description = value;
                    this.RaisePropertyChanged(() => Description);
                }
            }
        }

        #endregion

        #region methods

        public virtual void UpdateFromEntityBase()
        {
            this.ClassType = ClassType.Unknown;
        }

        public static IStructureBase Create(MyObjectBuilder_EntityBase entityBase)
        {
            if (entityBase is MyObjectBuilder_VoxelMap)
            {
                return new StructureVoxelModel(entityBase);
            }
            else if (entityBase is MyObjectBuilder_Character)
            {
                return new StructureCharacterModel(entityBase);
            }
            else if (entityBase is MyObjectBuilder_CubeGrid)
            {
                return new StructureCubeGridModel(entityBase);
            }
            else if (entityBase is MyObjectBuilder_FloatingObject)
            {
                return new StructureFloatingObjectModel(entityBase);
            }
            else
            {
                throw new NotImplementedException(string.Format("A new object has not been catered for in the StructureBase, of type '{0}'.", entityBase.GetType()));
            }
        }

        #endregion
    }
}