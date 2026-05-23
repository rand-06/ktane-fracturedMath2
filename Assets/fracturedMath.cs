using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class fracturedMath : MonoBehaviour
{

	public KMSelectable multiplyButton, submitButton, stateButton;
	public KMSelectable[] numberButtons;
	public TextMesh stateText;
	public TextMesh[] numbers;
	public KMAudio Audio;
	public TextMesh multiplierText;
	
	private bool ModuleSolved;

	private readonly List<int> denominators = new List<int> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
	private readonly List<int> multipliers = new List<int> { 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
	
	private List<int> currentIntegers, currentNumerators, currentDenominators;
	private List<int> submittedNumerators, submittedDenominators;
	private int nextMultiplier;
	private int mode; // 0 - READ, 1 - NUMERATORS, 2 - DENOMINATORS
	private readonly List<string> modeNames = new List<string> { "READ", "NUMS", "DENS" };
	private int selectedNumber = -1;
	
	private bool moduleSelected;
	
	static int ModuleIdCounter;
	int ModuleId;

	void multiplyButtonPress()
	{
		if (mode != 0 || ModuleSolved) return;
		for (int i = 0; i < currentIntegers.Count; i++)
		{
			currentIntegers[i] *= nextMultiplier;
			currentNumerators[i] *= nextMultiplier;
			currentIntegers[i] += currentNumerators[i] / currentDenominators[i];
			currentNumerators[i] %= currentDenominators[i];
			currentIntegers[i] %= 100;
		}
		nextMultiplier = multipliers.PickRandom();
		multiplierText.text = "×" + nextMultiplier.ToString("D2");
		Debug.LogFormat("[Shattered Math #{0}] New fractions: {1}",
			ModuleId,
			Enumerable.Range(0, currentNumerators.Count).Aggregate("", (a,b)=>a + ", "+currentNumerators[b]+"/"+currentDenominators[b])
			);
		refreshTexts();
	}

	void refreshTexts()
	{
		if (mode == 0)
		{
			for (int i = 0; i < currentIntegers.Count; i++) numbers[i].text = Rnd.Range(0,100).ToString("D2");
			List<List<int>> indices = new List<List<int>>
			{
				new List<int> { 0, 1, 2 },
				new List<int> { 3, 4, 5 },
				new List<int> { 6, 7, 8 },
				new List<int> { 0, 3, 6 },
				new List<int> { 1, 4, 7 },
				new List<int> { 2, 5, 8 }
			};
			for (int i = 0; i < 6; i++)
			{
				int sumInt = indices[i].Select(x => currentIntegers[x]).Sum();
				int multDen = indices[i].Select(x => currentDenominators[x]).Aggregate(1, (x, y) => x * y);
				int sumMult =
					currentNumerators[indices[i][0]] * currentDenominators[indices[i][1]] *
					currentDenominators[indices[i][2]] +
					currentNumerators[indices[i][1]] * currentDenominators[indices[i][2]] *
					currentDenominators[indices[i][0]] +
					currentNumerators[indices[i][2]] * currentDenominators[indices[i][0]] *
					currentDenominators[indices[i][1]];
				sumInt += sumMult / multDen;
				sumInt %= 100;
				numbers[9 + i].text = sumInt.ToString("D2");
			}
		}

		if (mode == 1)
		{
			for (int i = 0; i < 9; i++) numbers[i].text = submittedNumerators[i].ToString("D2");
			for (int i = 9; i < 15; i++) numbers[i].text = "";
		}
		if (mode == 2)
		{
			for (int i = 0; i < 9; i++) numbers[i].text = submittedDenominators[i].ToString("D2");
			for (int i = 9; i < 15; i++) numbers[i].text = "";
		}
	}

	bool checkSolution()
	{
		List<int> currentNumerators1 = currentNumerators,
			currentDenominators1 = currentDenominators,
			submittedNumerators1 = submittedNumerators,
			submittedDenominators1 = submittedDenominators;
		List<List<int>> indices = new List<List<int>>
		{
			new List<int> { 0, 1, 2 },
			new List<int> { 3, 4, 5 },
			new List<int> { 6, 7, 8 },
			new List<int> { 0, 3, 6 },
			new List<int> { 1, 4, 7 },
			new List<int> { 2, 5, 8 }
		};
		int nextMultiplier1;
		for (int _ = 0; _ < 10; _++)
		{
			nextMultiplier1 = multipliers.PickRandom();
			for (int i = 0; i < 6; i++)
			{
				int moduleNumerator =
					currentNumerators1[indices[i][0]] * currentDenominators1[indices[i][1]] *
					currentDenominators1[indices[i][2]] +
					currentNumerators1[indices[i][1]] * currentDenominators1[indices[i][2]] *
					currentDenominators1[indices[i][0]] +
					currentNumerators1[indices[i][2]] * currentDenominators1[indices[i][0]] *
					currentDenominators1[indices[i][1]];
				int playersNumerator =
					submittedNumerators1[indices[i][0]] * submittedDenominators1[indices[i][1]] *
					submittedDenominators1[indices[i][2]] +
					submittedNumerators1[indices[i][1]] * submittedDenominators1[indices[i][2]] *
					submittedDenominators1[indices[i][0]] +
					submittedNumerators1[indices[i][2]] * submittedDenominators1[indices[i][0]] *
					submittedDenominators1[indices[i][1]];
				int moduleDenominator = currentDenominators1[indices[i][0]] * currentDenominators1[indices[i][1]] *
				                        currentDenominators1[indices[i][2]];
				int playersDenominator = submittedDenominators1[indices[i][0]] * submittedDenominators1[indices[i][1]] *
				                         submittedDenominators1[indices[i][2]];
				if (moduleNumerator / moduleDenominator != playersNumerator / playersDenominator) return false;
			}

			currentNumerators1 = currentNumerators1.Select((x,n) => x * nextMultiplier1 % currentDenominators1[n]).ToList();
			submittedNumerators1 = submittedNumerators1.Select((x,n) => x * nextMultiplier1 % submittedDenominators1[n]).ToList();
		}

		return true;
	}
	void Awake(){ModuleId=++ModuleIdCounter;}
	void Start ()
	{
		GetComponent<KMSelectable>().OnFocus += delegate { moduleSelected = true; };
		GetComponent<KMSelectable>().OnDefocus += delegate { moduleSelected = false; };
		currentIntegers = Enumerable.Range(0,9).Select(_=>Rnd.Range(0,100)).ToList();
		currentDenominators = Enumerable.Range(0,9).Select(_=>denominators.PickRandom()).ToList();
		currentNumerators = currentDenominators.Select(x=>Rnd.Range(1,x)).ToList();
		submittedNumerators = Enumerable.Range(0,9).Select(_=>0).ToList();
		submittedDenominators = Enumerable.Range(0,9).Select(_=>0).ToList();
		nextMultiplier = multipliers.PickRandom();
		multiplierText.text = "×" + nextMultiplier.ToString("D2");
		multiplyButton.OnInteract += delegate
		{
			multiplyButtonPress();
			return false;
		};
		stateButton.OnInteract += delegate
		{
			pressState();
			return false;
		};
		for (int i = 0; i < numberButtons.Length; i++)
		{
			int i1 = i;
			numberButtons[i1].OnInteract += delegate
			{
				if(selectedNumber!=-1) numbers[selectedNumber].color = Color.white;
				selectedNumber = i1;
				numbers[selectedNumber].color = Color.yellow;
				return false;
			};
		}

		submitButton.OnInteract += delegate
		{
			if (submittedNumerators.Contains(0) || submittedDenominators.Contains(0)) return false;
			if (checkSolution())
			{
				ModuleSolved = true;
				GetComponent<KMBombModule>().HandlePass();
			}
			else GetComponent<KMBombModule>().HandleStrike();
			return false;
		};
		refreshTexts();
	}

	void pressState()
	{
		mode = (mode + 1) % 3;
		if(selectedNumber!=-1) numbers[selectedNumber].color = Color.white;
		selectedNumber = -1;
		stateText.text = modeNames[mode];
		refreshTexts();
	}

	void keyboardInput(int digit)
	{
		if (mode == 0 || ModuleSolved || selectedNumber == -1) return;
		if (mode == 1)
		{
			submittedNumerators[selectedNumber] = (submittedNumerators[selectedNumber] * 10 + digit) % 100;
			numbers[selectedNumber].text = submittedNumerators[selectedNumber].ToString("D2");
		}
		else
		{
			submittedDenominators[selectedNumber] = (submittedDenominators[selectedNumber] * 10 + digit) % 100;
			numbers[selectedNumber].text = submittedDenominators[selectedNumber].ToString("D2");
		}
		
	}

	void inputAll(List<string> numberStrings)
	{
		int[] casted = new int[9];
		if (Enumerable.Range(0, 9).All(i => int.TryParse(numberStrings[i], out casted[i])))
		{
			if (mode == 1) submittedNumerators = casted.ToList();
			else if (mode == 2) submittedDenominators = casted.ToList();
		}
	}
	
	void Update()
	{
		if (ModuleSolved || !moduleSelected) return;
		if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) keyboardInput(0);
		if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) keyboardInput(1);
		if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) keyboardInput(2);
		if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) keyboardInput(3);
		if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) keyboardInput(4);
		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) keyboardInput(5);
		if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) keyboardInput(6);
		if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) keyboardInput(7);
		if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) keyboardInput(8);
		if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) keyboardInput(9);
	}
	
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Use !{0} multiply / m to press multiplier button. " + 
"Use !{0} read/nums/dens to change the mode. Use !{0} input# ## to input digits in the corresponding slot " +
"(for example, !{0} input4 23 will input 23 in middle-left slot). Use !{0} inputall with 9 " +
"numbers to fill the screen at once. Use !{0} reset to reset the input.";
#pragma warning restore 414
	
	
	
	public IEnumerator ProcessTwitchCommand(string Command)
	{
		Command = Command.ToUpperInvariant();
		yield return null;
		switch (Command)
		{
			case "MULTIPLY": case "M": multiplyButtonPress(); yield break;
			case "RESET":
			{
				submittedNumerators = Enumerable.Range(0,9).Select(_=>0).ToList();
				submittedDenominators = Enumerable.Range(0,9).Select(_=>0).ToList();
				refreshTexts();
				yield break;
			}
			case "READ":
			case "NUMS":
			case "DENS":
			{
				int index = modeNames.IndexOf(Command);
				for (int i = 0; i < (index - mode + 3) % 3; i++)
				{
					yield return null; pressState();}
				yield break;
			}
			default:
			{
				List<string> commandPieces = Command.Split(' ').ToList();
				if (!commandPieces[0].StartsWith("INPUT")) yield return "sendtochaterror Invalid command.";
				if (commandPieces[0] == "INPUTALL")
				{
					if (mode==0) {yield return "sendtochaterror Cannot input while in mode READ."; yield break;}
					if (commandPieces.Count!=10) {yield return "sendtochaterror Invalid command."; yield break;}
					inputAll(commandPieces.Skip(1).ToList());
					yield break;
				}
				else
				{
					if (commandPieces[0].Length < 6 || commandPieces.Count!=2) {yield return "sendtochaterror Invalid command."; yield break;}

					int button = commandPieces[0][5] - '1';
					if (button<0 || button > 8) {yield return "sendtochaterror Invalid command."; yield break;}
					yield return null;
					numberButtons[button].OnInteract();
					int number;
					if (!int.TryParse(commandPieces[1], out number)) {yield return "sendtochaterror Invalid command."; yield break;}
					if (mode == 1) submittedNumerators[selectedNumber] = number;
					else if (mode == 2) submittedDenominators[selectedNumber] = number;
					yield break;
				}
			}
		}
	}

	public IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		ModuleSolved = true;
		GetComponent<KMBombModule>().HandlePass();
	}
}
