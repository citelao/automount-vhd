if (args.Length == 0)
{
    Console.WriteLine("Marks a VHD so that it will be automatically attached at boot.");
    Console.WriteLine();
    Console.WriteLine("Usage: programname.exe <path-to-vhd>");
    return;
}

var path = args[0];
Console.WriteLine($"Marking VHD {path} as attach-at-boot...");
VhdMethods.MarkVhdAsAttachAtBoot(path);