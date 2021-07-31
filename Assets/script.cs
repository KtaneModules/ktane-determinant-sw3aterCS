using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class script : MonoBehaviour {

	public KMAudio Audio;
    public KMBombInfo Info;
    public KMNeedyModule Module;
	public KMSelectable[] btn;
	public KMSelectable minus, submit;
	public GameObject NeedyTimer;
	public TextMesh Screen;

	//true_answer is the actual answer of the problem. answer is the inputted answer by the defuser.
	public string str_answer = "";
	public int true_answer = 0;
	public int answer = 0;

	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;
	private bool _isSolved = false, _lightsOn = false, _minusPressed = false;

	//bomb generation stage
	private void Awake() {
        _moduleId = _moduleIdCounter++;
        Module.OnNeedyActivation += OnNeedyActivation;
		Module.OnNeedyDeactivation += OnNeedyDeactivation;
		Module.OnTimerExpired += OnTimerExpired;
        Info.OnBombExploded += delegate () { OnEnd(false); };
        Info.OnBombSolved += delegate () { OnEnd(true); };
		NeedyTimer.SetActive(false);
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
		Module.HandleStrike();
		_isSolved = true;
		_minusPressed = false;
		answer = 0;
		str_answer = "";
		Screen.text = "";
	}

    void OnEnd(bool n)
    {
        bombSolved = true;
        if (n)
        {
            Screen.text = "";
        }
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
		Screen.text = str_elementA + " " + str_elementB + "\n" + str_elementC + " " + str_elementD;
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
			Debug.LogFormat ("[Needy Determinants #{0}] is temporarily cleared!", _moduleId);
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

    //twitch plays
    #pragma warning disable 414
    private bool bombSolved = false;
    private readonly string TwitchHelpMessage = @"!{0} submit <#> [Submits the specified determinant]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    int start = 0;
                    if (temp < 0)
                    {
                        start = 1;
                        minus.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    for (int i = start; i < parameters[1].Length; i++)
                    {
                        btn[int.Parse(parameters[1][i].ToString())].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    submit.OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified determinant to submit '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify a determinant to submit!";
            }
            yield break;
        }
    }

    void TwitchHandleForcedSolve()
    {
        //The code is done in a coroutine instead of here so that if the solvebomb command was executed this will just input the number right when it activates and it wont wait for its turn in the queue
        StartCoroutine(DealWithNeedy());
    }

    private IEnumerator DealWithNeedy()
    {
        while (!bombSolved)
        {
            while (Screen.text.Equals("")) { yield return null; }
            yield return ProcessTwitchCommand("submit " + true_answer);
        }
    }
}
