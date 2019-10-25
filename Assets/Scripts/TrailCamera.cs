using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;

public class TrailCamera : MonoBehaviour {

  static int MASK_TRACK = 1 << 8;

  [HideInInspector] public Vector3 playerPosition = Vector3.zero;
  [HideInInspector] public Vector3 playerPositionDamped = Vector3.zero;
  [HideInInspector] public Vector3 cameraPosition = Vector3.zero;
  [HideInInspector] public Vector3 cameraPositionDamped = Vector3.zero;
  [HideInInspector] public Vector3 velocity = Vector3.zero;
  [HideInInspector] public Vector3 up = Vector3.up;
  [HideInInspector] public Vector3 upDamped = Vector3.up;
  [HideInInspector] public Vector3 forward = Vector3.forward;
  [HideInInspector] public Vector3 forwardDamped = Vector3.forward;
  [HideInInspector] public Vector3 right = Vector3.right;
  [HideInInspector] public Quaternion rotation = Quaternion.identity;

  private Transform _camera;
  private TrackScript _track;
  private GameObject _trackObject;
  private int _triangleIndex;
  private Mesh _mesh;
  private RaycastHit _hit;

  private Vector3 _dampPlayer = Vector3.zero;
  private Vector3 _dampCamera = Vector3.zero;
  private Vector3 _dampForward = Vector3.zero;
  private Vector3 _dampUp = Vector3.zero;

	void Start () {
    if (!Physics.Raycast(transform.position, up * -1f, out _hit, 10f, MASK_TRACK)) {
      print("PANIC!");
      Debug.Break();
    }
    _track = _hit.collider.gameObject.GetComponent<TrackScript>();
    MeshCollider meshCollider = (MeshCollider) _hit.collider;
    _mesh = meshCollider.sharedMesh;
    _camera = transform.Find("Camera");
    _track = _hit.collider.gameObject.GetComponent<TrackScript>();
    playerPosition = _hit.point + 0.5f * _hit.normal;
    playerPositionDamped = playerPosition;
    up = _hit.normal;
    forward = _track.tangents[_hit.triangleIndex / 4];
    right = Vector3.Cross(up, forward);
    rotation = Quaternion.LookRotation(forward, up);
    cameraPosition = playerPosition - forward * 10f + up * 4f;
    cameraPositionDamped = cameraPosition;
	}

  public void DoFixedUpdate() {
    velocity.z *= 0.99f;
    velocity.x *= 0.98f;

    // apply force from edges
    Vector3 edgeLeftPosition = _track.transform.TransformPoint(_mesh.vertices[_mesh.triangles[_hit.triangleIndex * 3 + 1]]);
    Vector3 edgeRightPosition = _track.transform.TransformPoint(_mesh.vertices[_mesh.triangles[_hit.triangleIndex * 3 + 2]]);
    Vector3 edgeLeftDirection = playerPosition - edgeLeftPosition;
    Vector3 edgeRightDirection = playerPosition - edgeRightPosition;
    // @refactor this seems fishy
    float edgeLeftDecrement = 1f - Ease.InCubic(Mathf.Clamp(edgeLeftDirection.magnitude, 0f, 2f) / 2f);
    float edgeRightDecrement = 1f - Ease.InCubic(Mathf.Clamp(edgeRightDirection.magnitude, 0f, 2f) / 2f);
    velocity.z *= 1f - 0.2f * edgeLeftDecrement;
    velocity.z *= 1f - 0.2f * edgeRightDecrement;
    velocity.x *= 1f - 0.4f * edgeLeftDecrement;
    velocity.x *= 1f - 0.4f * edgeRightDecrement;
    // forward = Vector3.Lerp(_track.tangents[_hit.triangleIndex / 4], rotation * velocity, 0.2f + Mathf.Clamp(velocity.z / 40f, 0f, 2f)).normalized;
    // Debug.DrawRay(edgeLeftPosition, edgeLeftDirection.normalized * 2f, Color.white, 0f, false);
    // Debug.DrawRay(edgeRightPosition, edgeRightDirection.normalized * 2f, Color.white, 0f, false);
  }

  // guarente the order of updates with playerclass
	public void DoUpdate () {
    // float gas = Mathf.Max(Input.GetAxis("Vertical") - 0.5f, 0f);
    // float gas = Input.GetAxis("Vertical");
    float gas = 1f;
    velocity.z += gas * (1f - velocity.z / 60f) * 60f * Time.deltaTime;
    velocity.x += Input.GetAxis("Horizontal") * (1f - velocity.x / 40f) * 40f * Time.deltaTime;

    // do a raycast from the position down towards the track, to get the current triangle
    // we are currently on
    if (!Physics.Raycast(playerPosition + (rotation * velocity * Time.deltaTime), up * -1f, out _hit, 3f, MASK_TRACK)) {
      // stopping the player
      velocity = Vector3.zero;
      Physics.Raycast(playerPosition, up * -1f, out _hit, 3f, MASK_TRACK);
      /*
      // bounce felt fisy
      Vector3 delta = rotation * Vector3.Reflect(velocity, Vector3.right) * Time.deltaTime;
      bool safe = false;
      for (float i = 1f; i >= 0f; i -= 0.01f) {
        if (Physics.Raycast(playerPosition + delta * i, up * -1f, out _hit, 3f, MASK_TRACK)) {
          Debug.DrawRay(playerPosition + delta * i, up * 30f, Color.green, 120f, false);
          safe = true;
          break;
        }
        float sign = Mathf.Sign(Vector3.Dot(velocity, Vector3.right));
        velocity = Vector3.Reflect(velocity, Vector3.right) - (Vector3.right * sign) * 0.8f * Time.deltaTime;
        Debug.DrawRay(playerPosition + delta * i, up * 30f, Color.red, 120f, false);
      }
      if (!safe) {
        Debug.Log("PANIC couldnt find solid ground");
        Debug.Break();
      }
      */
    }

    // update the track, if we`re on a new one
    if (_hit.collider.gameObject != _trackObject) {
      _track = _hit.collider.gameObject.GetComponent<TrackScript>();
      MeshCollider meshCollider = (MeshCollider) _hit.collider;
      _mesh = meshCollider.sharedMesh;
      _trackObject = _hit.collider.gameObject;
    }
    if (_track == null) {
      Debug.Log("PANIC no track!");
      Debug.Break();
    }

    // update axis/rotations
    up       = _track.normals[_hit.triangleIndex / 4];
    forward  = _track.tangents[_hit.triangleIndex / 4];
    right    = Vector3.Cross(up, forward);
    rotation = Quaternion.LookRotation(forward, up);

    // update positions
    playerPosition = _hit.point + up * 0.5f;

    upDamped = Vector3.SmoothDamp(upDamped, up, ref _dampUp, 0.2f);
    forwardDamped = Vector3.SmoothDamp(forwardDamped, forward, ref _dampForward, 0.2f);
    playerPositionDamped = Vector3.SmoothDamp(playerPositionDamped, playerPosition, ref _dampPlayer, 0.1f);
	}

  public void DoLateUpdate() {
    cameraPosition = playerPosition - forward * 10f + up * 4f;
    cameraPositionDamped = Vector3.SmoothDamp(cameraPositionDamped, cameraPosition, ref _dampCamera, 0.2f);
    _camera.position = cameraPositionDamped;
    _camera.rotation = Quaternion.LookRotation(forwardDamped, upDamped);
  }
}
