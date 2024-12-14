using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

// This method allows you to delay an action or method call.
// To use it in another script, simply write: "this.Wait(5f, () => YourMethodOrAction());"
public static class WaitScript
{
    public static async void Wait(this MonoBehaviour mono, float delay, UnityAction action)
    {
        await Task.Delay((int)(delay * 1000));
        if (mono) action?.Invoke();
    }
}