using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public GameObject player;
	public float maxX = 0, maxY = 0, minX = 0, minY = 0;
	public float smoothTime = 0.3F;
	private Vector3 velocity = Vector3.zero;

	
	// Update is called once per frame
	void LateUpdate () {
		// Move camera to player
		Vector3 newPos = player.transform.TransformPoint(new Vector3(10, 0, -10));
		// Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);

		// Prevent camera from going out of given boundaries
		if (transform.localPosition.x > maxX) {
			transform.localPosition = new Vector3(maxX, transform.position.y, -10);
		}
		else if (transform.localPosition.x < minX) {
			transform.localPosition = new Vector3(minX, transform.position.y, -10);
		}

		if (transform.localPosition.y > maxY) {
			transform.localPosition = new Vector3(transform.position.x, maxY, -10);
		}
		else if (transform.localPosition.y < minY) {
			transform.localPosition = new Vector3(transform.position.x, minY, -10);
		}
	}
}
