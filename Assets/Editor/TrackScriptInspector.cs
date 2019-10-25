using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TrackScript))]
public class TrackScriptInspector : Editor {

  private TrackScript trackScript;

  void OnEnable() {
    trackScript = (TrackScript) target;
  }

  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    if (GUILayout.Button("Generate Mesh")) {
      trackScript.GenerateMesh();
    }
    if (GUILayout.Button("Create Coin")) {
      GameObject prefab = Resources.Load("Prefabs/CoinTriggerPrefab", typeof(GameObject)) as GameObject;
      GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
      instance.transform.parent = trackScript.transform;
      instance.name = "CoinTrigger";
      float t = Mathf.Clamp(Random.value, 0.1f, 0.9f);
      Vector3 point = trackScript.curve.GetPoint(t);
      Quaternion rotation = trackScript.GetRotation(t) * Quaternion.Euler(0f, 0f, trackScript.GetRoll(t));
      float width = trackScript.unitsWidth-4f;
      float offset = Random.value*width - width/2f + width/2f;
      instance.transform.position = point + rotation * new Vector3(offset, 0f, 0f);
      instance.transform.rotation = rotation;
    }
  }
}
