using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;
using Helpers;

public class TrackMeshMagnet : MonoBehaviour {

  static int MASK_TRACK = 1 << 8;

  protected GameObject _trackObject;
  protected TrackScript _trackScript;
  protected Mesh _trackMesh;
  protected RaycastHit _trackHit;
  protected bool _grounded = false;

  protected Vector3 _position = Vector3.zero;
  protected Vector3 _up = Vector3.up;
  protected Vector3 _down = -Vector3.up;
  protected Vector3 _forward = Vector3.forward;
  protected Vector3 _right = Vector3.right;
  protected Quaternion _rotation = Quaternion.identity;

  virtual public Vector3 position {
    get {
      return (_grounded)
        ? _position
        : transform.position;
    }
  }

  public bool ConnectToTrack() {
    Vector3 dir;
    if (Physics.Raycast(position, -Vector3.up, out _trackHit, 10f, MASK_TRACK)) {
      _down = -Vector3.up;
      _up = Vector3.up;
    } else if (Physics.Raycast(position, Vector3.up, out _trackHit, 10f, MASK_TRACK)) {
      _down = Vector3.up;
      _up = -Vector3.up;
    } else if (Physics.Raycast(position, -Vector3.forward, out _trackHit, 10f, MASK_TRACK)) {
      _down = Vector3.forward;
      _up = -Vector3.forward;
    } else if (Physics.Raycast(position, Vector3.forward, out _trackHit, 10f, MASK_TRACK)) {
      _down = -Vector3.forward;
      _up = Vector3.forward;
    } else if (Physics.Raycast(position, -Vector3.right, out _trackHit, 10f, MASK_TRACK)) {
      _down = -Vector3.right;
      _up = Vector3.right;
    } else if (Physics.Raycast(position, Vector3.right, out _trackHit, 10f, MASK_TRACK)) {
      _down = Vector3.right;
      _up = -Vector3.right;
    } else return false;
    return MoveOnTrackMesh(_trackHit.point);
  }

  public bool MoveOnTrackMesh(Vector3 position) {
    if (Physics.Raycast(position + _up, _down, out _trackHit, 3f, MASK_TRACK)) {
      if (_trackHit.collider.gameObject != _trackObject) {
        _trackScript = _trackHit.collider.gameObject.GetComponent<TrackScript>();
        MeshCollider meshCollider = (MeshCollider) _trackHit.collider;
        _trackMesh = meshCollider.sharedMesh;
      }
      _up       = _trackScript.normals[_trackHit.triangleIndex / 4];
      _down     = -_up;
      _forward  = _trackScript.tangents[_trackHit.triangleIndex / 4];
      _right    = Vector3.Cross(_up, _forward);
      _rotation = Quaternion.LookRotation(_forward, _up);
      _position = _trackHit.point;
      _grounded = true;
      return true;
    }
    _grounded = false;
    return false;
  }
}

public class TrackScript : UniqueMesh {

  public GameObject coinTriggerPrefab;
  public int segmentsDepth = 30;
  public float unitsWidth = 10f;
  public Vector3 up = Vector3.up;
  public int numberOfCoins = 10;

  [HideInInspector]
  public BezierSpline curve;
  [HideInInspector]
  public List<Vector3> tangents;
  [HideInInspector]
  public List<Vector3> normals;

  void Start() {
    curve = GetComponent<BezierSpline>();
    List<Vector3> previous = new List<Vector3>();
    int attempts = 0;
    while (previous.Count < numberOfCoins) {
generateCoins:
      if (attempts > 100) {
        Debug.Log("PANIC! oO");
        Debug.Break();
      }
      attempts++;
      float t = UnityEngine.Random.value;
      Vector3 point = curve.GetPoint(t);
      foreach (Vector3 other in previous) {
        if ((other - point).magnitude < 4f) {
          goto generateCoins;
        }
      }
      float width = unitsWidth-6f;
      Quaternion rotation = GetRotation(t) * Quaternion.Euler(0f, 0f, GetRoll(t));
      point += rotation * new Vector3(UnityEngine.Random.value*width - width/2f, 1f, 0f);
      GameObject instance = Instantiate(coinTriggerPrefab, point, rotation);
      instance.transform.SetParent(transform);
      previous.Add(point);
    }
  }

  public void GenerateMesh() {
    curve = curve == null ? GetComponent<BezierSpline>() : curve;
    var vertices = new List<Vector3>(); // @refactor .mesh, .sharedMesh etc..
    var triangles = new List<int>();
    float tDepth = 0f;
    float tStep = 1f / (segmentsDepth + -1);
    tangents = new List<Vector3>();
    normals = new List<Vector3>();

    for (int segment = 0; segment < segmentsDepth; segment++) {
      // get the rotation from the bezier curve, and bake in the roll (z rotation) specified for points
      Quaternion rotation = GetRotation(tDepth) * Quaternion.Euler(0f, 0f, GetRoll(tDepth));
      Vector3 tangent = rotation * Vector3.forward;
      Vector3 normal = rotation * Vector3.up;
      Vector3 position = curve.GetPoint(tDepth) - transform.position;

      // Create the vertices
      Vector3 line = rotation * (Vector3.right * unitsWidth);
      Vector3 left = position - line * 0.5f;
      Vector3 right = position + line * 0.5f;
      vertices.Add(transform.worldToLocalMatrix * left);
      vertices.Add(transform.worldToLocalMatrix * ((rotation * (Vector3.up * -0.1f)) + left));
      vertices.Add(transform.worldToLocalMatrix * right);
      vertices.Add(transform.worldToLocalMatrix * ((rotation * (Vector3.up * -0.1f)) + right));

      // Create the triangle (faces)
      if (segment != 0) {
        int x = (segment - 1) * 4;
        // face 1 front
        triangles.Add(x);
        triangles.Add(x + 4);
        triangles.Add(x + 2);
        // face 2 front
        triangles.Add(x + 2);
        triangles.Add(x + 4);
        triangles.Add(x + 6);
        // face 1 back
        triangles.Add(x + 1);
        triangles.Add(x + 3);
        triangles.Add(x + 5);
        // face 2 back
        triangles.Add(x + 3);
        triangles.Add(x + 7);
        triangles.Add(x + 5);
      }

      // Save data about track segment
      tangents.Add(tangent);
      normals.Add(normal);

      tDepth = Mathf.Min(tDepth + tStep, 1f);
    }

    // Create/update the mesh
    mesh.Clear();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    GetComponent<MeshCollider>().sharedMesh = mesh;
  }

  public Quaternion GetRotation(float t) {
    // go on an adventure to find the normal of bezier curve
    Vector3 tangent = curve.GetTangent(t).normalized;
    Vector3 binormal = Vector3.Cross(up, tangent).normalized;
    Vector3 normal = Vector3.Cross(tangent, binormal).normalized;
    return Quaternion.LookRotation(tangent, normal);
  }

  public float GetRoll(float t) {
    float tSegment = t * (curve.Count - 1);
    int startPoint = (int) tSegment;
    int endPoint = Math.Min(startPoint + 1, curve.Count - 1);
    return Mathf.Lerp(
      curve[startPoint].roll,
      curve[endPoint].roll,
      tSegment - startPoint);
  }

  public Quaternion GetRotation(Vector3 position, out float t) {
    curve.FindNearestPointTo(position, out t);
    return GetRotation(t);
  }
}
