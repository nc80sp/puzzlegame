using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] Slider loadingSlider;
    [SerializeField] Text loadingText;
    private List<AsyncOperationHandle<SceneInstance>> loadHandleList = new List<AsyncOperationHandle<SceneInstance>>();
    [SerializeField]
    private List<AssetReference> scene;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(loading());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator loading()
    {
        //更新処理
        var handle = Addressables.UpdateCatalogs();
        yield return handle;
        yield return new WaitForSeconds(1);

        //ダウンロード処理
        AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync("default", false);

        while (downloadHandle.Status == AsyncOperationStatus.None)
        {
            loadingSlider.value = downloadHandle.GetDownloadStatus().Percent * 100;

            yield return null;
        }
        loadingSlider.value = 100;
        Addressables.Release(downloadHandle); //Release the operation handle

        StartCoroutine(StartStage());
    }

    IEnumerator StartStage()
    {
        yield return new WaitForSeconds(0.5f);

        Initiate.Fade("SelectStage", Color.black, 1.0f);
    }
}
