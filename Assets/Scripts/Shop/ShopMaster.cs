using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopMaster : Singleton<ShopMaster>
{
    public static string SAVE_PATH
    {
        get { return Application.persistentDataPath + "/shopdata"; }
    }

    #region Inspector
    [Header("Scene Stuff")]
    [SerializeField]
    private SetOrthographicsAsAABBSize mainCamera;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI starsCountText;
    [SerializeField]
    private Canvas fadeCanvas;
    [SerializeField]
    private CanvasGroup fadeCanvasAlpha;

    [Header("Prefabs")]
    [SerializeField]
    private ShopBallItem prefabBallItem;

    [Header("Audio")]
    [SerializeField]
    private AudioSettings notEnoughStarsSound;
    [SerializeField]
    private AudioSettings skinUnlockedSound;

    [Header("Skins")]
    [SerializeField]
    private BallSkinsObject freeSkins;
    [SerializeField]
    private BallSkinsObject starSkins;

    [Header("Settings")]
    [SerializeField]
    private int defaultStarCost = 5;
    [SerializeField]
    private float inputSensitivity = 1.0f;

    [Header("Extra Settings")]
    [SerializeField]
    private Vector2 spacing = new Vector2(0.1f, 0.2f);
    [SerializeField]
    private float sidesSpacing = 0.5f;
    [SerializeField]
    private float topSpacing = 0.5f;
    #endregion

    public AudioSettings NotEnoughtStarsSound
    {
        get { return notEnoughStarsSound; }
    }
    public AudioSettings SkinUnlockedSound
    {
        get { return skinUnlockedSound; }
    }

    private List<string> unlockedSkinsIDs = new List<string>();

    public ShopBallItem LastSelectedItem
    {
        get;
        set;
    }

    public bool IgnoreInput
    {
        get;
        private set;
    } = false;

    private float worldTop;
    private float worldBottom;
    private Vector3 inputReference;
    protected override void SingletonAwake()
    {
        Load();
        foreach (BallSkinScriptable skin in freeSkins.Skins)
        {
            if (!IsItemUnlocked(skin.ID))
                unlockedSkinsIDs.Add(skin.ID);
        }
        Save();
    }
    protected override void SingletonDestroy()
    {
        
    }
    private void Start()
    {
        StartCoroutine(FadeInCoroutine());

        UpdateStarsText();

        Vector3 bl = CalcBL();
        Vector3 tr = CalcTR();

        Vector3 extents = prefabBallItem.MaxAABB.extents;

        Vector3 position = tr;
        position.y -= extents.y;

        string lastID = Ball.GetLastSkinID();

        // not doing gui cz need to render sprites on top 
        position = CreateBallItems(freeSkins, 0, bl, tr, position, extents, lastID);
        position.y -= extents.y * 2.0f + spacing.y;
        position = CreateBallItems(starSkins, defaultStarCost, bl, tr, position, extents, lastID);

        worldBottom = position.y - extents.y;
        worldTop = mainCamera.transform.position.y + mainCamera.Camera.orthographicSize;

        Debug.DrawLine(new Vector3(-1.0f, worldBottom), new Vector3(-1.0f, worldTop), Color.yellow, 30.0f);
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            inputReference = Input.mousePosition;
        }
        else if(Input.GetMouseButton(0))
        {
            Vector3 input = Input.mousePosition;

            Vector3 worldInputReference = mainCamera.Camera.ScreenToWorldPoint(inputReference);
            Vector3 worldInput = mainCamera.Camera.ScreenToWorldPoint(input);

            float offset = (worldInputReference.y - worldInput.y) * inputSensitivity;

            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.y += offset;
            mainCamera.transform.position = cameraPosition;

            AlignCameraWithBottom(worldBottom);
            AlignCameraWithTop(worldTop);

            inputReference = input;
        }
    }

    private void AlignCameraWithBottom(float compareBottom)
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        float ortho = mainCamera.Camera.orthographicSize;

        float currentBottom = cameraPosition.y - ortho;
        if(currentBottom < compareBottom)
        {
            cameraPosition.y += compareBottom - currentBottom;
            mainCamera.transform.position = cameraPosition;
        }
    }
    private void AlignCameraWithTop(float compareTop)
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        float ortho = mainCamera.Camera.orthographicSize;

        float currentTop = cameraPosition.y + ortho;
        if(currentTop > compareTop)
        {
            cameraPosition.y += compareTop - currentTop;
            mainCamera.transform.position = cameraPosition;
        }
    }

    private Vector3 CreateBallItems(BallSkinsObject skins, int cost,
        Vector3 bottomLeft, Vector3 topRight,
        Vector3 position, Vector3 extents,
        string lastID)
    {
        position.x = bottomLeft.x;

        foreach (BallSkinScriptable i in skins.Skins)
        {
            ShopBallItem item = Instantiate(prefabBallItem);
            item.SetItem(i, cost, IsItemUnlocked(i.ID));
            item.RemoveSelection();
            if(i.ID.Equals(lastID))
            {
                item.SelectThisItem(false);
            }

            position.x += extents.x;

            Vector3 right = position;
            right.x += extents.x;

            if (right.x > topRight.x)
            {
                // make new line if intersecting with right wall
                position.x = bottomLeft.x + extents.x;
                position.y -= spacing.y + extents.y * 2.0f;
            }

            item.transform.position = position;
            position.x += extents.x + spacing.x;
        }

        return position;
    }
  
    private void Save()
    {
        string path = SAVE_PATH;
        using (StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode))
        {
            foreach (string id in unlockedSkinsIDs)
            {
                sw.WriteLine(id);
            }
        }
    }
    private void Load()
    {
        unlockedSkinsIDs.Clear();

        string path = SAVE_PATH;
        if (!File.Exists(path))
            return;
        using (StreamReader sr = new StreamReader(path, Encoding.Unicode))
        {
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                    break;

                unlockedSkinsIDs.Add(line);
            }

        }
    }
    public bool IsItemUnlocked(string id)
    {
        foreach (string s in unlockedSkinsIDs)
        {
            if (s.Equals(id))
                return true;
        }
        return false;
    }
    public void SaveUnlockedSkin(BallSkinScriptable skin)
    {
        unlockedSkinsIDs.Add(skin.ID);
        Save();
    }

    private Vector3 CalcBL()
    {
        float h = mainCamera.CalcCurrentSize() * 0.5f;
        float v = mainCamera.Camera.orthographicSize;

        Vector3 bl = mainCamera.transform.position - new Vector3(h, v);
        bl.x += sidesSpacing;
        bl.z = 0.0f;

        return bl;
    }
    private Vector3 CalcTR()
    {
        float h = mainCamera.CalcCurrentSize() * 0.5f;
        float v = mainCamera.Camera.orthographicSize;

        Vector3 tr = mainCamera.transform.position + new Vector3(h, v);
        tr.x -= sidesSpacing;
        tr.y -= topSpacing;
        tr.z = 0.0f;

        return tr;
    }

    public void SwitchToGameScene()
    {
        StartCoroutine(SwitchToGameSceneCoroutine(0.2f));
    }

    public void UpdateStarsText()
    {
        starsCountText.text = UIMaster.GetStarsCount().ToString();
    }

    #region Coroutines
    private IEnumerator FadeInCoroutine()
    {
        fadeCanvas.gameObject.SetActive(true);
        yield return FadeCoroutine(1.0f, 0.0f);
        fadeCanvas.gameObject.SetActive(false);
    }
    private IEnumerator SwitchToGameSceneCoroutine(float time)
    {
        IgnoreInput = true;

        fadeCanvas.gameObject.SetActive(true);

        yield return FadeCoroutine(0.0f, 1.0f);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
    private IEnumerator FadeCoroutine(float from, float to)
    {
        yield return this.Lerp(0.2f, from, to, Mathf.Lerp, fadeCanvasAlpha.SetAlpha);
    }
    #endregion

    #region Editor
    private void OnDrawGizmos()
    {
        Vector3 tr = CalcTR();
        Vector3 bl = CalcBL();

        Vector3 br = new Vector3(tr.x, bl.y, 0.0f);
        Vector3 tl = new Vector3(bl.x, tr.y, 0.0f);

        Gizmos.color = Color.green;

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
    #endregion
}

