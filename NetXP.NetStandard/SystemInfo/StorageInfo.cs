using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.ext;

namespace NetXP.NetStandard.SystemInfo
{
    public class StorageInfo
    {
        //
        // Summary:
        //     Indicates the amount of available free space on a drive, in bytes.
        //
        // Returns:
        //     The amount of free space available on the drive, in bytes.
        //
        // Exceptions:
        //   T:System.UnauthorizedAccessException:
        //     Access to the drive information is denied.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public long AvailableFreeSpace { get; set; }
        //
        // Summary:
        //     Gets the name of the file system, such as NTFS or FAT32.
        //
        // Returns:
        //     The name of the file system on the specified drive.
        //
        // Exceptions:
        //   T:System.UnauthorizedAccessException:
        //     Access to the drive information is denied.
        //
        //   T:System.IO.DriveNotFoundException:
        //     The drive does not exist or is not mapped.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public string DriveFormat { get; set; }
        //
        // Summary:
        //     Gets the drive type, such as CD-ROM, removable, network, or fixed.
        //
        // Returns:
        //     One of the enumeration values that specifies a drive type.
        public DriveType DriveType { get; set; }
        //
        // Summary:
        //     Gets a value that indicates whether a drive is ready.
        //
        // Returns:
        //     true if the drive is ready; false if the drive is not ready.
        public bool IsReady { get; set; }
        //
        // Summary:
        //     Gets the name of a drive, such as C:\.
        //
        // Returns:
        //     The name of the drive.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the root directory of a drive.
        //
        // Returns:
        //     An object that contains the root directory of the drive.
        public DirectoryInfo RootDirectory { get; set; }
        //
        // Summary:
        //     Gets the total amount of free space available on a drive, in bytes.
        //
        // Returns:
        //     The total free space available on a drive, in bytes.
        //
        // Exceptions:
        //   T:System.UnauthorizedAccessException:
        //     Access to the drive information is denied.
        //
        //   T:System.IO.DriveNotFoundException:
        //     The drive is not mapped or does not exist.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public long TotalFreeSpace { get; set; }

        //
        // Summary:
        //     Gets the total amount of free space available on a drive, in human readable.
        //
        // Returns:
        //     The total free space available on a drive, in human readable ex: 10GB or 10MB.
        //
        public string TotalFreeSpaceInHumanReadable { get { return TotalFreeSpace.BytesToHumanReadable(); } }

        //
        // Summary:
        //     Gets the total size of storage space on a drive, in bytes.
        //
        // Returns:
        //     The total size of the drive, in bytes.
        //
        public long TotalSize { get; set; }


        //
        // Summary:
        //     Gets the total size of storage space on a drive, in a human readable.
        //
        // Returns:
        //     The total size of the drive, in MB GB Or Any other.
        //
        // Exceptions:
        //   T:System.UnauthorizedAccessException:
        //     Access to the drive information is denied.
        //
        //   T:System.IO.DriveNotFoundException:
        //     The drive is not mapped or does not exist.
        //
        //   T:System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public string TotalSizeInHumanReadable { get { return TotalSize.BytesToHumanReadable(); } }

        //
        // Summary:
        //     Gets or sets the volume label of a drive.
        //
        // Returns:
        //     The volume label.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        //
        //   T:System.IO.DriveNotFoundException:
        //     The drive is not mapped or does not exist.
        //
        //   T:System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   T:System.UnauthorizedAccessException:
        //     The volume label is being set on a network or CD-ROM drive.-or-Access to the
        //     drive information is denied.
        public string VolumeLabel { get; set; }
    }
}
