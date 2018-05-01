using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class twoColorsCamera : cfxNotificationIntegratedReceiver {

	public Shader m_Shader = null;
	private Material m_Material;

	public Color Left;
	public Color Right;

	private float Blendback;

	public string startCameraEffect;
	public string stopCameraEffect;

	private float counter;
	private float initialCounter;
	private bool fadeIn; // if not fade in we fade out

	public override void Start() {
		// call SIP's Start()
		base.Start ();
		if (m_Shader) {
			m_Material = new Material(m_Shader);
			m_Material.name = "twoColorsEffectMaterial";
			m_Material.hideFlags = HideFlags.HideAndDontSave;
		} else {
			Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.", this.gameObject);
			enabled = false;
		}

		subscribeTo (startCameraEffect); // they'll listen even though disabled
		subscribeTo (stopCameraEffect);

		// disable this script
		this.enabled = false;
	}

	//
	// This is the camera effect
	//
	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (counter < 0)
			counter = 0;
		
		if (m_Shader && m_Material) {
			m_Material.SetColor ("_Left", Left);
			m_Material.SetColor ("_Right", Right);
			if (fadeIn) {
				// blendback goes from 1..0
				Blendback = (counter / initialCounter);
			} else {
				// blendback from 0..1
				Blendback = 1 - (counter / initialCounter);
			}
			m_Material.SetFloat ("_Blendback", Blendback);

			Graphics.Blit(src, dst, m_Material);

		} else {
			Graphics.Blit(src, dst);
			Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.", this.gameObject);
			enabled = false;
		}

		counter = counter - Time.deltaTime;
	}


	public override void OnNotification (string notificationName, Dictionary<string, object> info)
	{
		if (notificationName == startCameraEffect) {
			// We use info to transport the following info:
			// time for the fx
			initialCounter = 1.0f;
			if (info.ContainsKey ("Duration")) {
				initialCounter = float.Parse (info ["Duration"] as string);
			}
			counter = initialCounter;

			fadeIn = false;
			if (info.ContainsKey ("FadeIn"))
				fadeIn = true;
			
			this.enabled = true;
		}

		if (notificationName == stopCameraEffect) {
			this.enabled = false;
		}
	}
}
