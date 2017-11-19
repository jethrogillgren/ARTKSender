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
	public const float startingTime = 60f;
	public float actualTimeRemaining;
	public float displayTimeRemaining;
	//minutes.

	//Percent to add on to display time ticks at the last section of the game.
	//eg 1 = twice as slow.  0.5 = half as slow.  0.1 = 10% slower.  2=200% (4 times) slower .
	float slowdown = 0.0f;

	public int corePercentComplete = 0;
	//Relies on last
	//100 is completed game.
	public int corePuzzleStep = 1;
	//Each core puzzle gives a step.    Order completed is partly variable.
	private List<Puzzle> corePuzzles;

	private List<Puzzle> dynamicInPuzzles;

	public bool inserterPuzzleNext = false;

	public    static int totalCorePuzzleSteps = 0;
	private static int totalCorePuzzleImportance = 0;
	//static so we can calculate it while initializing the Puzzle structs


	//// GUI refs
	public Button throwInPuzzleButton;
	public Slider difficultySlider;
	// 0-4.    0 Is easiest.    4 is every extra puzzle. 1/3 is some extra puzzles. 2 is the default.
	public int getDifficulty ()
	{
		return ( int )Math.Ceiling ( difficultySlider.value );//Locked to whole numbers anyway
	}

	public Button hintButton;
	public Text timeText;
	public Button cueInserterPuzzleButton;
	public Button cancelInserterPuzzleButton;


	Timer timer;

	public void Start ()
	{
//        if (timer != null)
//            timer.Dispose();
//            
//        timer = new Timer(1000);//Minute TODO
//        timer.AutoReset = true;
//        timer.Elapsed += new ElapsedEventHandler(HandleMinuteTimer);
//        timer.Start();

		InvokeRepeating ( "HandleMinuteTimer", 1, 1 );// Seconds
	}

	public void OnDisable ()
	{
//        if (timer != null)
//            timer.Dispose();
	}

	public override void OnStartServer ()
	{
		if (!hasAuthority)
			return;
            
		//Adds a listener to the main slider and invokes a method when the value changes.
		throwInPuzzleButton = ( Button )GameObject.Find ( "Throw In Puzzle" ).GetComponent<Button>;
		throwInPuzzleButton.onClick.AddListener ( delegate
			{
				ThrowInstantPuzzle ();
			} );
		difficultySlider = ( Slider )GameObject.Find ( "Difficulty Slider" ).GetComponent<Slider> ();
		difficultySlider.onValueChanged.AddListener ( delegate
			{
				OnDifficultyChange ();
			} );
		timeText = ( Text )GameObject.Find ( "Time Text" ).GetComponent<Text> ();
		hintButton = ( Button )GameObject.Find ( "Hint Button" ).GetComponent<Button> ();
		hintButton.onClick.AddListener ( delegate
			{
				GiveHint ();
			} );
		cueInserterPuzzleButton = ( Button )GameObject.Find ( "Cue Inserter Puzzle Button" ).GetComponent<Button> ();
		cueInserterPuzzleButton.onClick.AddListener ( delegate
			{
				{
					CueInserterPuzzle ();
				}
			} );
		cancelInserterPuzzleButton = ( Button )GameObject.Find ( "Cancel Inserter Puzzle Button" ).GetComponent<Button> ();
		cancelInserterPuzzleButton.onClick.AddListener ( delegate
			{
				{
					CancelInserterPuzzle ();
				}
			} );

		actualTimeRemaining = startingTime;
		displayTimeRemaining = actualTimeRemaining;

//      if (totalPuzzleImportance > 100)
//          Util.JLogErr("TOO MUCH IMPORTANCE in PuzzleController: " + totalPuzzleImportance);

//      puzzles.Add(puzzle1);
//      puzzles.Add(puzzle2);
//      puzzles.Add(puzzle3);
//      puzzles.Add(puzzle4);

		corePuzzles = Util.CreateList ( puzzle1, puzzle2, puzzle3, puzzle4 );

		ActivatePuzzle1 ();

		UpdateTimeText ();

	}

	private void HandleMinuteTimer ()
	{
		CalculateSlowdown ();
		TickTimeRemaining ();
		UpdateTimeText ();
	}

	//Should be called regularly during the last segment.
	//Relies on corePercentComplete to continue being updated  during the last segment.
	public void CalculateSlowdown ()
	{
		//if( !onLastSegment )
		//return

		float maxSlowdown = 0.5; //Maximum we will let ourselves change the speed of a minute (to stop it being noticeable)
		//Percent slowdown.  eg 1 = twice as slow.  0.5 = half as slow.  0.1 = 10% slower.  2=200% (4 times) slower .

		int maxDrift = 10; //Maximum extra minutes we will allow players
		int timeBehind = -GetTimeAhead ();

		//We're doing OK
		if (GetTimeAhead () >= 0)
		{
			slowdown = 1; //TODO - smooth this out over time.  So when players catch up to time at the end
			//they get a little more slowing and don't just back to real time.
		}
		else //We're struggling  eg 7 mins behind     or 3 mins behind
		{
			
			float percentOut = timeBehind / maxDrift; // eg 0.7 == 70%  or 0.3 == 30%
			slowdown = percentOut * maxSlowdown; //eg 0.35   or  0.15

			if (slowdown > maxSlowdown)
				slowdown = maxSlowdown;

		}
	}

	//Called every minute
	private void TickTimeRemaining ()
	{
		actualTimeRemaining--;

		//By default keep up with real time
		if (slowdown == 0)
		{
			displayTimeRemaining--;
		}
		else if (slowdown > 0)
		{
			displayTimeRemaining -= ( 1 - 1 * slowdown );
		}
		else
		{
			Util.JLogErr ( "NEGATIVE SLOWDOWN " + slowdown );
			displayTimeRemaining--;
		}
		
	}

	private void UpdateTimeText ()
	{
		timeText.text = "Time: " + actualTimeRemaining + " (" + displayTimeRemaining + ")" + " Puzzles: " + corePuzzleStep + "/" + totalCorePuzzleSteps + " (" + corePercentComplete + "%)    They are " + GetTimeAhead () + "m " + GetTimeAhead () > 0 ? "Ahead" : "Behind";
	}

	private void MoveStepAndPercent ( int importance )
	{
		corePuzzleStep++;

		//Get absolure importance for the percentage complete metric
		int importanceStep = 100 / totalCorePuzzleImportance;
		corePercentComplete += ( importance * importanceStep );

		UpdateTimeText ();//These changes affect the TimeText
	}

	//Tells us how far ahead of time the team is. minutes.
	//Will jump when corePercentComplete updates (ie when  puzzles are completed)
	public int GetTimeAhead ()
	{
		//Calculate how long it should have tagen to get here
		int shouldHaveTaken = ( corePercentComplete / 100 ) * startingTime; //mins
		int actuallyTook = startingTime - actualTimeRemaining;

		return shouldHaveTaken - actuallyTook;
	}

	public void GiveHint ()
	{
		hintButton.interactable = false;

		//Trigger Hint
		Util.JLog ( "Giving a Hint" );
		//TODO

		hintButton.interactable = true;
	}

	public void OnDifficultyChange ()
	{
		if (getDifficulty == 4)
			CueInserterPuzzle ();
	}

	public void ThrowInstantPuzzle ( Puzzle puzzle = null )
	{
		throwInPuzzleButton.interactable = false;
		if (!puzzle)
			puzzle = SelectInstantPuzzle ();

		puzzle.Activate ();
	}

	//
	public void CueInserterPuzzle ()
	{
		cueInserterPuzzleButton.interactable = false;
		inserterPuzzleNext = true;
		cancelInserterPuzzleButton.interactable = true;

	}
	public void CancelInserterPuzzle() {
		cancelInserterPuzzleButton.interactable = false;
		inserterPuzzleNext = false;
		cueInserterPuzzleButton.interactable = true;
	}

	private void SelectInstantPuzzle ()
	{
		return giantBomb; //TODO
	}







	////PUZZLE COMMON
	public struct Puzzle
	{
		
		
		public Puzzle ( int number, int importance,
				Action activate, Action complete, Action corePuzzleCallback = null )
		{
			Number = number;
			Importance = importance;

			Started = false;
			Completed = false;

			Activate = activate;
			Complete = complete;

			CorePuzzleCallback = corePuzzleCallback;

			totalCorePuzzleImportance += importance;
			totalCorePuzzleSteps++;
//          puzzles.Add(this);
		}

		public int Number;
		//Core puzzle number.  0 for dynamic puzzles
		public int Importance;
		//A value other than 1 indicates how much this puzzles completion affects total percentage of game.
		public Action Activate;
		public Action Complete;
		public bool Started;
		public bool Completed;
		public Action CorePuzzleCallback;
	};


	//Common actions on Core puzzles.
	public void OnActivateAnyPuzzle ( Puzzle puzzle )
	{
		puzzle.Started = true;
	}

	public void OnCompleteCorePuzzle ( Puzzle puzzle )
	{
		puzzle.Completed = true;
		MoveStepAndPercent ( puzzle.Importance );
	}
	public void OnCompleteThrowInPuzzle ( Puzzle puzzle )
	{
		puzzle.Completed = true;
		throwInPuzzleButton.interactable = true;
	}
	public void OnActivateInserterPuzzle(Puzzle puzzle) {
		cueInserterPuzzleButton.interactable = false;
		cancelInserterPuzzleButton.interactable = false;
	}
	public void OnCompleteInserterPuzzle(Puzzle puzzle)
	{
		puzzle.Completed = true;
		cueInserterPuzzleButton.interactable = true;
		cancelInserterPuzzleButton.interactable = false;

		//Move back to where we were in the Core Puzzle Chain
		puzzle.CorePuzzleCallback ();
	}




	//// THROW IN PUZZLES
	public Puzzle giantBomb = new Puzzle ( 0, 0, ActivatePuzzleGiantBomb, CompletePuzzleGiantBomb );

	public void ActivatePuzzleGiantBomb ( int numberOfBombs = 1 )
	{
//		if (numberOfUnlockedRooms == 1)
//			return; //Don't be cruel.

		OnActivateAnyPuzzle ();
//		
//		if (numberOfBombs >= numberOfUnlockedRooms)
//			numberOfBombs = numberOfUnlockedRooms - 1;
		Util.JLog ( "Throwing in " + numberOfBombs + " Bomb" + numberOfBombs > 1 ? "s" : "" + "!" );

		//TODO Do the actual work.
	}

	public void CompletePuzzleGiantBomb ()
	{
		OnCompleteThrowInPuzzle ();
		//Clean-Up 
	}






	//// INSERTER PUZZLES
	public Puzzle inserterPuzzleTest = new Puzzle(0, 0, ActivateInserterPuzzleTest, CompleteInserterPuzzleTest);
	public void ActivateInserterPuzzleTest() {
		OnActivateInserterPuzzle ();
		//Set-up Puzzle Gameobjects
	}
	public void CompleteInserterPuzzleTest() {
		OnCompleteInserterPuzzle ();
		//Clean Up
	}







	//// CORE PUZZLES

	//Wood Search
	public Puzzle puzzle1 = new Puzzle ( 1, 5, ActivatePuzzle1, CompletePuzzle1 );

	public void ActivatePuzzle1 ()
	{
		OnActivateAnyPuzzle ( puzzle1 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle1 ()
	{
		OnCompleteCorePuzzle ( puzzle1 );
		//Animate & Change any Gameobjects...

		if (inserterPuzzleNext)
		{
			inserterPuzzleTest.CorePuzzleCallback = ActivatePuzzles ( puzzle2, puzzle3 );
			inserterPuzzleTest.Activate ();
		}
		else
		{
			ActivatePuzzles ( puzzle2, puzzle3 );
		}
	}

	public void ActivatePuzzles ( Puzzle p1, Puzzle p2 = null, Puzzle puzzle3 = null ) {
		
	}


	//Wood Puzzle XXX
	public Puzzle puzzle2 = new Puzzle ( 2, 1, ActivatePuzzle2, CompletePuzzle2 );

	public void ActivatePuzzle2 ()
	{
		OnActivateAnyPuzzle ( puzzle2 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle2 ()
	{
		OnCompleteCorePuzzle ( puzzle2 );

		//Animate & Change any Gameobjects...

		if (puzzle3.Completed)
		{
			ActivatePuzzle4 ();
		}
	}


	//Wood Puzzle YYY
	public Puzzle puzzle3 = new Puzzle ( 3, 1, ActivatePuzzle3, CompletePuzzle3 );

	public void ActivatePuzzle3 ()
	{
		OnActivateAnyPuzzle ( puzzle3 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle3 ()
	{
		OnCompleteCorePuzzle ( puzzle3 );

		//Animate & Change any Gameobjects...

		if (puzzle2.Completed)
		{
			ActivatePuzzle4 ();
		}
	}


	public Puzzle puzzle4 = new Puzzle ( 3, 3, ActivatePuzzle4, CompletePuzzle4 );

	public void ActivatePuzzle4 ()
	{
		OnActivateAnyPuzzle ( puzzle4 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle4 ()
	{
		OnCompleteCorePuzzle ( puzzle1 );

		//Animate & Change any Gameobjects...
	}







}
