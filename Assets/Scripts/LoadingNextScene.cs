using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 목표: 다음 씬을 비동기 방식으로 로드하고 싶다.
// 필요속성: 다음 진행할 씬 번호
// 목표2: 현재 씬에 로딩 진행률을 슬라이더로 표현하고 싶다.
// 필요속성: 로딩 슬라이더, 로딩텍스트
public class LoadingNextScene : MonoBehaviour
{
    // 필요속성: 다음 진행할 씬 번호
    public int sceneNumber = 2;

    // 필요속성: 로딩 슬라이더, 로딩텍스트
    public Slider loadingSlider;
    public TMP_Text loadingText;

    private void Start()
    {
        // 비동기 씬을 코루틴 함수로 로드한다.
        StartCoroutine(AsyncNextScene(sceneNumber));
    }

    // 목표: 다음 씬을 비동기 방식으로 로드하고 싶다.
    IEnumerator AsyncNextScene(int num)
    {
        // 지정된 씬을 비동기 방식으로 만들고 싶다.
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(num);

        // 씬이 화면에 보이지 않게 하고 싶다.
        asyncOperation.allowSceneActivation = false;

        // 목표2: 현재 씬에 로딩 진행률을 슬라이더로 표현하고 싶다.
        while(!asyncOperation.isDone) 
        {
            loadingSlider.value = asyncOperation.progress;
            loadingText.text = (asyncOperation.progress * 100).ToString() + "%";

            // 특정 진행률일 때 다음 씬을 보여주고 싶다.
            if(asyncOperation.progress >= 0.9f)
            {
                // 씬이 화면에 보이게 하고 싶다.
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
