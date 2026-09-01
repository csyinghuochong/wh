using System.IO;

namespace ET
{
    public static class RecastFileReader
    {
        public static byte[] Read(string name)
        {
            string path = ResolveNavPath(name);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new System.Exception($"no nav data: {name}");
            }

            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// Scenes_300 / 300 都转成 RecastDll 用的 int。
        /// </summary>
        public static int ToNavId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return 0;
            }

            if (int.TryParse(name, out int id))
            {
                return id;
            }

            int us = name.LastIndexOf('_');
            if (us >= 0 && us < name.Length - 1 && int.TryParse(name.Substring(us + 1), out id))
            {
                return id;
            }

            int h = 23;
            for (int i = 0; i < name.Length; i++)
            {
                h = h * 31 + name[i];
            }

            return h & 0x7fffffff;
        }

        public static string ResolveNavPath(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            int id = ToNavId(name);
            string[] fileNames =
            {
                name,
                id > 0 ? $"Scenes_{id}" : null,
                id > 0 ? id.ToString() : null,
            };
            string[] dirs =
            {
                "../Config/Recast",
                "../Config/RecastNavData",
            };
            string[] exts =
            {
                "",
                ".bin",
                ".bytes",
            };

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                for (int d = 0; d < dirs.Length; d++)
                {
                    for (int e = 0; e < exts.Length; e++)
                    {
                        string path = Path.Combine(dirs[d], fileName + exts[e]);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            return Path.Combine("../Config/Recast", name);
        }
    }
}
