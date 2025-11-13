using UnityEngine;
using UnityEngine.Events;

public class Action : MonoBehaviour
{
    public UnityEvent OnInitiate;

    public void Initiate()
    {
        OnInitiate?.Invoke();
    }
}
