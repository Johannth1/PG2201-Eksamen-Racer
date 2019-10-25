using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers {
  public class Ease {
    // all copied from
    // https://gist.github.com/gre/1650294
    static public float InCubic(float t) { return t*t*t; }
    static public float OutCubic(float t) { return (--t)*t*t+1; }
    static public float InOutQuad(float t) { return t<.5f ? 2f*t*t : -1f+(4f-2f*t)*t; }
  }

  public class Spring {
    private float _current;
    private float _lastValue;
    private float _velocity = 0f;
    private float _k;
    private float _f;
    private float _fps;
    private int _idleCount = 0;

    public float current {
      get {
        return _current;
      }
    }

    public Spring(float current = 0f, float k = 0.04f, float f = 0.875f, float fps = 0.06f) {
      _current = current;
      _k = k;
      _f = f;
      _fps = fps;
    }

    public float Update(float delta, float target) {
      float dk = delta / _fps * _k;
      _velocity = (_velocity + (target - _current) * dk) * _f;
      _lastValue = _current;
      _current = _current + _velocity;
      return _current;
    }

    public bool Idle() {
      if (Mathf.Abs(_current - _lastValue) < 0.01f) {
        _idleCount++;
      } else {
        _idleCount = 0;
      }
      return _idleCount > 3;
    }
  }

  // https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gb871cf6ef_0_0
  // https://youtu.be/o9RK6O2kOKo?t=590
  public class UniqueMesh : MonoBehaviour {
    [HideInInspector] int ownerID; // To ensure they have a unique mesh
    MeshFilter _mf;
    MeshFilter mf { // Tries to find a mesh filter, adds one if it doesn't exist yet
      get{
        _mf = _mf == null ? GetComponent<MeshFilter>() : _mf;
        _mf = _mf == null ? gameObject.AddComponent<MeshFilter>() : _mf;
        return _mf;
      }
    }
    Mesh _mesh;
    protected Mesh mesh { // The mesh to edit
      get{
        bool isOwner = ownerID == gameObject.GetInstanceID();
        if( mf.sharedMesh == null || !isOwner ){
          mf.sharedMesh = _mesh = new Mesh();
          ownerID = gameObject.GetInstanceID();
          _mesh.name = "Mesh [" + ownerID + "]";
        }
        return _mesh;
      }
    }
  }
}
