using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XEngine.Engine;


namespace XEngine.Editor
{
    internal class AssetBundleBuilder
    {
        public static void Build(string abOutputDir, BuildTarget targetPlatform, string version)
        {
            // 先把 LuaProject 下面的的代码资源打包到 Res2Bundle 目录下
            Res2BundleBuilder.BuildRes2Bundle();
            if (!Directory.Exists(abOutputDir))
            {
                Directory.CreateDirectory(abOutputDir);
            }
            AnalysisRes2Bundle(out var toBuildBundleArray, out var ignoredList);
            foreach (string item in ignoredList)
            {
                Debug.LogError("The asset will not be packed in bundle: " + item);
            }
            BuildPipeline.BuildAssetBundles(abOutputDir, toBuildBundleArray, BuildAssetBundleOptions.ChunkBasedCompression, targetPlatform);
            AssetDatabase.Refresh();

            var bundleName2BundleInfo = new Dictionary<string, BundleInfo>();
            string[] files = Directory.GetFiles(abOutputDir);
            foreach (var filePath in files)
            {
                if (filePath.EndsWith(".manifest") || filePath.EndsWith(".meta") || filePath.EndsWith(PathProtocol.VersionFileName))
                {
                    continue;
                }
                var fileMD5 = GameUtil.GetFileMD5(filePath);
                long fileSize = GameUtil.GetFileSize(filePath);
                var fileName = Path.GetFileName(filePath);
                var bundleInfo = new BundleInfo()
                {
                    name = fileName,
                    md5 = fileMD5,
                    size = fileSize
                };
                bundleName2BundleInfo[bundleInfo.name] = bundleInfo;
            }
            var versionInfo = VersionInfo.BuildVersionInfo(version, bundleName2BundleInfo);
            var verInfoJson = JsonUtility.ToJson(versionInfo);
            Debug.Log($"New build version info json is : {verInfoJson}");
            File.WriteAllText(Path.Combine(abOutputDir, PathProtocol.VersionFileName), verInfoJson);
            AssetDatabase.Refresh();
            files = Directory.GetFiles(abOutputDir);
            foreach (var filePath in files) // TODO: 删掉.manifest文件的目的是什么?
            {
                if (filePath.EndsWith(".manifest") || filePath.EndsWith(".manifest.meta"))
                {
                    File.Delete(filePath);
                }
            }
            AssetDatabase.Refresh();
        }

        private static readonly string[] abExtensionBlacklists = new string[]
        {
            ".dll",
            ".cs",
            ".gitignore",
            ".js",
            ".boo",
        };

        private static bool IsSupportedAsset(string file)
        {
            var mainAssetTypeAtPath = AssetDatabase.GetMainAssetTypeAtPath(file);
            if (mainAssetTypeAtPath == typeof(LightingDataAsset))
            {
                return false;
            }
            if (mainAssetTypeAtPath == typeof(DefaultAsset))
            {
                return false;
            }
            foreach (var extension in abExtensionBlacklists)
            {
                if (file.EndsWith(extension))
                {
                    return false;
                }
            }
            return true;
        }

        private static void AnalysisRes2Bundle(out AssetBundleBuild[] toBuildBundleArray, out List<string> ignoredList)
        {
            var bundleName2Files = new Dictionary<string, List<string>>();

            // 先检查 Res2Bundle 下的检查所有文件(不包括子文件夹), 不应该有.meta之外的文件
            ignoredList = new();
            string[] files = Directory.GetFiles(PathProtocol.Res2BundleDirPath);
            foreach (var filePath in files)
            {
                if (!filePath.EndsWith(".meta"))
                {
                    ignoredList.Add(Path.GetFileName(filePath));
                }
            }

            // Res2Bundle 目录下的每个文件夹里面的都打包成一个ab包
            string[] subDirPaths = Directory.GetDirectories(PathProtocol.Res2BundleDirPath);
            foreach (var subDirPath in subDirPaths)
            {
                var subDirName = Path.GetRelativePath(PathProtocol.Res2BundleDirPath, subDirPath);
                files = GameUtil.GetAllFilesFromDirectory(subDirPath);
                // 子文件夹名字作为 bundle 名字
                var bundleName = subDirName.ToLower();
                if (!bundleName2Files.ContainsKey(bundleName))
                {
                    bundleName2Files.Add(bundleName, new List<string>());
                }
                for (var k = 0; k < files.Length; k++)
                {
                    if (files[k].EndsWith(".meta"))
                    {
                        continue;
                    }
                    string filePath = files[k].Replace('\\', '/');
                    string fileRelativePath = Path.GetRelativePath(subDirPath, filePath);
                    string assetFilePath = Path.Combine(PathProtocol.AssetsPathRes2BundleDir, subDirName, fileRelativePath);
                    if (IsSupportedAsset(assetFilePath))
                    {
                        var assetName = assetFilePath.ToLower(); // 使用相对路径作为资源名字, 在加载ab包的时候, 也使用相对路径加载
                        Debug.LogFormat("Asset name is {0}", assetName);
                        bundleName2Files[bundleName].Add(assetName);

                        var progress = (k + 1f) / files.Length;
                        EditorUtility.DisplayCancelableProgressBar("Collect Bundle Assets ..", assetFilePath, progress);
                    }
                    else
                    {
                        ignoredList.Add(assetFilePath);
                    }
                }
            }

            toBuildBundleArray = bundleName2Files.Select(x => new AssetBundleBuild
            {
                assetBundleName = x.Key,
                assetNames = x.Value.ToArray()
            }).ToArray();
            EditorUtility.ClearProgressBar();
        }

    }

}