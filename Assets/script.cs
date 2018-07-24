using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

public class script : MonoBehaviour {

	public KMAudio Audio;
	public KMNeedyModule Module;
	public KMBombInfo Info;
	public KMModSettings modSettings;
	public KMSelectable[] btn;
	public KMSelectable minus, submit;
	public TextMesh Screen;

	//true_answer is the actual answer of the problem. answer is the inputted answer by the defuser.
	public string str_answer = "";
	public int true_answer = 0;
	public int answer = 0;

	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;
	private bool _isSolved = false, _lightsOn = false, _minusPressed = false;
	
	//bomb generation stage
	void Start () {
		_moduleId = _moduleIdCounter++;
	}

	//the room is shown
	private void Awake() {
		GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
		GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
		GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
		Init();
		_lightsOn = true;

		//buttons handling
		minus.OnInteract += delegate () {
			handleMinus();
			return false;
		};
		submit.OnInteract += delegate () {
			ansCheck();
			return false;
		};
		for (int i = 0; i < 10; i++) {
			int j = i;
			btn [i].OnInteract += delegate () {
				handlePress(j);
				return false;
			};
		}
	}

	//initialization for start/reset
	void Init() {
		Debug.LogFormat("[Needy Determinants #{0}] This is a needy module that may reactivate at any time.", _moduleId);
	}

	protected void OnNeedyActivation() {
		_isSolved = false;
		Debug.LogFormat ("[Needy Determinants #{0}] has activated!", _moduleId);
		generateStage();
	}

	protected void OnNeedyDeactivation() {
		Debug.LogFormat ("[Needy Determinants #{0}] has deactivated.", _moduleId);
		_isSolved = true;
	}

	protected void OnTimerExpired() {
		Debug.LogFormat ("[Needy Determinants #{0}] has had its timer expire!", _moduleId);
		GetComponent<KMNeedyModule>().HandleStrike();
		_isSolved = true;
		_minusPressed = false;
		answer = 0;
		str_answer = "";
		Screen.text = "";
	}

	//generate matrix and problem
	void generateStage() {
		//generate matrix elements for interval [-9, 9], element in set of integers
		int elementA = UnityEngine.Random.Range (-9, 10);
		int elementB = UnityEngine.Random.Range (-9, 10);
		int elementC = UnityEngine.Random.Range (-9, 10);
		int elementD = UnityEngine.Random.Range (-9, 10);
		//convert to string for text display
		string str_elementA = Convert.ToString(elementA);
		string str_elementB = Convert.ToString(elementB);
		string str_elementC = Convert.ToString(elementC);
		string str_elementD = Convert.ToString(elementD);
		true_answer = (elementA * elementD) - (elementB * elementC);
		string str_true_answer = Convert.ToString(true_answer);
		Screen.text = str_elementA + "  " + str_elementB + "\n" + str_elementC + "  " + str_elementD;
		Debug.LogFormat ("[Needy Determinants #{0}] has generated with the elements {1}, {2}, {3}, and {4}. The solution is {5}.", _moduleId, str_elementA, str_elementB, str_elementC, str_elementD, str_true_answer);
	}

	//minus button handling
	void handleMinus() {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, minus.transform);
		minus.AddInteractionPunch();

		if (!_lightsOn || _isSolved || _minusPressed) return;

		Debug.LogFormat ("[Needy Determinants #{0}] Minus button is pressed. Multiplying input by -1.", _moduleId);
		_minusPressed = true;
	}

	//keypad handling
	void handlePress(int num) {
		Audio.HandlePlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, btn[num].transform);
		btn [num].AddInteractionPunch ();

		if (!_lightsOn || _isSolved) return;

		string str_num = "";
		str_num = Convert.ToString (num);
		str_answer += str_num;
		Debug.LogFormat ("[Needy Determinants #{0}] Button value {1} is pressed.", _moduleId, str_num);
	}

	//submit handling and answer checking
	void ansCheck() {
		Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
		submit.AddInteractionPunch();

		//prevent error for null string conversion to integer
		if (str_answer == "") {
			Debug.LogFormat ("[Needy Determinants #{0}] has been incorrectly solved. The module is resetting and deactivating.", _moduleId);
			GetComponent<KMNeedyModule> ().HandleStrike ();
			GetComponent<KMNeedyModule> ().HandlePass ();
			_isSolved = true;
			_minusPressed = false;
			answer = 0;
			str_answer = "";
			Screen.text = "";
			return;
		}

		answer = Convert.ToInt32(str_answer);

		if (!_lightsOn || _isSolved) return;

		Debug.LogFormat ("[Needy Determinants #{0}] Submit button pressed. Checking input...", _moduleId);
		if (_minusPressed) {
			answer = answer * -1;
		}

		//check input against actual determinant
		if (true_answer == answer) {
			Debug.LogFormat ("[Needy Determinants #{0}] is termporarily cleared!", _moduleId);
			GetComponent<KMNeedyModule> ().HandlePass ();
			//tbh i should just put this following block in a function
			_isSolved = true;
			_minusPressed = false;
			answer = 0;
			str_answer = "";
			Screen.text = "";
		} else {
			Debug.LogFormat ("[Needy Determinants #{0}] has been incorrectly solved. The module is resetting and deactivating.", _moduleId);
			GetComponent<KMNeedyModule> ().HandleStrike ();
			GetComponent<KMNeedyModule> ().HandlePass ();
			_isSolved = true;
			_minusPressed = false;
			answer = 0;
			str_answer = "";
			Screen.text = "";
		}
	}
}
