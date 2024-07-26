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
                UnityWebRequest www2 = UnityWebRequest.Get(versionUrl);
                yield return www2.SendWebRequest();
                if (www2.error != null)
                {
                    retriedAccessServerTimes++;
                    Debug.LogFormat($"CheckUpdateAsync failed [retry={retriedAccessServerTimes}]: {www2.error}]");
                    www2.Dispose();
                    yield return new WaitForSeconds(retriedAccessServerTimes * 2f); // 增加处理网络稳定性的等待时间
                    continue;
                }

                if (www2.isDone)
                {
                    versionData = www2.downloadHandler.data;
                    www2.Dispose();
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

                while (retryCount < MaxRetryAccessServerTimes && !downloadSuccessful)
                {
                    var www2 = UnityWebRequest.Get(bundleServerUrl + abName);
                    www2.timeout = 60;
                    var asyncOperation = www2.SendWebRequest();

                    while (!asyncOperation.isDone)
                    {
                        updateLoaderProgress?.Invoke(downloadedBytes + (long)(size * asyncOperation.progress), sumBytes);
                        yield return null;
                    }

                    if (www2.error != null)
                    {
                        retryCount++;
                        Debug.LogFormat($"Download failed for {abName} [retry={retryCount}]: {www2.error}");
                        www2.Dispose();
                        yield return new WaitForSeconds(retryCount * 2f);
                        continue;
                    }

                    if (www2.isDone)
                    {
                        GameUtil.Write2Disk(Path.Combine(PathProtocol.DownloadBundleSaveDir, abName), www2.downloadHandler.data);
                        www2.Dispose();
                        downloadedBytes += size;
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