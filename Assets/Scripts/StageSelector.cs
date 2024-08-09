using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    [SerializeField] GameObject menuItem;
    [SerializeField] GameObject parentGrid;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip touchSE;
    public static int SelectStageNo {  get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(NetworkManager.Instance.GetUserStage((userStages =>
        {
            int maxClearStage = 0;
            foreach (var stage in userStages)
            {
                maxClearStage = (maxClearStage > stage.StageID) ? maxClearStage : stage.StageID;
            }
            //クリアステージ+1まで表示
            for (int i = 1; i <= maxClearStage + 1; i++)
            {
                GameObject obj = Instantiate(menuItem, parentGrid.transform);
                Transform child = obj.transform.GetChild(0);
                child.GetComponent<Text>().text = i.ToString();
                int stageNo = i;
                obj.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    //ステージ移動
                    audioSource.PlayOneShot(touchSE);
                    SelectStageNo = stageNo;
                    Initiate.Fade("Game", Color.black, 1.0f, true);
                });
            }
        })));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
