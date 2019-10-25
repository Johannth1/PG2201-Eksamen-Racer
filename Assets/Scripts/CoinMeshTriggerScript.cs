using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMeshTriggerScript : MonoBehaviour {

  public GameObject coinPrefab;
  private CoinPileTriggerScript _parent;

  void Start() {
    _parent = transform.parent.gameObject.GetComponent<CoinPileTriggerScript>();
  }

  void OnTriggerEnter(Collider other) {
    if (other.gameObject.tag == "Player") {
      CoinStackable stack = (CoinStackable) other.transform.parent.GetComponent(typeof(CoinStackable));
      stack.StackCoin(null, null);
      _parent.StartLoading(10f);
    }
  }
}
