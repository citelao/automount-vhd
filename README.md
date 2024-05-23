# Automount VHD at boot

VHDs created via the command line (e.g. with `New-VHD`) do not automount on
boot. This example project shows off the native API that can enable
automounting.

## Example

```pwsh
# In an admin window.
# cd thisDirectory

# Create the VHD
$vhdName = "c:\test.vhdx"
New-VHD $vhdName -size 5mb

# Run this program to enable automounting.
dotnet run -- $vhdName
```

## Thanks

Dan Thompson wrote the original code, which I ported to CSWin32.

## See also

* https://learn.microsoft.com/en-us/windows/win32/api/virtdisk/ne-virtdisk-attach_virtual_disk_flag