using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.Storage.Vhd;

public static class VhdMethods
{
    // Given the file name, returns the storage type for opening.
    private static VIRTUAL_STORAGE_TYPE GetStorageType(string vhdFileName)
    {
        var storageType = new VIRTUAL_STORAGE_TYPE();
        storageType.VendorId = PInvoke.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT;

        string extension = Path.GetExtension(vhdFileName);
        var cmp = StringComparer.OrdinalIgnoreCase;

        if (cmp.Equals(extension, ".vhdx"))
        {
            storageType.DeviceId = PInvoke.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX;
        }
        else if (cmp.Equals(extension, ".vhd"))
        {
            storageType.DeviceId = PInvoke.VIRTUAL_STORAGE_TYPE_DEVICE_VHD;
        }
        else if (cmp.Equals(extension, ".iso"))
        {
            storageType.DeviceId = PInvoke.VIRTUAL_STORAGE_TYPE_DEVICE_ISO;
        }
        else
        {
            storageType.DeviceId = PInvoke.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN;
        }

        return storageType;
    }

    // Marks a VHD so that it will be automatically attached at boot.
    //
    // N.B. The VHD must not be currently attached/mounted (else you will get a
    // sharing violation).
    public static void MarkVhdAsAttachAtBoot(string vhdPath)
    {
        VIRTUAL_STORAGE_TYPE storageType = GetStorageType(vhdPath);
        var openParams = new OPEN_VIRTUAL_DISK_PARAMETERS();
        openParams.Version = OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_3;

        // Using an access mask of "none" seems odd, but for v3 opens, that's actually
        // the only accepted value.
        VIRTUAL_DISK_ACCESS_MASK accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_NONE;
        OPEN_VIRTUAL_DISK_FLAG openFlags = OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE;
        SafeFileHandle diskHandle;

        var error = PInvoke.OpenVirtualDisk(storageType,
                                        vhdPath,
                                        accessMask,
                                        openFlags,
                                        openParams,
                                        out diskHandle);
        if (error != 0)
        {
            throw new System.ComponentModel.Win32Exception((int)error);
        }

        using (diskHandle)
        {
            var attachParameters = new ATTACH_VIRTUAL_DISK_PARAMETERS();
            attachParameters.Version = ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_2;

            var attachFlags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_AT_BOOT |
                                ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME;

            error = PInvoke.AttachVirtualDisk(diskHandle,
                                        new(),     // security descriptor
                                        attachFlags,
                                        0,               // provider-specific flags
                                        attachParameters,
                                        null);   // overlapped
            if (error != 0)
            {
                // You could get a sharing violation here (ERROR_SHARING_VIOLATION or
                // equivalent) if the disk is already attached--in that case, you need
                // to detach the disk first, then you can try again.
                throw new System.ComponentModel.Win32Exception((int)error);
            }
        }
    }

}