using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GenerationScreen : MonoBehaviour {


	public static bool useRandomSeed;
	public InputField inputField;
	public static string seed;
	public Button play;


	void Start () {

	}


	void Update(){
		if (inputField.text == "") {
			play.interactable = false;
		} else {
			play.interactable = true;
		}

	}


	public void TaskOnClick () {

		if (inputField.text == "") {

			seed = System.DateTime.Now.GetHashCode ().ToString ();
			inputField.text = seed;
		} 
				
		}

	public void Play (){
		seed = inputField.text;
		SceneManager.LoadScene ("Game");
	}

	public void Quit(){
		Application.Quit();
	}
		




}
