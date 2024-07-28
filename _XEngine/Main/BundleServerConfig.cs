using UnityEngine;


namespace XEngine.Main
{
    public static class BundleServerConfig
    {
        public static readonly string ServerUrl = Application.platform switch
        {
            // don't forget to add '/'
            RuntimePlatform.WindowsEditor => @"https://darling-unity.oss-ap-southeast-1.aliyuncs.com/Windows/AssetBundle/",
            RuntimePlatform.OSXEditor => @"https://unityassetbundle.oss-cn-shanghai.aliyuncs.com/MacOS/AssetBundle/",
            RuntimePlatform.Android => @"https://darling-unity.oss-ap-southeast-1.aliyuncs.com/Windows/AssetBundle/",
            RuntimePlatform.IPhonePlayer => @"https://darling-unity.oss-ap-southeast-1.aliyuncs.com/Windows/AssetBundle/",
            _ => "Server not configured for this platform",
        };
    }
}
