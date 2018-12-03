using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMove : MonoBehaviour {

	public float ground = 0;
	public float g = 9;
	public float jumpForceMin = 10;
	public float jumpForceMax = 10;
	public float speed = 4;

	public SpriteRenderer[] sprites;
	public MeshRenderer renderer;
	public float maxX;

	//Vector3 velocity;
	float vy;

	// Use this for initialization
	void Start () {
		
	}

	public void SetZ(float z) {
		z = Mathf.Lerp(.3f,1,z);
		foreach (SpriteRenderer r in sprites) {
			r.color = new Color(r.color.r, r.color.g,r.color.b,z);
		}
		renderer.material.color = new Color(renderer.material.color.r,renderer.material.color.g,renderer.material.color.b,z);
	}
	
	// Update is called once per frame
	void Update () {
		vy -= Time.deltaTime * g;
		transform.position += new Vector3(speed,vy,0) * Time.deltaTime;

		if (transform.position.y <=ground) {
			transform.position = new Vector3(transform.position.x,ground,transform.position.z);
			vy = Random.Range(jumpForceMin,jumpForceMax);	
		}
		if (transform.position.x > maxX) {
			Destroy(gameObject);
		}
	}
}
