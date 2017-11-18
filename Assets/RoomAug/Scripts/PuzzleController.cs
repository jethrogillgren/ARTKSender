using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Timers;
using UnityEngine.Networking;

//Attaches to the Gamemaster Controls and orchestrates puzzle flow / changes
public class PuzzleController : NetworkBehaviour
{
	public const int startingTime = 60;
	public int actualTimeRemaining = 60; //Out of 60 minutes.
	public int displayTimeRemaining; //minutes.

	public int percentComplete = 0; //100 is completed game.
	public int puzzleStep = 1; //Each puzzle gives a step.  Order completed is partly variable.

	private List<Puzzle> puzzles;


	public  static int totalPuzzleSteps = 0;
	private static int totalPuzzleImportance = 0;//static so we can calculate it while initializing the Puzzle structs

	public Slider difficultySlider;// 0-4.  0 Is easiest.  4 is every extra puzzle. 1/3 is some extra puzzles. 2 is the default.
	public int getDifficulty() {
		return (int)Math.Ceiling( difficultySlider.value );//Locked to whole numbers anyway
	}

	public Button hintButton;
	public Text timeText;


	public override void OnStartServer()
//	public void Start()
	{
		Debug.LogError("Server Started with hasAuthority: " + hasAuthority);

		if (!hasAuthority)
			return;
			
		//Adds a listener to the main slider and invokes a method when the value changes.
		difficultySlider = (Slider) GameObject.Find("Difficulty Slider").GetComponent<Slider>();
		timeText = (Text) GameObject.Find("Time Text").GetComponent<Text>();
		hintButton = (Button) GameObject.Find("Hint Button").GetComponent<Button>();
		hintButton.onClick.AddListener( delegate { GiveHint(); } );

//		if (totalPuzzleImportance > 100)
//			Util.JLogErr("TOO MUCH IMPORTANCE in PuzzleController: " + totalPuzzleImportance);

//		puzzles.Add(puzzle1);
//		puzzles.Add(puzzle2);
//		puzzles.Add(puzzle3);
//		puzzles.Add(puzzle4);
		puzzles = Util.CreateList(puzzle1, puzzle2, puzzle3, puzzle4);

		ActivatePuzzle1();

		Timer timer = new Timer(60000);//Minute
		timer.AutoReset = true;
		timer.Elapsed +=new ElapsedEventHandler(HandleMinuteTimer);
		timer.Start();
	}

	private void HandleMinuteTimer(object source, System.Timers.ElapsedEventArgs e)
	{
		actualTimeRemaining--;
		SetDisplayTimeRemaining();
		UpdateTimeText();
	}

	public void SetDisplayTimeRemaining() {
		displayTimeRemaining = actualTimeRemaining - (GetTimeAhead()/2); //TODO
	}

	private void UpdateTimeText() {
		timeText.text = "Time: " + actualTimeRemaining + " (" + displayTimeRemaining + ")" + " Puzzles: " + puzzleStep + "/" + totalPuzzleSteps + " (" + percentComplete + "%)";
	}

	private void MoveStepAndPercent(int importance) {
		puzzleStep++;

		//Get absolure importance for the percentage complete metric
		int importanceStep = 100 / totalPuzzleImportance;
		percentComplete += (importance * importanceStep);

		UpdateTimeText();//These changes affect the TimeText
	}

	//Tells us how far ahead of time the team is. minutes.
	public int GetTimeAhead() {
		//Calculate how long it should have tagen to get here
		int shouldHaveTaken = (percentComplete / 100) * 60; //mins
		int actuallyTook = 60 -actualTimeRemaining ;

		return shouldHaveTaken - actuallyTook;
	}

	public void GiveHint() {
		hintButton.interactable = false;

		//Trigger Hint
		Util.JLog("Giving a Hint");
		//TODO

		hintButton.interactable = true;
	}



	////Manual Puzzle Steps.
	public struct Puzzle {
		public Puzzle(int number, int importance ) {
			Number = number;
			Importance = importance;

			Started = false;
			Completed = false;

//			Activate = activate;
//			Complete = complete;

			totalPuzzleImportance += importance;
			totalPuzzleSteps ++;
//			puzzles.Add(this);
		}

		public int Number;  
		public int Importance;//A value other than 1 indicates how much this puzzles completion affects total percentage of game.
//		public Action Activate;
//		public Action Complete;
		public bool Started;
		public bool Completed;
	};
	//Common actions on puzzles.
	public void ActivatePuzzle(Puzzle puzzle) {
		puzzle.Started = true;
	}
	public void CompletePuzzle(Puzzle puzzle) {
		puzzle.Completed = true;
		MoveStepAndPercent(puzzle.Importance);
	}


	//Wood Search
	public Puzzle puzzle1 = new Puzzle(1, 5);
	public void ActivatePuzzle1() {
		ActivatePuzzle(puzzle1);
		//Set-up Puzzle Gameobjects
	}
	public void CompletePuzzle1() {
		CompletePuzzle(puzzle1);
		//Animate & Change any Gameobjects...



		ActivatePuzzle2();
		ActivatePuzzle3();
	}


	//Wood Puzzle XXX
	public Puzzle puzzle2 = new Puzzle(2, 1);
	public void ActivatePuzzle2() {
		ActivatePuzzle(puzzle2);
		//Set-up Puzzle Gameobjects
	}
	public void CompletePuzzle2() {
		CompletePuzzle(puzzle2);

		//Animate & Change any Gameobjects...

		if( puzzle3.Completed ) {
			ActivatePuzzle4();
		}
	}


	//Wood Puzzle YYY
	public Puzzle puzzle3 = new Puzzle(3, 1);
	public void ActivatePuzzle3() {
		ActivatePuzzle(puzzle3);
		//Set-up Puzzle Gameobjects
	}
	public void CompletePuzzle3() {
		CompletePuzzle(puzzle3);

		//Animate & Change any Gameobjects...

		if( puzzle2.Completed ) {
			ActivatePuzzle4();
		}
	}


	public Puzzle puzzle4 = new Puzzle(3, 3);
	public void ActivatePuzzle4() {
		ActivatePuzzle(puzzle4);
		//Set-up Puzzle Gameobjects
	}
	public void CompletePuzzle4() {
		CompletePuzzle(puzzle1);

		//Animate & Change any Gameobjects...
	}

}
