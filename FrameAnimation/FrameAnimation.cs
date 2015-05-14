using UnityEngine;
using System.Collections;

public class FrameAnimation : MonoBehaviour {

	public enum FType
	{
		PlaneFrame = 0,  //面片上显示的
		ScreenFrame = 1, //全屏显示的
		BlankFrame = 2   //空白帧
	};

	public GameObject ManagerGameObj;
	private AppManager ManagerComp;

	public float fps = 25.0f;
	public string path = "";
	public int length = 0;
	public int startFrame = 0;  //设置开始帧序号，中间效果层开始帧可能不为零
	public int[] FrameBreakPoint; //设置不同类型的片段序列帧的剪切点
	public FType[] FrameType;  ////设置对应的不同类型的片段序列帧的类型，PlaneFrame在面片上显示，ScreenFrame是全屏显示

	private bool isPlaying = false;
	private int currentIndex = 0;
	private FType currentFrameType;
	private Texture frameTexture;

	private float startTime = 0.0f;
	private float dTime = 0.0f;

	// Use this for initialization
	void Start () {
		ManagerComp = ManagerGameObj.GetComponent("AppManager") as AppManager;

	}
	
	// Update is called once per frame
	void Update () {
		if (isPlaying) {
			ManagerComp.startTime = Time.time; //给管理器刷新时间，用来计算闲置时间
			dTime = Time.time - startTime;   //startTime在StartPlay()命令函数中赋值
			if (currentIndex < (FrameBreakPoint[FrameBreakPoint.Length-1] - 1)) {
				currentIndex = (int)(dTime * fps + 0.5); //+0.5是为了取整时向上一个整数靠拢
			}else{
				currentIndex = 0;
				isPlaying = false;
			}
			frameTexture = (Texture)Resources.Load (path + currentIndex.ToString ("D5"));
//			if(frameTexture == null){
//				Debug.Log ("frameTexture@@@@:Null");
//			}


			//判断当前帧是哪一种片段类型
			for (int i = 0; i < FrameBreakPoint.Length; i++) {
				if(i==0){
					if(0 < currentIndex && currentIndex < FrameBreakPoint[i]){
						currentFrameType = FrameType[i];
						break;
					}
				}else{
					if(FrameBreakPoint[i-1] < currentIndex && currentIndex < FrameBreakPoint[i]){
						currentFrameType = FrameType[i];
						break;
					}
				}
			}
			//Debug.Log ("currentFrameType: "+currentFrameType);
			if(frameTexture != null && currentFrameType == FType.PlaneFrame){
				renderer.material.mainTexture = frameTexture;
				Resources.UnloadUnusedAssets ();
			} 
		}
	}

	void OnGUI(){
		if (isPlaying) {
			if (frameTexture != null && currentFrameType == FType.ScreenFrame) {
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), frameTexture, ScaleMode.StretchToFill);
				Resources.UnloadUnusedAssets ();  
			}
		}
	}

	public void StartPlay(){
		Debug.Log ("FrameAnimation Start Play!");
		isPlaying = true;
		startTime = Time.time;
		if (audio != null) {
			if (audio.isPlaying) {
				audio.Stop ();
			}
			if (!audio.isPlaying) {
				audio.Play ();
			} 
		}
		Resources.UnloadUnusedAssets (); 
	}

	public void StopPlay(){
		Debug.Log ("FrameAnimation Stop Play!");
		isPlaying = false;
		currentIndex = 0;
		if (audio != null) {
			if (audio.isPlaying) {
				audio.Stop ();
			}
		}
		Resources.UnloadUnusedAssets (); 
	}
}
