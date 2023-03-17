using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoCaptureController : MonoBehaviour
{
    [SerializeField] private SystemMessage systemMessage;
    [SerializeField] private VideoPlayer videoPlayer;

    //キャプチャするカメラ　数は可変
    [SerializeField] private Camera[] cameras;

    //キャプチャする解像度 変更の際はカメラの設定も変更すること
    [SerializeField] private int resolutionWidth = 1920, resolutionHeight = 1080;

    //アプリケーションのフレームレート 負荷軽減のために60に設定
    [SerializeField] private int targetFrameRate = 60;

    //キャプチャ中か否かが変化したときに呼ばれるデリゲート
    public delegate void OnCaptureingChangeHandler(bool isCapturing);
    public event OnCaptureingChangeHandler OnCaptureingChange;

    //キャプチャするフレームレート
    public float captureFrameRate = 2;

    //キャプチャする水平視野角
    public int FOV = 95;

    //画像を保存するフォルダを作るディレクトリ
    public string parentDirectory = "";

    //画像を保存するフォルダ名兼プレフィックス
    [System.NonSerialized] public string folderName = "Images";

    //キャプチャする間隔のフレーム数
    private int frameInterval;

    //キャプチャした回数
    private int captureCount;

    //各キャプチャ回でキャプチャされた個数（最大値はカメラの数）
    private int eachTrialCount;

    //キャプチャする最大回数　終了判定に使用
    private int  maxCaptureCount;
    private bool _isCapturing = false, isCoroutineRunning = false;

    //最終的に画像を保存するパス
    private string finalPath;

    public bool IsCapturing
    {
        get => _isCapturing;
        set
        {
            _isCapturing = value;
            OnCaptureingChange?.Invoke(value);
        }
    }
    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        if(cameras.Length == 0)
        {
            Debug.LogError("No camera found!");
        }
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += OnFrameReady;

        //前回の設定を読み込む
        LoadSettings();
    }
    void Update() {
        if(!IsCapturing) return;

        //すべてのカメラでキャプチャが終わったら次に進む
        if(isCoroutineRunning && eachTrialCount == cameras.Length){
            isCoroutineRunning = false;
            eachTrialCount = 0;
            videoPlayer.frame += frameInterval;
        }
    }
    public void StartCaptureProcess()
    {
        if (videoPlayer.url == "")
        {
            systemMessage.ShowMessage("Please select a video file!");
            return;
        }
        if(parentDirectory == "")
        {
            systemMessage.ShowMessage("Please select a parent directory!");
            return;
        }
        
        //最終的な保存フォルダを準備
        finalPath = Path.Combine(parentDirectory, folderName + "_" + FOV);
        if (!Directory.Exists(finalPath))
        {
            Directory.CreateDirectory(finalPath);
        }

        //カメラの視野角を設定
        foreach(var _camera in cameras){
            _camera.fieldOfView = HorizontalToVerticalFov(FOV, _camera.aspect);
        }

        //初期化処理
        captureCount = 0;
        eachTrialCount = 0;
        IsCapturing = true;
        
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += StartPlayingVideo;        
    }
    private void StartPlayingVideo(VideoPlayer vp)
    {
        //キャプチャする間隔のフレーム数を計算
        frameInterval = (int)(vp.frameRate / captureFrameRate);
        //キャプチャする最大回数を計算
        maxCaptureCount = (int)(vp.frameCount / (ulong)frameInterval);

        vp.Play();
        vp.Pause();
        vp.frame = 0;

        vp.prepareCompleted -= StartPlayingVideo;
    }
    
    private void OnFrameReady(VideoPlayer vp,long frameIndex)
    {
        //最大キャプチャ回数に達したら終了
        if(captureCount >= maxCaptureCount){
            StopCaptureProcess();
            systemMessage.ShowMessage("Capture finished!");
        }

        //カメラごとに並行してキャプチャ
        for (int i = 0; i < cameras.Length; i++)
        {
            isCoroutineRunning = true;
            StartCoroutine(CaptureAndSaveImage(cameras[i], i));
        }
        captureCount++;
    }

    //実際の画像保存処理
    IEnumerator CaptureAndSaveImage(Camera cam, int index)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        string imageName = $"{folderName}_Cam{index}_{captureCount}.png";
        string path = Path.Combine(finalPath, imageName);
        File.WriteAllBytes(path, bytes);
        eachTrialCount++;
    }

    //VideoPlayerのURLを設定
    public void SetVideoPlayerURL(string url)
    {
        videoPlayer.url = "file://" + url;
    }

    //水平視野角から垂直視野角を計算
    private float HorizontalToVerticalFov(float horizontalFov, float aspectRatio)
    {
        return 2f * Mathf.Rad2Deg * Mathf.Atan(Mathf.Tan(horizontalFov * 0.5f * Mathf.Deg2Rad) / aspectRatio);
    }

    //キャプチャを終了する
    public void StopCaptureProcess()
    {
        videoPlayer.Stop();
        IsCapturing = false;
    }
    private void OnDestroy() {
        videoPlayer.frameReady -= OnFrameReady;
    }

    //設定を読み込む
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("CaptureFrameRate"))
        {
            captureFrameRate = PlayerPrefs.GetFloat("CaptureFrameRate");
        }
        if (PlayerPrefs.HasKey("FOV"))
        {
            FOV = PlayerPrefs.GetInt("FOV");
        }
        if(PlayerPrefs.HasKey("ParentDirectory"))
        {
            parentDirectory = PlayerPrefs.GetString("ParentDirectory");
        }
    }
}
