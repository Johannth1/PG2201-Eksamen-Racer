using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BezierSolution;
using Helpers;
using UnityEngine.SceneManagement;

public class IntroScreenScript : MonoBehaviour {

  public GameObject panel;
  public GameObject startButton;
  public GameObject exitButton;
  public GameObject exitButton2;
  public GameObject restartButton;
  public GameObject cameraObj;
  public GameObject cameraPath;
  public GameObject player;
  public GameObject MainMenu;
  public GameObject Options;
  public GameObject GameOver;
  public GameObject Lasted;
  public GameObject Collected;

  private bool _isOpen = true; // @refactor to state enum
  private bool _isZoomedOut = true;
  private bool _isZooming = false;
  private bool _isGameOver = false;

  private Spring _panelPosition = new Spring(current: 0f, k: 0.09f, f: 0.7f);
  private RectTransform _panelTransform;
  private BezierSpline _cameraPath;
  private float _cameraDelta = 0f;
  private Vector3 _cameraPosition;
  private Vector3 _cameraPositionVelocity = Vector3.zero;
  private Quaternion _cameraRotation;
  private Transform _cameraPlayer;

	// Use this for initialization
	void Start () {
    startButton.GetComponent<Button>().onClick.AddListener(OnClickStart);
    exitButton.GetComponent<Button>().onClick.AddListener(OnClickExit);
    exitButton2.GetComponent<Button>().onClick.AddListener(OnClickExit);
    restartButton.GetComponent<Button>().onClick.AddListener(OnClickRestart);
    _panelTransform = panel.GetComponent<RectTransform>();
    _cameraPath = cameraPath.GetComponent<BezierSpline>();
    _cameraPosition = cameraObj.transform.position;
    _cameraRotation = cameraObj.transform.rotation;
    _cameraPlayer = player.transform.Find("Camera");
    _cameraPlayer.GetComponent<Camera>().enabled = false;
    player.SetActive(false);
    player.GetComponent<PlayerScript>().Pause();
	}
	
  void OnClickStart() {
    _isOpen = !_isOpen;
    _isZoomedOut = false;
    _isZooming = true;
    player.SetActive(true);
    StartCoroutine(EnsureCameraSwap());
  }

  // sometimes the bezier curve seems get fishy at the end
  IEnumerator EnsureCameraSwap() {
    yield return new WaitForSeconds(2.5f);
    if (_isZooming) SwapCameras();
  }

  void SwapCameras() {
    _isZooming = false;
    _cameraDelta = 1f;
    _cameraPlayer.GetComponent<Camera>().enabled = true;
    cameraObj.GetComponent<Camera>().enabled = false;
    player.GetComponent<PlayerScript>().Unpause();
  }

  void OnClickExit() {
    Application.Quit();
	}

  void OnClickRestart() {
    int scene = SceneManager.GetActiveScene().buildIndex;
    SceneManager.LoadScene(scene, LoadSceneMode.Single);
    Time.timeScale = 1;
  }

	// Update is called once per frame
	void Update () {
    if (Input.GetKeyUp(KeyCode.Escape) && !_isZoomedOut && !_isGameOver) {
      _isOpen = !_isOpen;
      if (_isOpen)
        player.GetComponent<PlayerScript>().Pause();
      else
        player.GetComponent<PlayerScript>().Unpause();
    }

    if (!_isZoomedOut) {
      startButton.GetComponentInChildren<Text>().text = "RESUME";
    }

    if (!_isZoomedOut && _isZooming && _cameraDelta <= 1.1f) {
      Vector3 position;
      if (_cameraDelta > 0.5f) {
        player.GetComponent<PlayerScript>().Unpause();
      }
      if (_cameraDelta < 0.97f) {
        position = _cameraPath.MoveAlongSpline(ref _cameraDelta, 120f * Time.deltaTime);
      } else {
        position = _cameraPlayer.transform.position;
      }
      _cameraPosition = Vector3.SmoothDamp(_cameraPosition, position, ref _cameraPositionVelocity, 0.2f);
      cameraObj.transform.position = _cameraPosition;
      cameraObj.transform.rotation = Quaternion.Lerp(_cameraRotation, Quaternion.identity, _cameraDelta);
      if (_cameraDelta >= 1f) {
        SwapCameras();
      }
    }

    float offset = _panelPosition.Update(Time.deltaTime, _isOpen ? 0f : -Screen.width/2f);
    _panelTransform.localPosition = new Vector3(-offset, 0f, 0f);
	}

  public void ShowGameOverScreen(float time, float coins) {
    MainMenu.SetActive(false);
    Options.SetActive(false);
    GameOver.SetActive(true);
    _isOpen = true;
    _isGameOver = true;
    float minutes = Mathf.Floor(time / 60f);
    float seconds = Mathf.RoundToInt(time % 60f);
    Lasted.GetComponent<Text>().text = string.Format("Your time was {0} minute{1} and {2} second{3}", minutes, minutes == 1f ? "" : "", seconds, seconds == 1f ? "" : "s");
    Collected.GetComponent<Text>().text = string.Format("You collected {0} coins", coins);
  }
}
