namespace SEToolbox.Models
{
    using Microsoft.VisualBasic.FileIO;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GUI;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using VRage.FileSystem;
    using VRage.Game;
    using Res = SEToolbox.Properties.Resources;

    public class WorldResource : BaseModel
    {
        #region Fields

        private string _groupDescription;
        private SaveWorldType _saveType;
        private string _userName;
        private string _saveName;
        private string _savePath;
        private MyObjectBuilder_Checkpoint _checkpoint;
        private bool _compressedCheckpointFormat;
        private MyObjectBuilder_Sector _sectorData;
        private bool _compressedSectorFormat;
        private readonly SpaceEngineersResources _resources;
        private bool _isValid;
        private Version _version;
        private ulong? _workshopId;
        private string _sessionName;
        private DateTime _lastSaveTime;

        #endregion

        public WorldResource()
        {
            _resources = new SpaceEngineersResources();
        }

        #region Properties

        public string GroupDescription
        {
            get { return _groupDescription; }

            set
            {
                if (value != _groupDescription)
                {
                    _groupDescription = value;
                    OnPropertyChanged(nameof(GroupDescription));
                }
            }
        }

        public SaveWorldType SaveType
        {
            get { return _saveType; }

            set
            {
                if (value != _saveType)
                {
                    _saveType = value;
                    OnPropertyChanged(nameof(SaveType));
                }
            }
        }

