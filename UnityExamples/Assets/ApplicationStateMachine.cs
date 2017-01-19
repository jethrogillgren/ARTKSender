using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public enum ProcessState
{
	//Main Cycle
	Inactive, //Disconnected from Tango, Start point or paused
	Active, //Connected to Tango.
	Localised, //Connected and Localised to scene - Main gameplay state
	Terminated //Finished
}

public enum Command
{
	//Main Cycle
	Connect,
	Disconnect,
	Localise,
	Unlocalise,
	Exit
}


public class ApplicationStateMachine {

	/*
	
	Inactive +-----+
	 + ^   ^       |
	 | |   +----+  |
	 v +        |  |
	Active <--+ |  |
	 +        | |  |
	 |        | |  |
	 v        | |  |
	Localised +-+  |
	               |
	               |
	               |
	Terminated <---+

	*/


	Dictionary<StateTransition, ProcessState> transitions;
	public ProcessState CurrentState { get; private set; }

	private bool permissionsGranted = false;
	private bool firstLocalisationHappened = false;

	public ApplicationStateMachine()
	{
		CurrentState = ProcessState.Inactive;
		Debug.Log ("J# State Machine [" + CurrentState + "]" );

		transitions = new Dictionary<StateTransition, ProcessState>
		{
			{ new StateTransition(ProcessState.Inactive, Command.Exit), ProcessState.Terminated },
			{ new StateTransition(ProcessState.Inactive, Command.Connect), ProcessState.Active },
			{ new StateTransition(ProcessState.Active, Command.Disconnect), ProcessState.Inactive },
			{ new StateTransition(ProcessState.Active, Command.Localise), ProcessState.Localised },
			{ new StateTransition(ProcessState.Localised, Command.Unlocalise), ProcessState.Active },
			{ new StateTransition(ProcessState.Localised, Command.Disconnect), ProcessState.Inactive }
		};
	}

	public ProcessState GetNext(Command command)
	{
		StateTransition transition = new StateTransition(CurrentState, command);
		ProcessState nextState;
		if (!transitions.TryGetValue(transition, out nextState))
			throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
		return nextState;
	}

	public ProcessState MoveNext(Command command)
	{
		Debug.Log ("J# State Machine [" + CurrentState + "] --" + command + "--> [" + GetNext(command) + "]" );
		CurrentState = GetNext(command);
		return CurrentState;
	}

	public bool isTangoConnected() {
		if (CurrentState == ProcessState.Active || CurrentState == ProcessState.Localised)
			return true;
		return false;
	}

	public bool isPermitted() {
		return permissionsGranted;
	}
	public void setPermitted() {
		permissionsGranted = true;
	}

	public bool hasFirstLocalisedHappened() {
		return firstLocalisationHappened;
	}
	public void setFirstLocalised(){
		firstLocalisationHappened = true;
	}




	class StateTransition
	{
		readonly ProcessState CurrentState;
		readonly Command Command;

		public StateTransition(ProcessState currentState, Command command)
		{
			CurrentState = currentState;
			Command = command;
		}

		public override int GetHashCode()
		{
			return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			StateTransition other = obj as StateTransition;
			return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
		}
	}
}
