using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region 欄位
    [Header("速度"), Range(0, 500)]
    public float speed = 1;
    [Header("旋轉速度"), Range(0, 1000)]
    public float turn = 1;
    [Header("撿取音效")]
    public AudioClip soundProp;

    [HideInInspector]
    /// <summary>
    /// 禁止移動
    /// </summary>
    public bool stop;

    [Header("傳送門:0 NPC, 1 殭屍")]
    public Transform[] doors;
    [Header("介面區塊")]
    public Image barHp;
    public Image barMp;
    public Image barExp;
    public Text textLv;
    [Header("流星雨")]
    public Transform stone;

    [HideInInspector]
    public float stoneDamage = 150;         // 技能傷害
    public float stoneCost = 50;            // 技能消耗魔力
    private float attack = 60;
    private float hp = 100;
    private float maxHp = 100;
    private float mp = 50;
    private float maxMp = 50;
    private float exp;
    private float maxExp = 100;            // 經驗值需求
    private int lv = 1;
    private float restoreMp = 10;          // 回魔/秒

    public float[] exps = new float[99];   // 經驗值需求表

    private Rigidbody rig;
    private Animator ani;
    private AudioSource aud;
    private Transform cam;   // 攝影機根物件
    private NPC npc;
    #endregion

    #region 事件
    private void Awake()
    {
        // 取得元件<泛型>();
        // 泛型:所有類型
        aud = GetComponent<AudioSource>();
        rig = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();
        cam = GameObject.Find("攝影機根物件").transform;

        npc = FindObjectOfType<NPC>();

        // 迴圈輸入每一級需要的經驗值
        for (int i = 0; i < exps.Length; i++) exps[i] = 50 * (i + i);
    }

    private void Update()
    {
        Attack();
        Skill();
        RestoreMp();
    }

    private void FixedUpdate()
    {
        if (stop) return;          // 如果 停止 跳出
        Move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "骷髏頭") GetProp(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "傳送門 - NPC")
        {
            transform.position = doors[1].position;                        // 傳送到 殭屍
            doors[1].GetComponent<CapsuleCollider>().enabled = false;      // 關閉 殭屍 傳送門碰撞
            Invoke("OpendoorZombie", 3);
        }
        if (other.name == "傳送門 - 殭屍")
        {
            transform.position = doors[0].position;                        // 傳送到 NPC
            doors[0].GetComponent<CapsuleCollider>().enabled = false;      // 關閉 NPC 傳送門碰撞
            Invoke("OpendoorNPC", 3);
        }

        // 如果 碰到物件的標籤 等於 殭屍
        if (other.tag == "殭屍")
        {
            other.GetComponent<Enemy>().Hit(attack, transform);
        }
    }
    #endregion

    #region 方法
    /// <summary>
    /// 開啟 NPC 傳送門碰撞
    /// </summary>
    private void OpendoorNPC()
    {
        doors[0].GetComponent<CapsuleCollider>().enabled = true;
    }

    /// <summary>
    /// 開啟 殭屍 傳送門碰撞
    /// </summary>
    private void OpendoorZombie()
    {
        doors[1].GetComponent<CapsuleCollider>().enabled = true;
    }

    /// <summary>
    /// 移動方法:前後左右移動與動畫
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");                              // 前後:WS 上下
        float h = Input.GetAxis("Horizontal");                            // 左右:AD 左右
        Vector3 pos = cam.forward * v + cam.right * h;                    // 移動座標 = 角色.前方 * 前後.右方 + 角色
        rig.MovePosition(transform.position + pos * speed);               // 移動座標(原本座標 + 移動座標 * 速度)

        ani.SetFloat("移動", Mathf.Abs(v) + Mathf.Abs(h));                // 設定浮點數(絕對值 v 與 h )

        if (v != 0 || h != 0)                                                            // 如果 控制中
        {
            pos.y = 0;                                                                   // 移動座標.y = 0
            Quaternion angle = Quaternion.LookRotation(pos);                             // B 角度 = 面向(移動座標)
            transform.rotation = Quaternion.Slerp(transform.rotation, angle, turn);      // A 角度 = 角物,插值(A 角度, B 角度, 旋轉角度)
        }
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ani.SetTrigger("攻擊觸發");
        }
    }

    /// <summary>
    /// 流星雨
    /// </summary>
    private void Skill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (stoneCost <= mp)
            {
                mp -= stoneCost;
                barMp.fillAmount = mp / maxMp;
                Vector3 pos = transform.forward * 4 + transform.up * 3.5f;
                Instantiate(stone, transform.position + pos, transform.rotation);
            }
        }
    }

    /// <summary>
    /// 取得道具
    /// </summary>
    /// <param name="prop"></param>
    private void GetProp(GameObject prop)
    {
        Destroy(prop);
        npc.UpdateTextMission();
        aud.PlayOneShot(soundProp, 0.05f);
    }
    /// <summary>
    /// 受傷:動畫,扣血與擊退
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="direction"></param>
    public void Hit(float damage, Transform direction)
    {
        hp -= damage;
        ani.SetTrigger("受傷觸發");
        rig.AddForce(direction.forward * 100 + direction.up * 150);     // 擊退朝怪物前方與上方

        hp = Mathf.Clamp(hp, 0, 99999);                                 // 夾住血量不低於 0
        barHp.fillAmount = hp / maxHp;                                  // 更新血條

        if (hp == 0) Dead();                                            // 如果血量等於 0 就死亡
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        ani.SetBool("死亡開關", true);      // 死亡動畫
        enabled = false;                    // 關閉此腳本
    }

    public void Exp(float getExp)
    {
        exp += getExp;
        barExp.fillAmount = exp / maxExp;

        while (exp >= maxExp && lv < exps.Length) LevelUp();    // 當 經驗值 >= 經驗值需求 就 持續升級
    }

    /// <summary>
    /// 升級
    /// </summary>
    private void LevelUp()
    {
        lv++;                  // 等級遞增
        maxHp += 20;           // 血量遞增
        maxMp += 25;           // 魔力遞增
        attack += 20;          // 攻擊遞增
        stoneDamage += 25;     // 技能攻擊遞增

        hp = maxHp;            // 恢復血量
        mp = maxMp;            // 恢復魔力
        exp -= maxExp;         // 扣掉最大經驗值保留多餘的經驗值

        maxExp = exps[lv - 1]; // 下一級最大經驗值

        barHp.fillAmount = 1;                // 血條全滿
        barMp.fillAmount = 1;                // 魔力全滿
        barExp.fillAmount = exp / maxExp;    // 更新經驗值介面
        textLv.text = "Lv " + lv;            // 更新等級介面
    }

    /// <summary>
    /// 回魔
    /// </summary>
    private void RestoreMp()
    {
        mp += restoreMp * Time.deltaTime;
        mp = Mathf.Clamp(mp, 0, maxMp);
        barMp.fillAmount = mp / maxMp;
    }

    #endregion
}
