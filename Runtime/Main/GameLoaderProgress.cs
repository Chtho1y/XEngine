using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using XEngine.Engine;


namespace XEngine.Main
{
    public class GameLoaderProgress : MonoBehaviour
    {
        [SerializeField]
        private Slider loadingSlider;
        [SerializeField]
        private Text loadingTip;
        private float simulatedProgress = 0f;
        private float targetProgress = 0f;
        private float actualProgress = 0f;
        private bool needsUpdate = false;

        private void Start()
        {
            loadingSlider.value = 0f;
            loadingSlider.fillRect.gameObject.SetActive(true);
        }

        public void UpdateLoadingProgress(long currentBytes, long totalBytes)
        {
            actualProgress = (float)currentBytes / totalBytes;
            targetProgress = actualProgress;

            float loadedMB = (float)currentBytes / 1024 / 1024;
            float allMB = (float)totalBytes / 1024 / 1024;

            loadingTip.text = string.Format("下载资源文件中... {0}%, {1}/{2}M", (actualProgress * 100).ToString("f0"), loadedMB.ToString("f2"), allMB.ToString("f2"));

            if (actualProgress >= 1f)
            {
                simulatedProgress = 1f;
                loadingSlider.value = simulatedProgress;
                loadingTip.text = "下载完成!";
            }
        }

        public void Begin(bool needsUpdate)
        {
            this.needsUpdate = needsUpdate;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            simulatedProgress = 0f;
            targetProgress = 0f;
            actualProgress = 0f;
            loadingSlider.value = 0f;

            if (needsUpdate)
            {
                // 初始阶段模拟进度条平滑移动
                UniTaskUtil.Instance.StartUniTask(nameof(InitialSmoothProgress), token => InitialSmoothProgress(token));
            }
            // 然后根据实际进度更新进度条
            UniTaskUtil.Instance.StartUniTask(nameof(SmoothProgress), token => SmoothProgress(token));
        }

        public void End()
        {
            StopTasks().Forget();
        }

        private async UniTaskVoid StopTasks()
        {
            UniTaskUtil.Instance.StopUniTask(nameof(InitialSmoothProgress));
            UniTaskUtil.Instance.StopUniTask(nameof(SmoothProgress));

            // 等待一帧，确保任务已停止
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            transform.SetAsFirstSibling();
            gameObject.SetActive(false);
            simulatedProgress = 1f;
            targetProgress = 1f;
            loadingSlider.value = 1f;
            loadingSlider.fillRect.gameObject.SetActive(true);
        }

        private async UniTask InitialSmoothProgress(CancellationToken token)
        {
            float initialTarget = 0.1f; // 初始模拟进度目标
            float initialSpeed = 0.05f; // 初始模拟进度速度

            while (simulatedProgress < initialTarget && !token.IsCancellationRequested)
            {
                simulatedProgress += initialSpeed * Time.deltaTime;
                loadingSlider.value = simulatedProgress;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private async UniTask SmoothProgress(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (simulatedProgress < targetProgress)
                {
                    float speed = Mathf.Max(0.01f, targetProgress - simulatedProgress) * 0.5f; // 调整速度使其更平滑
                    simulatedProgress += speed * Time.deltaTime;
                    simulatedProgress = Mathf.Min(simulatedProgress, targetProgress);
                }

                loadingSlider.value = simulatedProgress;

                if (actualProgress >= 1f && simulatedProgress >= 1f)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 确保进度条在下载完成时设置为100%
            if (needsUpdate)
            {
                simulatedProgress = 1f;
                loadingSlider.value = simulatedProgress;
                loadingTip.text = "下载完成!";
            }
        }
    }
}