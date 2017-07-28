namespace NetXP.NetStandard.SystemInformation
{
    public enum DriveType
    {
        //
        // Summary:
        //     The type of drive is unknown.
        Unknown = 0,
        //
        // Summary:
        //     The drive does not have a root directory.
        NoRootDirectory = 1,
        //
        // Summary:
        //     The drive is a removable storage device, such as a floppy disk drive or a USB.
        //     flash drive.
        Removable = 2,
        //
        // Summary:
        //     The drive is a fixed disk.
        Fixed = 3,
        //
        // Summary:
        //     The drive is a network drive.
        Network = 4,
        //
        // Summary:
        //     The drive is an optical disc device, such as a CD or DVD-ROM.
        CDRom = 5,
        //
        // Summary:
        //     The drive is a RAM disk.
        Ram = 6
    }
}