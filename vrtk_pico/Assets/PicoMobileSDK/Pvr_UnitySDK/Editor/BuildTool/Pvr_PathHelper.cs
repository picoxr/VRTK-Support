// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.

using System;
using System.IO;

public static class Pvr_PathHelper
{
    public static string MakeRelativePath(string fromPath, string toPath)
    {
        var fromUri = new Uri(Path.GetFullPath(fromPath));
        var toUri = new Uri(Path.GetFullPath(toPath));

        if (fromUri.Scheme != toUri.Scheme)
        {
            return toPath;
        }

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
        {
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        return relativePath;
    }
}
