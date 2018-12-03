using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour {

	bool levelComplete;

	void OnTriggerEnter2D(Collider2D c) {
		if (c.tag == "Player" && !levelComplete) {
			levelComplete = true;
			FindObjectOfType<Spawner>().LevelComplete();
		}
	}
}
