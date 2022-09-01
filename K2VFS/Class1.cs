using K2host.Erp.Classes;
using K2host.Threading.Classes;
using K2host.Vfs.Classes;
using K2host.Vfs.Extentions;
using System;

namespace K2VFS
{
    public class K2VFS
    {
        public void createvdisk(string vdiskpath,string rootAliasInVdisk)
        {
            try
            {
                //This can be any ERP / CRM user from any software where all 
                //we need is the ID of the object set as the owner of the vdisk.
                //OErpUser owner = OErpUser.Retrieve(1, ConnectionString, out _);

                //OServer engine = new(new OThreadManager())
                //{
                //    Version = "CodeName: VFSV5",
                //    OnCreateNewStatusEvent = (e, n) => { Console.WriteLine(n); },
                //    OnCreateNewCompleteEvent = (e) => { Console.WriteLine("Completed.."); }
                //};

                OServer engine = new OServer(new OThreadManager())
                {

                    OnMountedEvent = (e) => { Console.WriteLine("Mounted."); },
                    OnDismountedEvent = (e) => { Console.WriteLine("Dismounted."); },
                    OnDeviceUpdateEvent = (e) => { Console.WriteLine("Device Updated."); },

                    OnBackupStatusEvent = (e, status) => { Console.WriteLine(status); },
                    OnBackupCompleteEvent = (e) => { Console.WriteLine("Backup Complete."); },

                    OnRestoreStatusEvent = (e, status) => { Console.WriteLine(status); },
                    OnRestoreCompleteEvent = (e) => { Console.WriteLine("Restore Complete."); },

                    OnCreateNewStatusEvent = (e, status) => { Console.WriteLine(status); },
                    OnCreateNewCompleteEvent = (e) => { Console.WriteLine("Create New Complete."); },

                    OnDefragePreperationEvent = (e) => { Console.WriteLine("Defrage Preperation."); },
                    OnDefrageStatusEvent = (e, status) => { Console.WriteLine(status); },
                    OnDefragProgressNextEvent = (e, status) => { Console.WriteLine("Defrage Pass " + status.ToString()); },
                    OnDefragProgressResetEvent = (e, status) => { Console.WriteLine("Defrage Reset " + status.ToString()); },
                    OnDefrageCompleteEvent = (e) => { Console.WriteLine("Defrage Complete."); },
                    OnDefrageErrorEvent = (e) => { Console.WriteLine("Defrage Error."); },

                    OnDirectoryAddedEvent = (e, status) => { Console.WriteLine(status); },

                    OnDirectoryDeletingEvent = (e, status) => { Console.WriteLine(status); },
                    OnDirectoryDeletedEvent = (e, status) => { Console.WriteLine(status); },
                    OnDirectoryRestoredEvent = (e, status) => { Console.WriteLine(status); },
                    OnDirectoryRestoringEvent = (e, status) => { Console.WriteLine(status); },

                    OnEmptyRecycleBinPreperationEvent = (e) => { Console.WriteLine("Empty Recycle Bin Preperation"); },
                    OnEmptyRecycleBinStartEvent = (e, status) => { Console.WriteLine(status); },
                    OnEmptyRecycleBinItemDeletedEvent = (e) => { Console.WriteLine("Empty Recycle Bin Item Deleted"); },
                    OnEmptyRecycleBinCompleteEvent = (e) => { Console.WriteLine("Empty Recycle Bin Complete."); },

                    OnFileAddedEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileAddingEvent = (e, status, n) => { Console.WriteLine(status); },
                    OnFileDeletedEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileDeletingEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileExportedEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileExportingEvent = (e, status, n) => { Console.WriteLine(status); },
                    OnFileRestoredEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileRestoringEvent = (e, status) => { Console.WriteLine(status); },
                    OnFileSavedEvent = (e, status) => { Console.WriteLine(status); },

                    OnMergeCompletedEvent = (e) => { Console.WriteLine("Merge Completed."); },
                    OnMergeErrorEvent = (e) => { Console.WriteLine("Merge Error."); },
                    OnMergeProgressNextEvent = (e, n) => { Console.WriteLine("Merge Progress Next " + n.ToString()); },
                    OnMergeProgressResetEvent = (e, n) => { Console.WriteLine("Merge Progress Reset " + n.ToString()); },
                    OnMergeStartEvent = (e) => { Console.WriteLine("Merge Start."); },
                    OnMergeStatusEvent = (e, status) => { Console.WriteLine(status); },

                    OnErrorEvent = (e, ex) => { Console.WriteLine(ex.ToString()); },

                    Version = "CodeName: VFSV5",
                    SpeedBuffer = 4096 * 4, // 16384

                };

                engine.CreateNew(
                    @vdiskpath,
                    @rootAliasInVdisk,  // The root alias in the Vdisk.
                    new OUserRequirements()
                    {
                        //UserId = owner.Uid,
                        SpaceLimit = -1,    // Any size allowed.
                        SpaceUsed = 0
                    },
                    "AESkey20220831").Dispose();

                //owner.Dispose();

            }
            catch (Exception ex)
            {

                Console.Write(ex.Message);

            }
        }
    }
}
