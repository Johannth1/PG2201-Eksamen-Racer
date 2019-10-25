using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Helpers;
using BezierSolution;

public class PlayerScript : MonoBehaviour, CoinStackable {

  public Text textBox1;
  public Text textBox2;
  public Image timeLeftCircle;
  public GameObject introScreen;

  public Vector3 position {
    get { return _camera.playerPosition; }
  }
  public Vector3 positionDamped {
    get { return _camera.playerPositionDamped; }
  }
  public Vector3 velocity {
    get { return _camera.velocity; }
  }
  public Vector3 forward {
    get { return _camera.forward; }
  }
  public Vector3 right {
    get { return _camera.right; }
  }

  private TrailCamera _camera;
  private Transform _ball;
  private Transform _pivot;
  // private Transform _light;
  private Transform _hud;
  private Transform _coin;
  // private Image _timeLeftCircle;

  private float _totalTime = 0f;
  private float _numCoins = 0f;
  private float _time = 10f;
  private float _momentum = 0f;

  // private Spring _xRotation = new Spring(k: 0.12f, f: 0.9f);
  // private Spring _zRotation = new Spring(k: 0.12f, f: 0.9f);
  private Spring _duration = new Spring(k: 0.06f, f: 0.8f, current: 0f);

  private bool _paused = false;

  void Start() {
    _camera = GetComponent<TrailCamera>();
    _ball = transform.Find("BallMesh");
    _pivot = transform.Find("BallPivot");
    // _light = _pivot.Find("PointLight");
    _hud = transform.Find("HUD");
    // _timeLeftCircle = timeLeftCircle.GetComponent<Image>();
  }

  void FixedUpdate() {
    if (_paused) return;
    _camera.DoFixedUpdate();
  }

  void Update() {
    if (_paused) return;
    _camera.DoUpdate();

    _time -= Time.deltaTime;
    _totalTime += Time.deltaTime;

    if (_time <= 0f) {
      Debug.Log("Game over");
      Pause();
      introScreen.GetComponent<IntroScreenScript>().ShowGameOverScreen(_totalTime, _numCoins);
      return;
    }

    transform.position = _camera.playerPositionDamped;
    _pivot.rotation = _camera.rotation;
    _hud.rotation = Quaternion.LookRotation(_camera.forward, _camera.up);

    // Spin the mesh representing the ball, in the direction were traveling
    _ball.Rotate(_camera.rotation * new Vector3(_camera.velocity.z * 45f * Time.deltaTime, 0f, _camera.velocity.x * -45f * Time.deltaTime), Space.World);

    /*
    float x = _xRotation.Update(Time.deltaTime, _camera.velocity.x);
    float z = _zRotation.Update(Time.deltaTime, _camera.velocity.z);
    if (_coin != null) {
      _coin.gameObject.GetComponent<CoinScript>().UpdateCoin(new Vector3(x, 0f, z), 1f, (float)_numCoins);
    }
    */

    textBox1.text = Mathf.Ceil(_time).ToString();
    textBox2.text = Mathf.Floor(_camera.velocity.magnitude).ToString();
    timeLeftCircle.fillAmount = _duration.Update(Time.deltaTime, Mathf.Clamp(_time / 30f, 0f, 1f));
    if (_time < 3f) {
      float t = 0.4f + 0.6f * Ease.InOutQuad(Mathf.Abs(_time - Mathf.Floor(_time) - 0.5f) * 2f);
      Color color = new Color(1f-t*0.5f, 1, 1, t);
      timeLeftCircle.color = color;
      textBox1.color = color;
    } else {
      timeLeftCircle.color = Color.white;
    }
  }

  void LateUpdate() {
    if (_paused) return;
    _camera.DoLateUpdate();
  }

  public void Pause() {
    _paused = true;
  }

  public void Unpause() {
    _paused = false;
  }

  public void UpdateCoin(Vector3 velocity, float index, float numCoins) {}

  public Vector3 CoinVelocity(Vector3 velocity) {
    return _camera.velocity;
  }

  public void StackCoin(Transform coin, CoinStackable parent = null) {
    _time = Mathf.Min(_time + 10f, 30f);
    _numCoins++;
    // _momentum += 1f;
    /*
    _numCoins += 1f;
    Transform current = _coin;
    _coin = coin;
    coin.SetParent(_pivot);
    _coin.gameObject.GetComponent<CoinScript>().StackCoin(current, this);
    */
  }

  public void DropCoin(Collider collider, RaycastHit hit) {
    // _numCoins -= 1f;
  }
}
