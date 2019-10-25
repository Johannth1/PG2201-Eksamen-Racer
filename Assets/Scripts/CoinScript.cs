using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CoinStackable {
  void DropCoin(Collider collider, RaycastHit hit);
  void StackCoin(Transform coin, CoinStackable parent = null);
  void UpdateCoin(Vector3 velocity, float index, float numCoins);
  Vector3 CoinVelocity(Vector3 velocity);
}

public class CoinScript : MonoBehaviour, CoinStackable {

  private CoinStackable _parent;
  private Transform _child;
  private Vector3 _velocity = Vector3.zero;


	public void UpdateCoin(Vector3 velocity, float index, float numCoins) {
    _velocity = velocity;
    // transform.localPosition = new Vector3(-velocity.x/500f, transform.localPosition.y, -velocity.z/500f);
    // float r = (1f - Mathf.Min(8f, index) / 8f) * 0.5f;
    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    if (_child != null) {
      _child.gameObject.GetComponent<CoinScript>().UpdateCoin(velocity, index + 1f, numCoins);
    }
  }

  public void StackCoin(Transform coin, CoinStackable parent = null) {
    _parent = parent;
    if (coin != null) {
      Transform current = _child;
      _child = coin;
      _child.SetParent(transform);
      _child.localPosition = new Vector3(0f, 0.15f, 0f);
      _child.gameObject.GetComponent<CoinScript>().StackCoin(current, this);
    }
  }

  public Vector3 CoinVelocity(Vector3 velocity) {
    if (_parent == null)
      return _velocity + velocity;
    return _parent.CoinVelocity(_velocity + velocity);
  }

  public void DropCoin(Collider collider, RaycastHit hit) {
    if (_child) {
      _child.gameObject.GetComponent<CoinScript>().DropCoin(collider, hit);
    }
    if (_parent != null) {
      // getting the current velocity for coin, and adding some small variance
      // I want the coins to spread out if/when they detach at the same time
      Vector3 velocity = CoinVelocity(new Vector3(Random.value, Random.value, Random.value));
      Vector3 position = transform.TransformPoint(transform.position) - velocity * 2f;
      Transform mesh = transform.Find("Mesh");
      mesh.SetParent(transform.root);

      Rigidbody rb = mesh.GetComponent<Rigidbody>();
      rb.isKinematic = false;
      rb.detectCollisions = true;
      rb.position = position;
      rb.AddForce(velocity, ForceMode.VelocityChange);

      BoxCollider boxCollider = mesh.GetComponent<BoxCollider>();
      boxCollider.isTrigger = false;

      Destroy(gameObject);
      Destroy(mesh.gameObject, 2f);
      _parent = null;
    }
  }
}
