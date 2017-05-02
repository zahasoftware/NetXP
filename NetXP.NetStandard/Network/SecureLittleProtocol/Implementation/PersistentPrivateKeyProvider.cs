using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Cryptography;
using System.IO;
using NetXP.NetStandard.Serialization;
using System.Threading;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard;
using Microsoft.Extensions.Options;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementation
{
    public class PersistentPrivateKeyProvider : IPersistentPrivateKeyProvider
    {
        public PersistentPrivateKeyProvider(
            ISerializer serializer
            , IHash hash
            , ILogger logger
            , ICustomDateTime customDateTime
            , IOptions<PersistenPrivateKeyConfiguration> options
            , string ppkDirectory = null//Optional With InjectionContructor 
            )
        {
            this.serializer = serializer;
            this.hash = hash;
            this.logger = logger;
            this.customDateTime = customDateTime;
            this.ppkPath = Path.Combine(string.IsNullOrEmpty(ppkDirectory)
                                                    ? Directory.GetCurrentDirectory()
                                                    : ppkDirectory, ".ppk");

            this.Minutes2IncrementExpirtationDate = options.Value.Minutes2IncrementExpirtationDate ?? 60 * 24 * 30;

            Init();
        }

        private void Init()
        {
            this.MakeContainerDirectory();
            this.LoadAllFilesOfDirectory();
            this.WriteREADMEFile();
        }

        private readonly string ppkPath = "";
        private readonly ISerializer serializer;
        private readonly IHash hash;
        private readonly ILogger logger;
        private readonly ICustomDateTime customDateTime;

        public int Minutes2IncrementExpirtationDate { get; private set; }

        private void WriteREADMEFile()
        {
            if (!File.Exists(Path.Combine(ppkPath, "README")))
                File.WriteAllText(Path.Combine(ppkPath, "README"),
                    $@"Don't delete or add any file here, .ppk Directory is controlled by {System.Reflection.Assembly.GetEntryAssembly().FullName}
                       if any file is edited the connection could be lost by long time.");
        }
        private List<PersistentPrivateKey> LoadAllFilesOfDirectory()
        {
            var aFilesInPPK = Directory.GetFiles(ppkPath);

            var aFilesToDelete = aFilesInPPK.Where(o => !Path.GetFileNameWithoutExtension(o).StartsWith("ppk-") && !Path.GetFileNameWithoutExtension(o).Equals(("README")));
            aFilesToDelete.ToList().ForEach(o => File.Delete(o));

            var aFilesToLoad = aFilesInPPK.Where(o => Path.GetFileNameWithoutExtension(o).StartsWith("ppk-") && !Path.GetFileNameWithoutExtension(o).Equals(("README")));

            List<PersistentPrivateKey> allPPKInDirectory = new List<PersistentPrivateKey>();
            var filesToDelete = new List<string>();
            aFilesToLoad.ToList().ForEach(ppkFilePath =>
               {
                   using (var ppkFile = File.Open(ppkFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                   using (var ppkReader = new BinaryReader(ppkFile))
                   {
                       try
                       {
                           var ppkSerialized = ppkReader.ReadBytes((int)ppkFile.Length);
                           var ppk = this.serializer.Deserialize<PersistentPrivateKey>(ppkSerialized);
                           ppk.File = ppkFilePath;

                           if ((customDateTime.UtcNow - ppk.ExpirationDate).TotalMinutes > this.Minutes2IncrementExpirtationDate)
                           {
                               filesToDelete.Add(ppkFilePath);//Delete expired files
                           }
                           else
                           {
                               allPPKInDirectory.Add(ppk);
                           }
                       }
                       catch (Exception)
                       {
                           if (File.Exists(ppkFilePath))
                           {
                               filesToDelete.Add(ppkFilePath);//Corrupted files are deleted
                           }
                       }
                   }

               }
            );

            foreach (var fileToDelete in filesToDelete)
            {
                try
                {
                    File.Delete(fileToDelete);
                }
                catch (UnauthorizedAccessException)
                {
                    logger.Warn($"Unauthorized access exception when try to delete .ppk \"{fileToDelete}\" file.");
                }
            }

            return allPPKInDirectory;
        }

        private void MakeContainerDirectory()
        {
            if (!Directory.Exists(ppkPath))
            {
                var dir = Directory.CreateDirectory(ppkPath);
                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        public PersistentPrivateKey Read(string server, int port)
        {
#if DEBUG
            this.logger.Debug($"Read PPK With Server{server},Port={port}");
#endif
            if (string.IsNullOrEmpty(server) || port == 0)
            {
                return null;
            }
            var ppks = LoadAllFilesOfDirectory();
            var ppk = ppks.FirstOrDefault(o => o.Server == server && o.Port == port);
            if (ppk != null)
            {
                ppk.FromServer = false;
            }
            return ppk;
        }

        public PersistentPrivateKey Read(PublicKey publicKey)
        {
            if (publicKey == null)
            {
                return null;
            }
#if DEBUG
            var ppkHash = hash.Generate(new ByteArray(serializer.Serialize(publicKey)));
            this.logger.Debug($"Read PPK With RPK=\"{BitConverter.ToString(ppkHash)}\"");
#endif

            var ppk = this.LoadAllFilesOfDirectory().FirstOrDefault(o => o.PublicKeyRemote != null
                                                                      && o.PublicKeyRemote.Equals(publicKey));
            if (ppk != null)
            {
                ppk.FromServer = true;
            }
            return ppk;
        }

        public void Save(PersistentPrivateKey persistentPrivateKey)
        {
            this.Init();

            if (string.IsNullOrEmpty(persistentPrivateKey?.File))
            {
                persistentPrivateKey.File = Path.Combine(ppkPath, $"ppk-{DateTime.Now:MMfffmmHHyyddss}");
            }
            var clientPPK = this.Read(persistentPrivateKey.Server, persistentPrivateKey.Port);
            var serverPPK = this.Read(persistentPrivateKey.PublicKeyRemote);

            if (persistentPrivateKey == null)
            {
                throw new ArgumentNullException("persistentPrivateKey null");
            }
            else if (!persistentPrivateKey.FromServer && clientPPK != null)//If client is trying to save ppk
            {
                clientPPK.ExpirationDate = customDateTime.UtcNow.AddMinutes(this.Minutes2IncrementExpirtationDate);
                SaveFile(clientPPK);
            }
            else if (persistentPrivateKey.FromServer && serverPPK != null)
            {
                serverPPK.ExpirationDate = customDateTime.UtcNow.AddMinutes(this.Minutes2IncrementExpirtationDate);
                SaveFile(serverPPK);
            }
            else if (clientPPK == null && serverPPK == null)
            {
                persistentPrivateKey.ExpirationDate = customDateTime.UtcNow.AddMinutes(this.Minutes2IncrementExpirtationDate);
                SaveFile(persistentPrivateKey);
            }

#if DEBUG
            byte[] ppkSerialized = null;
            ppkSerialized = this.serializer.Serialize(persistentPrivateKey);
            var aPPKHash = hash.Generate(new ByteArray(ppkSerialized));
            var aPKHash = hash.Generate(new ByteArray(serializer.Serialize(persistentPrivateKey.PublicKeyRemote)));
            logger.Debug($"Saving PPK Hash:\"{BitConverter.ToString(aPPKHash)}\", PPK.PK:\"{BitConverter.ToString(aPKHash)}\"");
#endif
        }

        private void SaveFile(PersistentPrivateKey ppk)
        {
            using (var ppkFile = File.Open(ppk.File, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            using (var ppkSR = new BinaryWriter(ppkFile))
            {
                var ppkSerialized = this.serializer.Serialize(ppk);
                ppkSR.BaseStream.Write(ppkSerialized, 0, ppkSerialized.Length);
            }
        }
    }
}
