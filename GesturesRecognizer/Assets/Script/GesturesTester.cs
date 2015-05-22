using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GR = GesturesRecognizer;

public class GesturesTester : MonoBehaviour {

	GR gr = new GR();
	List<GR.Point> points = new List<GR.Point>();
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButton (0)) {
			Debug.Log("Mouse Press:");
			points.Add(new GR.Point(Input.mousePosition.x,Input.mousePosition.y));
		}
		if (Input.GetMouseButtonUp (0)) {
			GesturesRecognizer.Result tempRe = gr.Recognize(points);
			Debug.Log("Points：" + points.Count);
			Debug.Log("\""+ tempRe.Name +"\"----Score:"+tempRe.Score);
			points.Clear();
		}
	}

	void OnGUI(){
		GUILayout.Label ("mouseX：" + Input.mousePosition.x);
		GUILayout.Label ("mouseY：" + Input.mousePosition.y);
		GUILayout.Label ("time：" + Time.time);
		GUILayout.Label ("deltaTime：" + Time.deltaTime);
		GUILayout.Label ("fixedTime：" + Time.fixedTime);
		GUILayout.Label ("fixedDeltaTime：" + Time.fixedDeltaTime);
	}
}