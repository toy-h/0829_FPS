using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 목적: HP bar의 앞방향이 타겟의 앞 방향으로 향한다.
// 필요속성: 타겟
public class HpBarTarget : MonoBehaviour
{
    // 필요속성: 타겟
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        // 목적: HP bar의 앞방향이 타겟의 앞 방향으로 향한다.
        transform.forward = target.forward;
    }
}
