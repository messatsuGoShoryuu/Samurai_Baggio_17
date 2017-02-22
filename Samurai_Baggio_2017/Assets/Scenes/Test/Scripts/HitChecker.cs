using UnityEngine;
using System.Collections;

public class HitChecker : MonoBehaviour
{
    public delegate void HitFunction(Collider2D other);
    public string[] tags;
    public HitFunction[] functions;
    public void OnTriggerEnter2D(Collider2D other)
    {
        int l = tags.Length;
        for (int i = 0; i < l; i++)
        {
            if(tags[i] == other.tag)
                functions[i](other);
        }
    }
}
