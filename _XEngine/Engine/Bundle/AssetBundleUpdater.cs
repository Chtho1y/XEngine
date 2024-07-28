using UnityEngine;
using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System.Collections.Generic;


namespace XEngine.Engine
{
    public class AssetBundleUpdater
    {
        public bool NeedsUpdate { get; private set; } = false; // 是否需要更新ab包
        private const int MaxRetryAccessServerTimes = 3; // 访问ab服务器的最大重试次数
        private int retriedAccessServerTimes = 0; // 访问ab服务器的已重试次数

        public void CheckUpdate(ResourceMode resMode, string bundleServerUrl, Action<UpdateStatus> updateCallback, Action<long, long> updateLoaderProgress = null)
        {
            BundleManager.Instance.SetResourceMode(resMode);
            if (BundleManager.Instance.IsLoadRaw())
            {
                // 本地读取Lua源码文件
                GameManager.Instance.InitLua();
                updateCallback?.Invoke(UpdateStatus.NoNeedUpdate);
                return;
            }
            if (resMode == ResourceMode.LocalStreamingAssetBundle)
            {
                // 从本地的 StreamingAsset 读取文件
                // UnityWebRequest 不能直接在Mac,Linux上用 StreamingAssetPath 读取本地文件, 需要加上 file://
                bundleServerUrl = "file://" + PathProtocol.LocalStreamingAssetBundlePath;
            }
            GameManager.Instance.StartCoroutine(CheckUpdateAsync(bundleServerUrl, updateCallback, updateLoaderProgress));
        }

