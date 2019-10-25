using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Helpers;

public class CoinPileTriggerScript : TrackMeshMagnet {

  public enum State {
    Active = 0,
    Reloading
  }

  public Text timeLeftText;
  public Image timeLeftCircle;

  [HideInInspector]
  public State state = State.Active;

  private Transform _meshCoin;
  private Renderer _meshRenderer;
  private Color _meshColor;
  private Transform _cooldown;
  private Collider _collider;
  // private LineRenderer _line;
  private AudioSource _audioPling1;
  private AudioSource _audioPling2;

  private Vector3 _velocity = Vector3.zero;
  private Vector3 _addVelocity = Vector3.zero;
  // private PlayerScript _anchor = null;

  private float _spinning;
  private float _reloadTimer = 0f;
  private float _reloadTimerStart = 0f;
  private float _cooldownBobbing = 0f;
  private Spring _meshYPosition = new Spring(current: 0.6f);
  private Spring _meshAlphaColor = new Spring();

  void Start() {
    _meshCoin = transform.Find("Mesh");
    _meshRenderer = _meshCoin.GetComponent<Renderer>();
    _meshColor = _meshRenderer.material.GetColor("_Color");
    _cooldown = transform.Find("Cooldown");
    _meshCoin.Rotate(90f, 0f, 0f);
    _spinning = Random.value * 90f - 45f;
    _collider = GetComponent<Collider>();
    _audioPling1 = transform.Find("AudioPling1").gameObject.GetComponent<AudioSource>();
    _audioPling2 = transform.Find("AudioPling2").gameObject.GetComponent<AudioSource>();
    // _line = transform.Find("Line").gameObject.GetComponent<LineRenderer>();
    // _line.startWidth = 1f;
    // _line.endWidth = 2f;

    if (!ConnectToTrack()) {
      print("PANIC! CoinPileTriggerScript wasnt able to connect");
      print(transform.position);
      print(transform.parent.gameObject.name);
      Debug.Break();
    }
  }

  void Update() {
    _velocity *= 1f - (0.8f * Time.deltaTime);
    _velocity += _down * 0.98f * Time.deltaTime;
    Vector3 transfer = 0.5f * _addVelocity * Time.deltaTime;
    _addVelocity -= transfer;
    _velocity += transfer;
    _spinning *= 0.99f;

    if (_velocity.magnitude > 0f) {
      if (!MoveOnTrackMesh(position + (_rotation * _velocity) * Time.deltaTime)) {
        _velocity = Vector3.Reflect(_velocity, Vector3.right);
      }
    }
    transform.position = position;
    transform.rotation = _rotation;

    /*
    if (_anchor != null) {
      Vector3 offset = _anchor.position - transform.position;
      if (offset.magnitude >= 20f) {
        _line.enabled = false;
        _anchor = null;
      } else {
        _line.enabled = true;
        _line.SetPosition(0, transform.position + _up);
        _line.SetPosition(1, _anchor.positionDamped);
        // _velocity += 10f * offset  * Time.deltaTime;
        // _velocity += 10f * Ease.InCubic(offset.magnitude / 20f) * _anchor.velocity * Time.deltaTime;
        // _velocity +=  * 1.3f * _anchor.velocity * Time.deltaTime;
      }
    }
    */

    if (state == State.Reloading) {
      if (_reloadTimer <= 0f) {
        state = State.Active;
        _meshCoin.gameObject.SetActive(true);
        _cooldown.gameObject.SetActive(false);
        _collider.enabled = true;
        // cancel the bobbing up and down
      } else {
        if (_reloadTimerStart - _reloadTimer > 2f) {
          _cooldown.gameObject.SetActive(true);
        }
        _reloadTimer -= Time.deltaTime;
        timeLeftText.text = Mathf.Ceil(_reloadTimer).ToString();
        timeLeftCircle.fillAmount = _reloadTimer / _reloadTimerStart;
        _cooldownBobbing += 4f * Time.deltaTime;
        // bob the cooldown circle up/down a little
        float px = _cooldown.localPosition.x;
        float py = 0.4f * Ease.InOutQuad(Mathf.Abs(_reloadTimer - Mathf.Floor(_reloadTimer) - 0.5f) * 2f);
        float pz = _cooldown.localPosition.y;
        _cooldown.localPosition = new Vector3(px, py, pz);
        _meshCoin.localPosition = new Vector3(0f, _meshYPosition.Update(Time.deltaTime, 10f), 0f);
        _meshColor.a = _meshAlphaColor.Update(Time.deltaTime, 0f);
        _meshRenderer.material.SetColor("_Color", _meshColor);
      }
    } else {
      float px = _cooldown.localPosition.x;
      float py = 0f;
      float pz = _cooldown.localPosition.y;
      _cooldown.localPosition = new Vector3(px, py, pz);
      _meshCoin.localPosition = new Vector3(0f, Mathf.Abs(_meshYPosition.Update(Time.deltaTime, 0f))+1f, 0f);
      _meshColor.a = _meshAlphaColor.Update(Time.deltaTime, 1f);
      _meshRenderer.material.SetColor("_Color", _meshColor);
    }

    // spin the coin (its rotate y on the z axis)
    float ry = (Mathf.Sign(_spinning) * 20f + _spinning) * Time.deltaTime;
    _meshCoin.Rotate(0f, 0f, ry);
  }

  public void StartLoading(float time) {
    state = State.Reloading;
    _reloadTimerStart = _reloadTimer = time;
    _collider.enabled = false;
    if (Random.value >= 0.75)
      _audioPling2.Play();
    else
      _audioPling1.Play();
  }

  void AddSpin(Collider other, float multiplier) {
    if (other.gameObject.tag == "Player") {
      PlayerScript player = other.transform.parent.GetComponent<PlayerScript>();
      _addVelocity += 0.75f * player.velocity * multiplier;
      Vector3 offset = transform.position - player.position;
      _spinning = 40f * player.velocity.z * -Mathf.Sign(Vector3.Dot(player.right, offset)) * multiplier;
    }
  }

  void OnTriggerEnter(Collider other) {
    AddSpin(other, 1.2f);
    // _anchor = other.transform.parent.GetComponent<PlayerScript>();
  }

  void OnTriggerExit(Collider other) {
    AddSpin(other, 2f);
  }
}