        /// <summary>
        /// This will be the SteamId of the local user, or the Instance name of the Server.
        /// </summary>
        public string UserName
        {
            get { return _userName; }

            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string Savename
        {
            get { return _saveName; }

            set
            {
                if (value != _saveName)
                {
                    _saveName = value;
                    OnPropertyChanged(nameof(Savename));
                }
            }
        }

        public string Savepath
        {
            get { return _savePath; }

            set
            {
                if (value != _savePath)
                {
                    _savePath = value;
                    OnPropertyChanged(nameof(Savepath));
                }
            }
        }

        public UserDataPath DataPath { get; set; }

        public MyObjectBuilder_Checkpoint Checkpoint
        {
            get { return _checkpoint; }

            set
            {
                if (value != _checkpoint)
                {
                    _checkpoint = value;

                    _isValid = _checkpoint != null;

                    if (_checkpoint == null)
                        _version = new Version();
                    else
                    {
                        var str = _checkpoint.AppVersion.ToString(CultureInfo.InvariantCulture);
                        if (str == "0")
                            _version = new Version();
                        else
                        {
                            try
                            {
                                str = str.Substring(0, str.Length - 6) + "." + str.Substring(str.Length - 6, 3) + "." + str.Substring(str.Length - 3);
                                _version = new Version(str);
                            }
                            catch
                            {
                                _version = new Version();
                            }
                        }
                    }

                    WorkshopId = _checkpoint?.WorkshopId;

                    _sessionName = _checkpoint != null ? _checkpoint.SessionName : Res.ErrorInvalidSaveLabel;

                    _lastSaveTime = _checkpoint?.LastSaveTime ?? DateTime.MinValue;

                    OnPropertyChanged(nameof(Checkpoint), nameof(SessionName), nameof(LastSaveTime), nameof(IsValid));
                }
            }
        }

        public string SessionName
        {
            get { return _sessionName; }

            set
            {
                if (value != _sessionName)
                {
                    _sessionName = value;
                    OnPropertyChanged(nameof(SessionName));
                }
            }
        }

        public DateTime LastSaveTime
        {
            get { return _lastSaveTime; }

            set
            {
                if (value != _lastSaveTime)
                {
                    _lastSaveTime = value;
                    OnPropertyChanged(nameof(LastSaveTime));
                }
            }
        }

        public Version Version
        {
            get { return _version; }

            set
            {
                if (value != _version)
                {
                    _version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        public bool IsWorkshopItem => _workshopId.HasValue;

        public ulong? WorkshopId
        {
            get { return _workshopId; }

            set
            {
                if (value != _workshopId)
                {
                    _workshopId = value;
                    OnPropertyChanged(nameof(WorkshopId));
                }
            }
        }

        public bool IsValid
        {
            get { return _isValid; }

            set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string ThumbnailImageFilename => Path.Combine(_savePath, SpaceEngineersConsts.ThumbnailImageFilename);

        public override string ToString()
        {
            return SessionName;
        }

        public MyObjectBuilder_Sector SectorData
        {
            get { return _sectorData; }

            set
            {
                if (value != _sectorData)
                {
                    _sectorData = value;
                    OnPropertyChanged(nameof(SectorData));
                }
            }
        }

        public SpaceEngineersResources Resources
        {
            get { return _resources; }
        }

        #endregion

        #region methods

        #region Load and Save

        /// <summary>
        /// Loads checkpoint file.
        /// </summary>
        public bool LoadCheckpoint(out string errorInformation, bool snapshot = false)
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            MyObjectBuilder_Checkpoint checkpoint;
            bool retVal = SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Checkpoint>(filename, out checkpoint, out _compressedCheckpointFormat, out errorInformation, snapshot);
            Checkpoint = checkpoint;
            return retVal;
        }

        public void LoadDefinitionsAndMods()
        {
            if (_resources == null || Checkpoint == null || Checkpoint.Mods == null)
                return;

            List<MyObjectBuilder_Checkpoint.ModItem> mods = [.. Checkpoint.Mods];
            var result = SpaceEngineersWorkshop.DownloadWorldModsBlocking(mods, cancelToken: null);

            if (result.Result == 0)
                mods.Clear();

            _resources.LoadDefinitionsAndMods(DataPath.ModsPath, mods);
        }

        public bool LoadSector(out string errorInformation, bool snapshot = false)
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            MyObjectBuilder_Sector sectorData;

            bool retVal = SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Sector>(filename, out sectorData, out _compressedSectorFormat, out errorInformation, snapshot);
            SectorData = sectorData;
            return retVal;
        }

        public void SaveCheckPoint(bool backupFile)
        {
            var checkpointFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            if (backupFile)
            {
                var checkpointBackupFilename = checkpointFilename + ".bak";

                if (File.Exists(checkpointBackupFilename))
                {
                    FileSystem.DeleteFile(checkpointBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                File.Move(checkpointFilename, checkpointBackupFilename);
            }

            if (_compressedCheckpointFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile(Checkpoint, tempFilename);
                ZipTools.GZipCompress(tempFilename, checkpointFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile(Checkpoint, checkpointFilename);
            }
        }

        public void SaveSector(bool backupFile)
        {
            var sectorFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

            if (backupFile)
            {
                // xml sector file.  (it may or may not be compressed)
                var sectorBackupFilename = sectorFilename + ".bak";

                if (File.Exists(sectorBackupFilename))
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                File.Move(sectorFilename, sectorBackupFilename);

                // binary sector file. (it may or may not be compressed)
                sectorBackupFilename = sectorFilename + SpaceEngineersConsts.ProtobuffersExtension + ".bak";

                if (File.Exists(sectorBackupFilename))
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                // The protoBuf (.sbsPB, .sbsB1) may not exist in older save games.
                if (File.Exists(sectorFilename + SpaceEngineersConsts.ProtobuffersExtension))
                    File.Move(sectorFilename + SpaceEngineersConsts.ProtobuffersExtension, sectorBackupFilename);
            }

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile(SectorData, tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile(SectorData, sectorFilename);
            }
            SpaceEngineersApi.WriteSpaceEngineersFilePB(SectorData, sectorFilename + SpaceEngineersConsts.ProtobuffersExtension, _compressedSectorFormat);
        }

        public XmlDocument LoadSectorXml()
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);
            var xDoc = new XmlDocument();

            try
            {
                if (ZipTools.IsGzipedFile(filename))
                {
                    // New file format is compressed.
                    // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                    // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                    var tempFilename = TempfileUtil.NewFilename();
                    ZipTools.GZipUncompress(filename, tempFilename);
                    xDoc.Load(tempFilename);
                    _compressedCheckpointFormat = true;
                }
                else
                {
                    // Old file format is raw XML.
                    xDoc.Load(filename);
                    _compressedCheckpointFormat = false;
                }
            }
            catch
            {
                return null;
            }

            return xDoc;
        }

        public void SaveSectorXml(bool backupFile, XmlDocument xDoc)
        {
            var sectorFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

            if (backupFile)
            {
                var sectorBackupFilename = sectorFilename + ".bak";

                if (File.Exists(sectorBackupFilename))
                {
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                File.Move(sectorFilename, sectorBackupFilename);
            }

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                xDoc.Save(tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
            {
                xDoc.Save(sectorFilename);
            }
        }

        public void SaveCheckPointAndSector(bool backupFile)
        {
            LastSaveTime = DateTime.Now;
            Checkpoint.AppVersion = SpaceEngineersConsts.GetSEVersionInt();
            SectorData.AppVersion = SpaceEngineersConsts.GetSEVersionInt();
            SaveCheckPoint(backupFile);
            SaveSector(backupFile);
        }

        public void LoadWorldInfo()
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            if (!File.Exists(filename))
            {
                IsValid = false;
                SessionName = Res.ErrorInvalidSaveLabel;
                return;
            }

            try
            {
                XDocument doc;
                using (var stream = MyFileSystem.OpenRead(filename).UnwrapGZip())
                {
                    doc = XDocument.Load(stream);
                }

                var root = doc.Root;
                if (root == null)
                {
                    IsValid = true;
                    SessionName = Res.ErrorInvalidSaveLabel;
                    return;
                }

                var session = root.Element("SessionName");
                var lastSaveTime = root.Element("LastSaveTime");
                var workshopId = root.Element("WorkshopId");
                var appVersion = root.Element("AppVersion");

                if (session != null) SessionName = MyStatControlText.SubstituteTexts(session.Value);
                DateTime tempDateTime;
                if (lastSaveTime != null && DateTime.TryParse(lastSaveTime.Value, out tempDateTime))
                    LastSaveTime = tempDateTime;
                else
                    LastSaveTime = DateTime.MinValue;

                ulong tmp;
                if (workshopId != null && ulong.TryParse(workshopId.Value, out tmp))
                    WorkshopId = tmp;

                if (appVersion == null || appVersion.Value == "0")
                    Version = new Version();
                else
                {
                    try
                    {
                        string str = appVersion.Value.Substring(0, appVersion.Value.Length - 6) + "." + appVersion.Value.Substring(appVersion.Value.Length - 6, 3) + "." + appVersion.Value.Substring(appVersion.Value.Length - 3);
                        Version = new Version(str);
                    }
                    catch
                    {
                        Version = new Version();
                    }
                }

                IsValid = true;
            }
            catch
            {
                IsValid = false;
                SessionName = Res.ErrorInvalidSaveLabel;
            }
        }

        #endregion

        #region Miscellaneous

        public MyObjectBuilder_Character FindPlayerCharacter()
        {
            if (SectorData == null || Checkpoint == null)
                return null;

            foreach (var entityBase in SectorData.SectorObjects)
            {
                var character = entityBase as MyObjectBuilder_Character;
                if (character != null && character.EntityId == Checkpoint.ControlledObject)
                {
                    return character;
                }

                var cubeGrid = entityBase as MyObjectBuilder_CubeGrid;
                if (cubeGrid != null)
                {
                    foreach (MyObjectBuilder_CubeBlock cube in cubeGrid.CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e.EntityId == Checkpoint.ControlledObject && e is MyObjectBuilder_Cockpit))
                    {
                        List<MyObjectBuilder_Character> pilots = cube.GetHierarchyCharacters();
                        if (pilots.Count > 0)
                            return pilots[0];
                    }
                }
            }

            return null;
        }

        public MyObjectBuilder_Character FindAstronautCharacter()
        {
            return SectorData != null ? SectorData.SectorObjects.OfType<MyObjectBuilder_Character>().FirstOrDefault() : null;
        }

        public MyObjectBuilder_Cockpit FindPilotCharacter()
        {
            if (SectorData != null)
            {
                foreach (var entityBase in SectorData.SectorObjects)
                {
                    if (entityBase is MyObjectBuilder_CubeGrid)
                    {
                        foreach (MyObjectBuilder_CubeBlock cube in ((MyObjectBuilder_CubeGrid)entityBase).CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit))
                        {
                            List<MyObjectBuilder_Character> pilots = cube.GetHierarchyCharacters();
                            if (pilots.Count > 0)
                                return (MyObjectBuilder_Cockpit)cube;
                        }

                    }
                }
            }

            return null;
        }

        #endregion

        #endregion
    }
}
