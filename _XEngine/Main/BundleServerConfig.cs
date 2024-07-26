using UnityEngine;


namespace XEngine.Main
{
    public static class BundleServerConfig
    {
        public static readonly string ServerUrl = Application.platform switch
        {
            // don't forget to add '/'
            RuntimePlatform.WindowsEditor => @"https://aliyuncs.com/Windows/AssetBundle/",
            RuntimePlatform.OSXEditor => @"https://aliyuncs.com/MacOS/AssetBundle/",
            RuntimePlatform.Android => @"https://aliyuncs.com/Windows/AssetBundle/",
            RuntimePlatform.IPhonePlayer => @"https://aliyuncs.com/Windows/AssetBundle/",
            _ => "Server not configured for this platform",
        };
    }
}
