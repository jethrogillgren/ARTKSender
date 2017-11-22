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
	//eg 1 = twice as slow.  0.5 = half as slow.  0.1 = 10% slower.  1= stopped.
	public float slowdown = 0.0f;
	public bool allowSlowdownTime = false;
//set to true when we are in the last section of gameplay and want to fudge the finish countdown.


	//Each core puzzle gives a step.    Order completed is partly variable.

	public float corePercentComplete = 0;
	//100 is completed game.
	public int corePuzzleStep = 0;
	//Each core puzzle gives a step.    Order completed is partly variable


	//These lists are manually populated in Start().
	public List<Puzzle> allPuzzles;
	public List<Puzzle> corePuzzles;
	public List<Puzzle> inserterPuzzles;
	public List<Puzzle> throwInPuzzles;
	public List<Puzzle> currentPuzzles;
	//TODO plural?

	public bool inserterPuzzleNext = false;

	public  int totalCorePuzzleSteps = 0;
	private float totalCorePuzzleImportance = 0;

	private float totalScore = 0;
	//The current score.  Uses importance and time remaining
	private float totalPuzzleImportance = 0;
//The max score possible

	//// GUI refs
	public Button throwInPuzzleButton;
	public Slider difficultySlider;
	// 0-4.    0 Is easiest.    4 is every extra puzzle. 1/3 is some extra puzzles. 2 is the default.
	public int GetDifficulty ()
	{
		return ( int )Math.Ceiling ( difficultySlider.value );//Locked to whole numbers anyway
	}

	public Button hintButton;
	public Text timeText;
	public Button cueInserterPuzzleButton;
	public Button cancelInserterPuzzleButton;

	public Button debugDoPuzzleButton;

	public Puzzle giantBombPuzzle;
	public Puzzle inserterTestPuzzle;
	public Puzzle puzzle1;
	public Puzzle puzzle2;
	public Puzzle puzzle3;
	public Puzzle puzzle4;




	public void OnDisable ()
	{
		if (IsInvoking ( "HandleMinuteTimer" ))
			CancelInvoke ( "HandleMinuteTimer" );
	}

	public override void OnStartServer ()
	{
		if (!hasAuthority)
			return;
            
		//Adds a listener to the main slider and invokes a method when the value changes.
		throwInPuzzleButton = ( Button )GameObject.Find ( "Throw In Puzzle" ).GetComponent<Button> ();
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
		debugDoPuzzleButton = ( Button )GameObject.Find ( "Debug Do Puzzle" ).GetComponent<Button> ();
		debugDoPuzzleButton.onClick.AddListener ( delegate
			{
				{
					DebugDoPuzzle ();
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

		giantBombPuzzle = new Puzzle ( "GiantBomb", 0, 0, ActivatePuzzleGiantBomb, CompletePuzzleGiantBomb );
		inserterTestPuzzle = new Puzzle ( "InserterTest", 0, 0, ActivateInserterPuzzleTest, CompleteInserterPuzzleTest );
		puzzle1 = new Puzzle ( "Core1", 1, 5, ActivatePuzzle1, CompletePuzzle1 );
		puzzle2 = new Puzzle ( "Core2", 2, 1, ActivatePuzzle2, CompletePuzzle2 );
		puzzle3 = new Puzzle ( "Core3", 3, 1, ActivatePuzzle3, CompletePuzzle3 );
		puzzle4 = new Puzzle ( "Core4", 3, 3, ActivatePuzzle4, CompletePuzzle4 );

		corePuzzles = Util.CreateList ( puzzle1, puzzle2, puzzle3, puzzle4 );
		inserterPuzzles = Util.CreateList ( inserterTestPuzzle );
		throwInPuzzles = Util.CreateList ( giantBombPuzzle );

		allPuzzles = new List<Puzzle> ( corePuzzles );
		currentPuzzles = new List<Puzzle> ();

		foreach ( Puzzle p in corePuzzles )
		{
			totalCorePuzzleImportance += p.Importance;
			totalCorePuzzleSteps++;
			allPuzzles.Add ( p );
		}
		foreach ( Puzzle p in inserterPuzzles )
		{
			allPuzzles.Add ( p );
		}
		foreach ( Puzzle p in throwInPuzzles )
		{
			allPuzzles.Add ( p );
		}
		foreach ( Puzzle p in allPuzzles )
			totalPuzzleImportance += p.Importance;
		

		ActivatePuzzle1 ();

		UpdateTimeText ();

		InvokeRepeating ( "HandleMinuteTimer", 1, 1 );// Seconds

	}

	//Debug, used for testing the puzzle code before client testing was doable.
	private void DebugDoPuzzle ()
	{
		if (currentPuzzles.Count < 1)
			return;
		
		Util.JLog ( "Debug Doing puzzle: " + currentPuzzles [ 0 ].Name );
		currentPuzzles [ currentPuzzles.Count - 1 ].Complete ();
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

		float maxSlowdown = 0.5f; //see slowdown.  Maximum we will let ourselves change the speed of a minute (to stop it being noticeable)

		int maxDrift = 10; //Maximum extra minutes we will allow players
		float timeBehind = -GetTimeAhead ();

		//We're doing OK
		if (!allowSlowdownTime || GetTimeAhead () >= 0)
		{
			slowdown = 0f; //TODO - smooth this out over time.  So when players catch up to time at the end
			//they get a little more slowing and don't jump back to real time.
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
		string tmp = "Time: " + actualTimeRemaining + " (" + displayTimeRemaining + ") " + "   They are " + Math.Abs ( GetTimeAhead () ) + "m " + ( GetTimeAhead () > 0 ? "Ahead" : "Behind" ) + " \n"
		             + "Puzzles: " + corePuzzleStep + "/" + totalCorePuzzleSteps + " (" + corePercentComplete + "%) \n"
		             + "Current: ";
		foreach ( Puzzle p in currentPuzzles )
			tmp += p.Name + "  ";

		timeText.text = tmp;
	}

	private void MoveStepAndPercent ( float importance )
	{
		//Don't move step if we are finishing
		if (corePuzzleStep < totalCorePuzzleSteps)
			corePuzzleStep++;

		//Get absolure importance for the percentage complete metric
		float importanceStep = 100 / totalCorePuzzleImportance;
		corePercentComplete += ( importance * importanceStep );

		//Extra check
		if (corePercentComplete > 100)
			corePercentComplete = 100;

		UpdateTimeText ();//These changes affect the TimeText
	}

	//Tells us how far ahead of time the team is. minutes.
	//Will jump when corePercentComplete updates (ie when  puzzles are completed)
	public float GetTimeAhead ()
	{
		//Calculate how long it should have tagen to get here
		float shouldHaveTaken = ( corePercentComplete / 100f ) * startingTime; //mins it should have taken
		float actuallyTook = startingTime - actualTimeRemaining;

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
		if (GetDifficulty () == 4)
			CueInserterPuzzle ();
	}

	public void ThrowInstantPuzzle ()
	{
		Puzzle selected = SelectInstantPuzzle ();
		ThrowInstantPuzzle ( ref selected );
	}

	public void ThrowInstantPuzzle ( ref Puzzle puzzle )
	{
		throwInPuzzleButton.interactable = false;
		puzzle.Activate ();
	}

	//
	public void CueInserterPuzzle ()
	{
		cueInserterPuzzleButton.interactable = false;
		inserterPuzzleNext = true;
		cancelInserterPuzzleButton.interactable = true;

	}

	public void CancelInserterPuzzle ()
	{
		cancelInserterPuzzleButton.interactable = false;
		inserterPuzzleNext = false;
		cueInserterPuzzleButton.interactable = true;
	}

	private Puzzle SelectInstantPuzzle ()
	{
		return giantBombPuzzle; //TODO
	}







	////PUZZLE COMMON
	//This object defines each puzzle (core, inserter and throw-in).  Each puzzle self-contains it's own different difficulty versions.
	//New instances must be manually added to the List<Puzzle> objects
	public struct Puzzle
	{
		public Puzzle ( string name, int number, int importance,
		                Action activate, Action complete, Action corePuzzleCallback = null )
		{
			Name = name;
			Number = number;
			Importance = importance;

			Started = false;
			Completed = false;

			Activate = activate;
			Complete = complete;

			CorePuzzleCallback = corePuzzleCallback;

//          puzzles.Add(this);
		}

		public String Name;
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
	public void OnActivateAnyPuzzle ( ref Puzzle puzzle )
	{
		puzzle.Started = true;
		currentPuzzles.Add ( puzzle );
	}

	public void OnActivateInserterPuzzle ( ref Puzzle puzzle )
	{
		cueInserterPuzzleButton.interactable = false;
		cancelInserterPuzzleButton.interactable = false;
		OnActivateAnyPuzzle ( ref  puzzle );
	}


	public void OnCompleteAnyPuzzle ( ref Puzzle puzzle )
	{
		puzzle.Completed = true;

		//		currentPuzzles.Remove (  puzzle ); TODO this needed replacing with the below???
		foreach ( Puzzle p in currentPuzzles )
		{
			if (p.Name == puzzle.Name)
			{
				currentPuzzles.Remove ( p );
				break;
			}
		}

		totalScore += puzzle.Importance;

		UpdateTimeText ();
	}

	public void OnCompleteCorePuzzle ( ref Puzzle puzzle )
	{
		MoveStepAndPercent ( puzzle.Importance );
		OnCompleteAnyPuzzle ( ref puzzle );
	}

	public void OnCompleteThrowInPuzzle ( ref Puzzle puzzle )
	{
		OnCompleteAnyPuzzle ( ref puzzle );
		throwInPuzzleButton.interactable = true;
	}

	public void OnCompleteInserterPuzzle ( ref Puzzle puzzle )
	{
		OnCompleteAnyPuzzle ( ref puzzle );
		cueInserterPuzzleButton.interactable = true;
		cancelInserterPuzzleButton.interactable = false;

		//Move back to where we were in the Core Puzzle Chain
		puzzle.CorePuzzleCallback ();
	}




	//// THROW IN PUZZLES

	public void ActivatePuzzleGiantBomb ()
	{
		ActivatePuzzleGiantBomb ( 1 );
	}
	//TODO shouldn't be needed but the Action delegate needs sortinh with parameters

	public void ActivatePuzzleGiantBomb ( int numberOfBombs )
	{
//		if (numberOfUnlockedRooms == 1)
//			return; //Don't be cruel.

		OnActivateAnyPuzzle ( ref giantBombPuzzle );
//		
//		if (numberOfBombs >= numberOfUnlockedRooms)
//			numberOfBombs = numberOfUnlockedRooms - 1;
		Util.JLog ( "Throwing in " + numberOfBombs + " Bomb" + ( numberOfBombs > 1 ? "s" : "" ) + "!" );

		//TODO Do the actual work.
	}

	public void CompletePuzzleGiantBomb ()
	{
		OnCompleteThrowInPuzzle ( ref giantBombPuzzle );
		//Clean-Up 
	}






	//// INSERTER PUZZLES
	public void ActivateInserterPuzzleTest ()
	{
		OnActivateInserterPuzzle ( ref inserterTestPuzzle );
		//Set-up Puzzle Gameobjects
	}

	public void CompleteInserterPuzzleTest ()
	{
		OnCompleteInserterPuzzle ( ref inserterTestPuzzle );
		//Clean Up
	}







	//// CORE PUZZLES

	//Wood Search

	public void ActivatePuzzle1 ()
	{
		OnActivateAnyPuzzle ( ref puzzle1 );
		//Set-up Puzzle Gameobjects

		if (GetDifficulty () > 3)
		{
			//Do the hard version
		}
	}

	public void CompletePuzzle1 ()
	{
		OnCompleteCorePuzzle ( ref puzzle1 );
		//Animate & Change any Gameobjects...

		if (inserterPuzzleNext)
		{
			inserterTestPuzzle.CorePuzzleCallback = ActivatePuzzles2And3;
			inserterTestPuzzle.Activate ();
		}
		else
		{
			ActivatePuzzles ( ref puzzle2, ref puzzle3 );
		}
	}

	//TODO these are not working as Action delegates - I need to work out how to pass parameters (Activate and Complete callbacks)
	public void ActivatePuzzles ( ref Puzzle p1, ref Puzzle p2 )
	{
		p1.Activate ();
		p2.Activate ();
	}

	public void ActivatePuzzles ( ref Puzzle p1, ref Puzzle p2, ref Puzzle p3 )
	{
		p1.Activate ();
		p2.Activate ();
		p3.Activate ();
	}

	public void ActivatePuzzles2And3 ()
	{
		ActivatePuzzles ( ref puzzle2, ref puzzle3 );
	}


	//Wood Puzzle XXX

	public void ActivatePuzzle2 ()
	{
		OnActivateAnyPuzzle ( ref puzzle2 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle2 ()
	{
		OnCompleteCorePuzzle ( ref puzzle2 );

		//Animate & Change any Gameobjects...

		if (puzzle3.Completed)
		{
			Debug.Log ( "Puzzle 3 also done, so moving to 4" );

			ActivatePuzzle4 ();
		}
		else
		{
			Debug.Log ( "Waiting for Puzzle 3 to complete before moving to 4" );

		}
	}


	//Wood Puzzle YYY

	public void ActivatePuzzle3 ()
	{
		OnActivateAnyPuzzle ( ref puzzle3 );
		//Set-up Puzzle Gameobjects
	}

	public void CompletePuzzle3 ()
	{
		OnCompleteCorePuzzle ( ref puzzle3 );

		//Animate & Change any Gameobjects...

		if (puzzle2.Completed)
		{
			Debug.Log ( "Puzzle 2 also done, so moving to 4" );

			ActivatePuzzle4 ();
		}
		else
		{
			Debug.Log ( "Waiting for Puzzle 2 to complete before moving to 4" );
		}
	}



	public void ActivatePuzzle4 ()
	{
		OnActivateAnyPuzzle ( ref puzzle4 );
		//Set-up Puzzle Gameobjects
		allowSlowdownTime = true;
	}

	public void CompletePuzzle4 ()
	{
		OnCompleteCorePuzzle ( ref puzzle4 );

		//Animate & Change any Gameobjects...
		WinGame ();
	}





	private void WinGame ()
	{
		if (IsInvoking ( "HandleMinuteTimer" ))
			CancelInvoke ( "HandleMinuteTimer" );

		if (actualTimeRemaining > 0)
			totalScore += actualTimeRemaining;

		Debug.LogError ( "WINNER with " + totalScore + " points" );
	}

}
