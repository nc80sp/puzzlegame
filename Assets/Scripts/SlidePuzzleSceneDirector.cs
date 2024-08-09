using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlidePuzzleSceneDirector : MonoBehaviour
{
    // ピース
    List<GameObject> pieces;
    // ゲームクリア時に表示されるボタン
    [SerializeField] GameObject buttonBack;
    // リトライボタン
    [SerializeField] GameObject buttonRetry;
    // シャッフル回数
    [SerializeField] int shuffleCount;
    // 開始カウントダウン
    [SerializeField] Text countDownText;
    // ゲーム中
    bool isPlaying = false;
    // タイマー
    float timer = 0.0f;
    // タイマーテキスト
    [SerializeField] Text timerText;
    // タイマーテキスト
    [SerializeField] Text limitTimerText;
    // 初期位置
    List<Vector2> startPositions;
    // クリア時エフェクト
    [SerializeField] GameObject clearEffect;
    // 結果表示テキスト
    [SerializeField] GameObject clearText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] Text newRecordText;
    [SerializeField] Text myRecordText;
    [SerializeField] Text worldRecordText;
    StageRecordResponse records;

    //BGM
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource seAudioSource;
    [SerializeField] AudioClip gameOverJingle;
    [SerializeField] AudioClip gameClearJingle;
    [SerializeField] AudioClip panelMoveSE;
    [SerializeField] AudioClip countDown1SE;
    [SerializeField] AudioClip countDown2SE;
    [SerializeField] AudioClip gameClearEffectSE;
    [SerializeField] AudioClip buttonSE;

    // Start is called before the first frame update
    void Start()
    {
        // ボタン非表示
        buttonBack.SetActive(false);
        buttonRetry.SetActive(false);
        clearText.SetActive(false);
        gameOverText.SetActive(false);

        // テキスト初期化
        countDownText.text = "";
        timerText.text = timer.ToString("00.000");
        limitTimerText.text = "/" +
            NetworkManager.Instance.stages[StageSelector.SelectStageNo - 1].LimitTime.ToString("00.000");
        newRecordText.enabled = false;
        myRecordText.text = "";
        worldRecordText.text = "";
        StartCoroutine(NetworkManager.Instance.GetStageRecord(StageSelector.SelectStageNo, response =>
        {
            records = response;
            myRecordText.text = (records.MyRecord == 0) ? "--.---" : records.MyRecord.ToString("00.000");
            worldRecordText.text = (records.WorldRecord == 0) ? "--.---" : records.WorldRecord.ToString("00.000");

            //ステージ読み込み
            Addressables.LoadSceneAsync("Stage" + StageSelector.SelectStageNo.ToString(), LoadSceneMode.Additive).Completed += op =>
            {
                pieces = new List<GameObject>(GameObject.FindGameObjectsWithTag("Piece"));
                pieces.Sort((a, b) => string.Compare(a.name, b.name));

                // 初期位置を保存
                startPositions = new List<Vector2>();
                foreach (var item in pieces)
                {
                    startPositions.Add(item.transform.position);
                }

                // 指定回数シャッフル
                for (int i = 0; i < shuffleCount; i++)
                {
                    // 0番と隣接するピース
                    List<GameObject> movablePieces = new List<GameObject>();

                    // 0番と隣接するピースをリストに追加
                    foreach (var item in pieces)
                    {
                        if (GetEmptyPiece(item) != null)
                        {
                            movablePieces.Add(item);
                        }
                    }

                    // 隣接するピースをランダムで入れかえる
                    int rnd = Random.Range(0, movablePieces.Count);
                    GameObject piece = movablePieces[rnd];
                    SwapPiece(piece, pieces[0]);
                }

                StartCoroutine(StartCountDown());
            };
        }));
    }

    IEnumerator StartCountDown()
    {
        yield return new WaitForSeconds(1.0f);

        countDownText.text = "3";
        seAudioSource.PlayOneShot(countDown1SE);
        yield return new WaitForSeconds(1.0f);

        countDownText.text = "2";
        //seAudioSource.PlayOneShot(countDown1SE);
        yield return new WaitForSeconds(1.0f);

        countDownText.text = "1";
        //seAudioSource.PlayOneShot(countDown1SE);
        yield return new WaitForSeconds(1.0f);

        //seAudioSource.PlayOneShot(countDown2SE);
        countDownText.gameObject.SetActive(false);
        isPlaying = true;
        buttonRetry.SetActive(true);
        buttonBack.SetActive(true);

        bgmAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= NetworkManager.Instance.stages[StageSelector.SelectStageNo - 1].LimitTime)
        {
            timer = NetworkManager.Instance.stages[StageSelector.SelectStageNo - 1].LimitTime;

            // ゲームオーバー処理
            isPlaying = false;
            gameOverText.SetActive(true);
            buttonBack.SetActive(true);
            bgmAudioSource.Stop();
            seAudioSource.PlayOneShot(gameOverJingle);
        }
        timerText.text = timer.ToString("00.000");

        // タッチ処理
        if (Input.GetMouseButtonUp(0))
        {
            // スクリーン座標からワールド座標に変換
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // レイを飛ばす
            RaycastHit2D hit2d = Physics2D.Raycast(worldPoint, Vector2.zero);

            // 当たり判定があった
            if(hit2d)
            {
                seAudioSource.PlayOneShot(panelMoveSE);
                // ヒットしたゲームオブジェクト
                GameObject hitPiece = hit2d.collider.gameObject;
                // 0番のピースと隣接していればデータが入る
                GameObject emptyPiece = GetEmptyPiece(hitPiece);
                // 選んだピースと0番のピースを入れかえる
                SwapPiece(hitPiece, emptyPiece);

                // クリア判定
                bool isClear = true;
                buttonBack.SetActive(true);

                // 正解の位置と違うピースを探す
                for (int i = 0; i < pieces.Count; i++)
                {
                    // 現在のポジション
                    Vector2 position = pieces[i].transform.position;
                    // 初期位置と違ったらボタンを非表示
                    if(position != startPositions[i])
                    {
                        isClear = false;
                    }
                }

                // クリア状態
                if(isClear)
                {
                    Debug.Log("クリア！！");
                    isPlaying = false;
                    bgmAudioSource.Stop();
                    seAudioSource.PlayOneShot(gameClearJingle);
                    StartCoroutine(DisplayClearEffect());
                    clearText.SetActive(true);
                    if (records.MyRecord > timer)
                    {
                        newRecordText.enabled = true;
                        newRecordText.GetComponent<Text>().DOFade(0, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                    }
                    StartCoroutine(NetworkManager.Instance.SendUserStage(StageSelector.SelectStageNo, timer, response =>
                    {
                        //送信後処理（何かあれば）
                    }));
                }
            }
        }
    }

    IEnumerator DisplayClearEffect()
    {
        while(true)
        {
            float waitSec = Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(waitSec);
            float x = Random.Range(0, 2) - 1.0f;
            float y = Random.Range(1, 4) - 1.0f;
            Instantiate(clearEffect, new Vector3(x,y,-1),Quaternion.identity);
            seAudioSource.PlayOneShot(gameClearEffectSE);
        }
    }

    // 引数のピースが0番のピースと隣接していたら0番のピースを返す
    GameObject GetEmptyPiece(GameObject piece)
    {
        // 2点間の距離を代入
        float dist =
            Vector2.Distance(piece.transform.position, pieces[0].transform.position);

        // 距離が1なら0番のピースを返す（2個以上離れていたり、斜めの場合は1より大きい距離になる）
        if (dist == 1)
        {
            return pieces[0];
        }

        return null;
    }

    // 2つのピースの位置を入れかえる
    void SwapPiece(GameObject pieceA, GameObject pieceB)
    {
        // どちらかがnullなら処理をしない
        if (pieceA == null || pieceB == null)
        {
            return;
        }

        // AとBのポジションを入れかえる
        Vector2 position = pieceA.transform.position;
        pieceA.transform.position = pieceB.transform.position;
        pieceB.transform.position = position;
    }

    // リトライボタン
    public void OnClickRetry()
    {
        Initiate.Fade("Game", Color.black, 3.0f);
        seAudioSource.PlayOneShot(buttonSE);
    }

    // 戻るボタン
    public void OnClickBack()
    {
        Initiate.Fade("SelectStage", Color.black, 1.5f);
        seAudioSource.PlayOneShot(buttonSE);
    }
}
