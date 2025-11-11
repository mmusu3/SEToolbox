using System.Collections.Generic;
using System.Threading;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using VRage.Game;
using VRage.GameServices;

namespace SEToolbox.Interop
{
    public class SpaceEngineersWorkshop
    {
        public static MyWorkshop.ResultData DownloadWorldModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, MyWorkshop.CancelToken cancelToken)
        {
            if (!MyGameService.IsActive)
                return default;

            MyWorkshop.ResultData ret = default;

            Task task = Parallel.Start(() =>
            {
                ret = DownloadWorldModsBlockingInternal(mods, cancelToken);
            });

            while (!task.IsComplete)
            {
                MyGameService.Update();
                Thread.Sleep(10);
            }

            return ret;
        }

        static MyWorkshop.ResultData DownloadWorldModsBlockingInternal(List<MyObjectBuilder_Checkpoint.ModItem> mods, MyWorkshop.CancelToken cancelToken)
        {
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - START");
            MySandboxGame.Log.IncreaseIndent();

            MyWorkshop.ResultData resultData = default;
            resultData.Result = MyGameServiceCallResult.OK;

            if (mods != null && mods.Count > 0)
            {
                FixModServiceName(mods);

                var list = new List<WorkshopId>();

                foreach (MyObjectBuilder_Checkpoint.ModItem mod in mods)
                {
                    if (mod.PublishedFileId != 0L)
                    {
                        var item = new WorkshopId(mod.PublishedFileId, mod.PublishedServiceName);

                        if (!list.Contains(item))
                            list.Add(item);
                    }
                    else if (Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        MySandboxGame.Log.WriteLineAndConsole("Local mods are not allowed in multiplayer.");
                        MySandboxGame.Log.DecreaseIndent();
                        return default;
                    }
                }

                list.Sort();

                bool flag = false;

                if (MyPlatformGameSettings.CONSOLE_COMPATIBLE)
                {
                    foreach (WorkshopId item2 in list)
                    {
                        var aggregate = MyGameService.WorkshopService.GetAggregate(item2.ServiceName);

                        if (aggregate == null)
                        {
                            flag = true;
                            MySandboxGame.Log.WriteLineAndConsole($"Can't download mod {item2.Id}. Service {item2.ServiceName} is not available");
                        }
                        else if (!aggregate.IsConsoleCompatible)
                        {
                            flag = true;
                            MySandboxGame.Log.WriteLineAndConsole($"Can't download mod {item2.Id}. Service {aggregate.ServiceName} is not console compatible");
                        }
                    }
                }

                if (flag)
                {
                    resultData.Result = MyGameServiceCallResult.Fail;
                }
                //else if (Sandbox.Engine.Platform.Game.IsDedicated)
                //{
                //    if (MySandboxGame.ConfigDedicated.AutodetectDependencies)
                //        AddModDependencies(mods, list);

                //    MyGameService.SetServerModTemporaryDirectory();
                //    resultData = DownloadModsBlocking(mods, list, cancelToken);
                //}
                else
                {
                    if (Sync.IsServer)
                        AddModDependencies(mods, list);

                    resultData = DownloadModsBlocking(mods, list, cancelToken);
                }
            }

            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - END");

            if (cancelToken != null)
                resultData.Cancel |= cancelToken.Cancel;

            return resultData;
        }

        static void AddModDependencies(List<MyObjectBuilder_Checkpoint.ModItem> mods, List<WorkshopId> workshopIds)
        {
            var hashSet = new HashSet<WorkshopId>();
            var hashSet2 = new HashSet<WorkshopId>();

            foreach (var mod in mods)
            {
                var item = new WorkshopId(mod.PublishedFileId, mod.PublishedServiceName);
                hashSet2.Add(item);

                if (!mod.IsDependency && mod.PublishedFileId != 0L)
                    hashSet.Add(item);
            }

            bool hasReferenceIssue;

            foreach (var item3 in MyWorkshop.GetModsDependencyHiearchy(hashSet, out hasReferenceIssue))
            {
                var item2 = new WorkshopId(item3.Id, item3.ServiceName);

                if (hashSet2.Add(item2))
                {
                    mods.Add(new MyObjectBuilder_Checkpoint.ModItem(item3.Id, item3.ServiceName, isDependency: true) {
                        FriendlyName = item3.Title
                    });
                }

                if (!workshopIds.Contains(item2))
                    workshopIds.Add(item2);
            }
        }

        static MyWorkshop.ResultData DownloadModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, List<WorkshopId> workshopIds, MyWorkshop.CancelToken cancelToken)
        {
            var ret = new MyWorkshop.ResultData(MyGameServiceCallResult.OK, false);
            var toGet = new List<MyWorkshopItem>(workshopIds.Count);

            if (!MyWorkshop.GetItemsBlockingUGC(workshopIds, toGet))
            {
                MySandboxGame.Log.WriteLine("Could not obtain workshop item details");
                ret.Result = MyGameServiceCallResult.Fail;
            }

            // Changed from SE to not fail if some mods could not be found

            //else if (workshopIds.Count != toGet.Count)
            //{
            //    MySandboxGame.Log.WriteLine($"Could not obtain all workshop item details, expected {workshopIds.Count}, got {toGet.Count}");
            //    ret.Result = MyGameServiceCallResult.Fail;
            //}
            else
            {
                //if (m_downloadScreen != null)
                //{
                //    MySandboxGame.Static.Invoke(delegate
                //    {
                //        m_downloadScreen.MessageText = new StringBuilder(MyTexts.GetString(MyCommonTexts.ProgressTextDownloadingMods) + " 0 of " + toGet.Count);
                //    }, "DownloadModsBlocking");
                //}

                ret = MyWorkshop.DownloadModsBlockingUGC(toGet, cancelToken);

                if (ret.Result != MyGameServiceCallResult.OK)
                {
                    MySandboxGame.Log.WriteLine($"Downloading mods failed, Result: {ret.Result}");
                }
                else
                {
                    var array = mods.ToArray();

                    for (int i = 0; i < array.Length; i++)
                    {
                        var myWorkshopItem = toGet.Find((MyWorkshopItem x) => x.Id == array[i].PublishedFileId);

                        if (myWorkshopItem != null)
                        {
                            array[i].FriendlyName = myWorkshopItem.Title;
                            array[i].SetModData(myWorkshopItem);
                        }
                        else
                        {
                            array[i].FriendlyName = array[i].Name;
                        }
                    }

                    mods.Clear();
                    mods.AddRange(array);
                }
            }

            return ret;
        }

        static void FixModServiceName(List<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                var value = mods[i];

                if (string.IsNullOrEmpty(value.PublishedServiceName))
                {
                    value.PublishedServiceName = MyGameService.GetDefaultUGC().ServiceName;
                    mods[i] = value;
                }
            }
        }
    }
}
