using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPC : MonoBehaviour
{
    [Header("NPC 資料")]
    public NPCData data;
    [Header("對話區塊")]
    public GameObject panelDialog;
    [Header("名稱")]
    public Text textName;
    [Header("內容")]
    public Text textContent;
    [Header("打字速度"), Range(0.01f, 1)]
    public float printSpeed = 0.2f;
    [Header("打字音效")]
    public AudioClip soundPrint;
    [Header("任務區塊")]
    public RectTransform panelMission;
    [Header("任務數量")]
    public Text textMission;
    [Header("傳送門")]
    public GameObject[] doors;

    private AudioSource aud;
    private Animator ani;
    private Player player;

    public int count;

    public void UpdateTextMission()
    {
        count++;
        textMission.text = count + " / " + data.count;
    }

    /// <summary>
    /// 對話系統
    /// </summary>
    public void Dialog()
    {
        panelDialog.SetActive(true);
        textName.text = name;
        StartCoroutine(Print());
    }

    /// <summary>
    /// 取消對話
    /// </summary>
    public void cancelDialog()
    {
        panelDialog.SetActive(false);
        ani.SetBool("對話開關", false);
    }

    /// <summary>
    /// 打字效果
    /// </summary>
    /// <returns></returns>
    private IEnumerator Print()
    {
        AnimationControl();
        Missioning();

        player.stop = true;                              // 對話開始時禁止移動

        string dialog = data.dialogs[(int)data._NPCState];                 // 對話 = NPC 資料,對話第一段
        textContent.text = "";

        for (int i = 0; i < dialog.Length; i++)          // 跑對話第一個字到最後一個字
        {
            textContent.text += dialog[i];               // 對話內容,文字 += 對話[]
            aud.PlayOneShot(soundPrint, 0.2f);
            yield return new WaitForSeconds(printSpeed);
        }

        player.stop = false;                             // 對話結束時解除禁止移動

        NoMission();
    }

    /// <summary>
    /// 未接任務狀態切換為任務進行中 : 對話後執行
    /// </summary>
    private void NoMission()
    {
        // 如果狀態為未接任務 將狀態改為任務進行中
        if (data._NPCState == NPCState.NoMission)
        {
            data._NPCState = NPCState.Missioning;
            StartCoroutine(ShowMisson());
        }
    }

    private IEnumerator ShowMisson()
    {
        // 當任務區塊.X 大於 0 就插值跑到 0
        while (panelMission.anchoredPosition.x > 0)
        {
            panelMission.anchoredPosition = Vector3.Lerp(panelMission.anchoredPosition, new Vector3(0, 0, 0), 10 * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 任務進行中切換為任務完成 : 完成對話後執行
    /// </summary>
    private void Missioning()
    {
        // 如果 數量 等於 NPC 需求數量 將狀態改為任務完成
        if (count >= data.count) data._NPCState = NPCState.Finish;

        // 迴圈執行 將所有傳送門顯示
        for (int i = 0; i < doors.Length; i++) doors[i].SetActive(true);
    }

    private void AnimationControl()
    {
        if (data._NPCState == NPCState.NoMission || data._NPCState == NPCState.Missioning)
            ani.SetBool("對話開關", true);
        else
            ani.SetTrigger("感謝觸發");
    }

    private void Awake()
    {
        data._NPCState = NPCState.NoMission;              // 遊戲開始時將狀態改為未接任務

        aud = GetComponent<AudioSource>();
        ani = GetComponent<Animator>();

        player = FindObjectOfType<Player>();             // 透過類型尋找物件 *僅限場上只有一個類型    
    }

    // Enter 進入
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "玩家") Dialog();
    }

    // Exit 離開
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "玩家") cancelDialog();
    }
}