        private IEnumerator CheckUpdateAsync(string bundleServerUrl, Action<UpdateStatus> updateCallback, Action<long, long> updateLoaderProgress)
        {
            string versionUrl = bundleServerUrl + PathProtocol.VersionFileName;
            Debug.LogFormat($"CheckUpdateAsync[Retry={retriedAccessServerTimes}]: {versionUrl}");
            if (!Directory.Exists(PathProtocol.DownloadBundleSaveDir))
            {
                Directory.CreateDirectory(PathProtocol.DownloadBundleSaveDir);
            }
            while (!Caching.ready)
            {
                yield return null;
            }

            byte[] versionData = null;
            while (retriedAccessServerTimes < MaxRetryAccessServerTimes)
            {
                UnityWebRequest req = UnityWebRequest.Get(versionUrl);
                yield return req.SendWebRequest();
                if (req.error != null)
                {
                    retriedAccessServerTimes++;
                    Debug.LogFormat($"CheckUpdateAsync failed [retry={retriedAccessServerTimes}]: {req.error}]");
                    req.Dispose();
                    yield return new WaitForSeconds(retriedAccessServerTimes * 10); // 增加处理网络稳定性的等待时间
                    continue;
                }

                if (req.isDone)
                {
                    versionData = req.downloadHandler.data;
                    req.Dispose();
                    break;
                }
            }

            if (versionData == null)
            {
                Debug.LogError("CheckUpdateAsync failed: " + versionUrl);
                yield break;
            }

            string versionInfoJson = GameUtil.Bytes2String(versionData);
            var newVersion = JsonUtility.FromJson<VersionInfo>(versionInfoJson);

            UpdateStatus status = UpdateStatus.NoNeedUpdate;
            Queue<BundleInfo> toDownloads = new();
            long sumBytes = 0L;
            long downloadedBytes = 0L;
            var localVersionFilePath = Path.Combine(PathProtocol.DownloadBundleSaveDir, PathProtocol.VersionFileName);
            VersionInfo oldVersion = VersionUtil.LoadVersionInfoFromFile(localVersionFilePath);
            status = VersionUtil.CompareVersionForUpdate(oldVersion, newVersion);

            switch (status)
            {
                case UpdateStatus.NoNeedUpdate:
                    NeedsUpdate = false;
                    break;
                case UpdateStatus.NeedDownloadNewClient:
                    GameManager.Instance.InitLua();
                    updateCallback?.Invoke(status);
                    yield break;
                case UpdateStatus.FirstTime:
                    NeedsUpdate = true;
                    var firstNewBundleInfo = newVersion.DecodeBundleInfo();
                    foreach (KeyValuePair<string, BundleInfo> info in firstNewBundleInfo)
                    {
                        var filePath = PathProtocol.DownloadBundleSaveDir + info.Key;
                        if (!File.Exists(filePath))
                        {
                            toDownloads.Enqueue(info.Value);
                            sumBytes += info.Value.size;
                            continue;
                        }
                        var md5 = GameUtil.GetFileMD5(filePath);
                        if (md5 != info.Value.md5)
                        {
                            toDownloads.Enqueue(info.Value);
                            sumBytes += info.Value.size;
                        }
                    }
                    break;
                default:
                    NeedsUpdate = true;
                    var newBundleInfo = newVersion.DecodeBundleInfo();
                    var oldBundleInfo = oldVersion.DecodeBundleInfo();
                    foreach (KeyValuePair<string, BundleInfo> old in oldBundleInfo)
                    {
                        if (!newBundleInfo.ContainsKey(old.Key))
                        {
                            var filePath = PathProtocol.DownloadBundleSaveDir + old.Key;
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }
                    foreach (KeyValuePair<string, BundleInfo> info in newBundleInfo)
                    {
                        var filePath = PathProtocol.DownloadBundleSaveDir + info.Key;
                        if (!File.Exists(filePath))
                        {
                            toDownloads.Enqueue(info.Value);
                            sumBytes += info.Value.size;
                            continue;
                        }
                        var md5 = GameUtil.GetFileMD5(filePath);
                        if (md5 != info.Value.md5)
                        {
                            toDownloads.Enqueue(info.Value);
                            sumBytes += info.Value.size;
                        }
                    }
                    break;
            }

            while (toDownloads.Count > 0)
            {
                BundleInfo bundleInfo = toDownloads.Dequeue();
                string abName = bundleInfo.name;
                long size = bundleInfo.size;
                int retryCount = 0;
                bool downloadSuccessful = false;
                long existingFileSize = 0;
                string filePath = Path.Combine(PathProtocol.DownloadBundleSaveDir, abName);

                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    existingFileSize = fileInfo.Length;
                }

                while (retryCount < MaxRetryAccessServerTimes && !downloadSuccessful)
                {
                    UnityWebRequest req2 = UnityWebRequest.Get(bundleServerUrl + abName);
                    if (existingFileSize > 0)
                    {
                        req2.SetRequestHeader("Range", "bytes=" + existingFileSize + "-");
                    }
                    req2.timeout = 60;
                    var asyncOperation = req2.SendWebRequest();

                    while (!asyncOperation.isDone)
                    {
                        updateLoaderProgress?.Invoke(downloadedBytes + existingFileSize + (long)(size * asyncOperation.progress), sumBytes);
                        yield return null;
                    }

                    if (req2.error != null)
                    {
                        retryCount++;
                        Debug.LogFormat($"Download failed for {abName} [retry={retryCount}]: {req2.error}");
                        req2.Dispose();
                        yield return new WaitForSeconds(retryCount * 2f);
                        continue;
                    }

                    if (req2.isDone)
                    {
                        byte[] data = req2.downloadHandler.data;
                        if (existingFileSize > 0)
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Append))
                            {
                                fileStream.Write(data, 0, data.Length);
                            }
                        }
                        else
                        {
                            GameUtil.Write2Disk(filePath, data);
                        }

                        req2.Dispose();
                        downloadedBytes += data.Length;
                        updateLoaderProgress?.Invoke(downloadedBytes, sumBytes);
                        Debug.LogFormat($"Downloaded: {downloadedBytes}/{sumBytes} Bytes");
                        downloadSuccessful = true;
                    }
                }

                if (!downloadSuccessful)
                {
                    throw new Exception($"Failed to download bundle {abName} after {MaxRetryAccessServerTimes} attempts.");
                }
            }

            var versionSaveFilePath = Path.Combine(PathProtocol.DownloadBundleSaveDir, PathProtocol.VersionFileName);
            GameUtil.Write2Disk(versionSaveFilePath, versionData);
            yield return new WaitForSeconds(1f);

            var allBundleSavedPath = Path.Combine(PathProtocol.DownloadBundleSaveDir, "AssetBundle");
            AssetBundle allBundle = AssetBundle.LoadFromFile(allBundleSavedPath);
            var manifest = allBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            BundleManager.Instance.SetManifest(manifest);
            GameManager.Instance.InitLua();
            updateCallback?.Invoke(status);
        }
    }
}