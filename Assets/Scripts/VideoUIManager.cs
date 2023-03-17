using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class VideoUIManager : MonoBehaviour
{
    [SerializeField] private VideoCaptureController videoCaptureController;
    [SerializeField] private Button openVideoButton, startButton, selectParentDirectoryButton, stopButton;
    [SerializeField] private TMP_InputField fovInputField, frameRateInputField;
    [SerializeField] private TMP_Text parentDirectoryText, videoPathText;

    //値が入ってないときに表示するテキスト
    [SerializeField] private GameObject directoryPlaceholder, videoPathPlaceholder;
    void Start()
    {
        openVideoButton.onClick.AddListener(ShowLoadVideoDialog);
        startButton.onClick.AddListener(videoCaptureController.StartCaptureProcess);
        selectParentDirectoryButton.onClick.AddListener(ShowSaveDirectoryDialog);
        stopButton.onClick.AddListener(videoCaptureController.StopCaptureProcess);
    
        stopButton.interactable = false;

        fovInputField.onValueChanged.AddListener(SetFOV);
        frameRateInputField.onValueChanged.AddListener(SetFrameRate);

        //キャプチャ中かどうかのイベント登録
        videoCaptureController.OnCaptureingChange += OnCaptureingChange;

        //VideoCaptureControllerの値をUIに反映 Awakeの後なのでPlayerPrefsから読み込み済み
        parentDirectoryText.text = videoCaptureController.parentDirectory;
        fovInputField.text = videoCaptureController.FOV.ToString();
        frameRateInputField.text = videoCaptureController.captureFrameRate.ToString();

        //値が入ってたらプレースホルダーを消す
        if(!string.IsNullOrEmpty(parentDirectoryText.text)){
            directoryPlaceholder.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        videoCaptureController.OnCaptureingChange -= OnCaptureingChange;
    }

    //動画を選択するダイアログを表示
    private void ShowLoadVideoDialog()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("SelectVideo", "", "mp4", false);
        if(path.Length > 0) LoadVideo(path[0]);
    }

    //動画のパスを受け取っていろいろやる
    private void LoadVideo(string path)
    {
        videoCaptureController.SetVideoPlayerURL(path);
        videoPathText.text = path;
        //動画ファイル名をフォルダ名にする
        videoCaptureController.folderName = ExtractFileNameFromPath(path);
        if(!string.IsNullOrEmpty(videoPathText.text)){
            videoPathPlaceholder.SetActive(false);
        }
    }

    //保存先を選択するダイアログを表示
    private void ShowSaveDirectoryDialog()
    {
        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
        if(path.Length == 0) return;
        videoCaptureController.parentDirectory = path[0];
        parentDirectoryText.text = path[0];
        PlayerPrefs.SetString("ParentDirectory", path[0]);
        if(!string.IsNullOrEmpty(parentDirectoryText.text)){
            directoryPlaceholder.SetActive(false);
        }
    }
    private void SetFOV(string value)
    {
        videoCaptureController.FOV = int.Parse(value);
        PlayerPrefs.SetInt("FOV", int.Parse(value));
    }
    private void SetFrameRate(string value)
    {
        videoCaptureController.captureFrameRate = float.Parse(value);
        PlayerPrefs.SetFloat("CaptureFrameRate", float.Parse(value));
    }

    //キャプチャ中はStop以外のボタンを押せないようにする
    private void OnCaptureingChange(bool isCapturing)
    {
        startButton.interactable = !isCapturing;
        stopButton.interactable = isCapturing;

        fovInputField.interactable = !isCapturing;
        frameRateInputField.interactable = !isCapturing;

        openVideoButton.interactable = !isCapturing;
        selectParentDirectoryButton.interactable = !isCapturing;
    }
    private string ExtractFileNameFromPath(string path)
    {
        string fileNameWithExtension = Path.GetFileName(path);
        string fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
        return fileName;
    }
}
