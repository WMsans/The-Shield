using UnityEngine.Scripting;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;
[Preserve]
public class SkipSplashImage
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Run()
    {
        Task.Run(() =>
        {
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        });
    }
}