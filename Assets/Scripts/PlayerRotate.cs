using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    // 필요속성: 마우스 입력 X, Y, 속도
    public float speed = 200f;

    // Update is called once per frame
    void Update()
    {
        // 목적7: GameManager가 Ready 상태일 때는 플레이어, 적이 움직일 수 없도록 한다.
        if (GameManager.Instance.state != GameManager.GameState.Start)
            return;

        // 순서1. 사용자의 마우스 입력(X, Y)을 받는다.
        float mouseX = Input.GetAxis("Mouse X");

        // 순서2. 마우스 입력에 따라 회전 방향을 설정한다.
        Vector3 dir = new Vector3(0, mouseX, 0);

        // 순서3. 물체를 회전시킨다.
        // r = r0 + vt
        transform.eulerAngles = transform.eulerAngles + dir * speed * Time.deltaTime;
    }
}
