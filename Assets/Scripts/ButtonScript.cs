using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

// 목표: 플레이어가 버튼을 누르면 다리가 켜지고, 네비게이션 매시를 다시 만든다.
// 필요속성: 다리 게임오브젝트, navMeshSurface
public class ButtonScript : MonoBehaviour
{
    // 필요속성: 다리 게임오브젝트, navMeshSurface
    public GameObject bridge;
    public NavMeshSurface navMeshSurface;

    // Start is called before the first frame update
    void Start()
    {
        bridge.gameObject.SetActive(false);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        // 목표: 플레이어가 버튼을 누르면 다리가 켜지고, 네비게이션 매시를 다시 만든다.
        if (other.CompareTag("Player"))
        {
            // 다리가 켜진다.
            bridge.SetActive(true);

            // 네비게이션 매시를 다시 만든다.
            navMeshSurface.BuildNavMesh();
        }
    }
}
