using UnityEngine;
using System.Collections;

public class learnCoroutine : MonoBehaviour
{
    public Transform ming;

    public void Start()
    {
        // 啟動協程
        StartCoroutine(Test());
        StartCoroutine(Big());
    }

    // 定義協程
    // 傳回類型必須是 IEnumerator 傳回時間
    public IEnumerator Test()
    {
        print("協程");
        yield return new WaitForSeconds(2);     // 等待時間
        print("過了兩秒的協程");
        yield return new WaitForSeconds(1);     // 等待時間
        print("又過了一秒");
    }

    public IEnumerator Big()
    {
        for (int i = 0; i < 10; i++)
        {
            ming.localScale += Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
