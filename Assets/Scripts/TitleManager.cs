using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject touchToStart;
    bool isStart = false;

    // Start is called before the first frame update
    void Start()
    {
        touchToStart.transform.DOScale(0.8f, 1.0f).SetLoops(-1, LoopType.Yoyo);
        touchToStart.GetComponent<SpriteRenderer>().DOFade(0, 2).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) && !isStart)
        {
            touchToStart.SetActive(false);
            isStart = true;
            StartCoroutine(startGame());
        }
    }

    IEnumerator startGame()
    {
        //�ŏ��ɃX�e�[�W�ꗗ�}�X�^�擾
        NetworkManager.Instance.LoadMasterData();

        NetworkManager.Instance.LoadUser();

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        var updates = checkHandle.Result;
        Addressables.Release(checkHandle);
        if (updates.Count >= 1)
        {
            //�X�V������ꍇ�̓��[�h��ʂ�
            Initiate.Fade("LoadStage", Color.black, 1);
        }
        else
        {
            //�X�V���Ȃ��ꍇ�̓X�e�[�W�I����ʂ�
            Initiate.Fade("SelectStage", Color.black, 1);
        }
    }
}
